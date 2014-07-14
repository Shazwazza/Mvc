// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Mvc.DecisionTree;
using System.Globalization;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultActionSelector : IActionSelector
    {
        private readonly IActionDescriptorsCollectionProvider _actionDescriptorsCollectionProvider;
        private readonly IActionBindingContextProvider _bindingProvider;
        private ActionSelectionDecisionTree _tree;

        public DefaultActionSelector(IActionDescriptorsCollectionProvider actionDescriptorsCollectionProvider,
                                     IActionBindingContextProvider bindingProvider)
        {
            _actionDescriptorsCollectionProvider = actionDescriptorsCollectionProvider;
            _bindingProvider = bindingProvider;
        }

        public async Task<ActionDescriptor> SelectAsync2([NotNull] RouteContext context)
        {
            var allDescriptors = GetActions();

            var matching = allDescriptors.Where(ad => Match(ad, context)).ToList();

            var matchesWithConstraints = new List<ActionDescriptor>();
            foreach (var match in matching)
            {
                if (match.DynamicConstraints != null && match.DynamicConstraints.Any() ||
                    match.MethodConstraints != null && match.MethodConstraints.Any())
                {
                    matchesWithConstraints.Add(match);
                }
            }

            // If any action that's applicable has constraints, this is considered better than 
            // an action without.
            if (matchesWithConstraints.Any())
            {
                matching = matchesWithConstraints;
            }

            if (matching.Count == 0)
            {
                return null;
            }
            else
            {
                return await SelectBestCandidate(context, matching);
            }
        }

        public async Task<ActionDescriptor> SelectAsync([NotNull] RouteContext context)
        {
            var tree = GetDecisionTree();
            var matching = tree.Select(context.RouteData.Values);

            if (matching.Count == 0)
            {
                return null;
            }

            // If any action that's applicable has constraints, this is considered better than 
            // an action without.
            var seenConstraint = false;
            var bestMatches = new List<ActionDescriptor>();
            for (var i = 0; i < matching.Count; i++)
            {
                var action = matching[i];

                var hasConstraint =
                    action.DynamicConstraints != null && action.DynamicConstraints.Count > 0 ||
                    action.MethodConstraints != null && action.MethodConstraints.Count > 0;

                if (hasConstraint)
                {
                    seenConstraint = true;

                    var isConstraintFailed = false;
                    if (action.DynamicConstraints != null)
                    {
                        for (var j = 0; j < action.DynamicConstraints.Count; j++)
                        {
                            var constraint = action.DynamicConstraints[j];
                            if (!constraint.Accept(context))
                            {
                                isConstraintFailed = true;
                                break;
                            }
                        }
                    }

                    if (action.MethodConstraints != null && !isConstraintFailed)
                    {
                        for (var j = 0; j < action.MethodConstraints.Count; j++)
                        {
                            var constraint = action.MethodConstraints[j];
                            if (!constraint.Accept(context))
                            {
                                isConstraintFailed = true;
                                break;
                            }
                        }
                    }

                    if (!isConstraintFailed)
                    {
                        bestMatches.Add(action);
                    }
                }
                else if (seenConstraint)
                {
                    break;
                }
                else
                {
                    bestMatches = matching;
                    break;
                }
            }

            return await SelectBestCandidate(context, bestMatches);
        }

        public bool Match(ActionDescriptor descriptor, RouteContext context)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }

            return (descriptor.RouteConstraints == null ||
                        descriptor.RouteConstraints.All(c => c.Accept(context))) &&
                   (descriptor.MethodConstraints == null ||
                        descriptor.MethodConstraints.All(c => c.Accept(context))) &&
                   (descriptor.DynamicConstraints == null ||
                        descriptor.DynamicConstraints.All(c => c.Accept(context)));
        }

        protected virtual async Task<ActionDescriptor> SelectBestCandidate(
            RouteContext context,
            List<ActionDescriptor> candidates)
        {
            var applicableCandiates = new List<ActionDescriptorCandidate>();
            foreach (var action in candidates)
            {
                var isApplicable = true;
                var candidate = new ActionDescriptorCandidate()
                {
                    Action = action,
                };

                var actionContext = new ActionContext(context, action);
                var actionBindingContext = await _bindingProvider.GetActionBindingContextAsync(actionContext);

                foreach (var parameter in action.Parameters.Where(p => p.ParameterBindingInfo != null))
                {
                    if (!ValueProviderResult.CanConvertFromString(parameter.ParameterBindingInfo.ParameterType))
                    {
                        continue;
                    }

                    if (await actionBindingContext.ValueProvider.ContainsPrefixAsync(
                        parameter.ParameterBindingInfo.Prefix))
                    {
                        candidate.FoundParameters++;
                        if (parameter.IsOptional)
                        {
                            candidate.FoundOptionalParameters++;
                        }
                    }
                    else if (!parameter.IsOptional)
                    {
                        isApplicable = false;
                        break;
                    }
                }

                if (isApplicable)
                {
                    applicableCandiates.Add(candidate);
                }
            }

            if (applicableCandiates.Count == 0)
            {
                return null;
            }

            var mostParametersSatisfied =
                applicableCandiates
                .GroupBy(c => c.FoundParameters)
                .OrderByDescending(g => g.Key)
                .First();

            var fewestOptionalParameters =
                mostParametersSatisfied
                .GroupBy(c => c.FoundOptionalParameters)
                .OrderBy(g => g.Key).First()
                .ToArray();

            if (fewestOptionalParameters.Length > 1)
            {
                throw new InvalidOperationException("The actions are ambiguious.");
            }

            return fewestOptionalParameters[0].Action;
        }

        // This method attempts to ensure that the route that's about to generate a link will generate a link
        // to an existing action. This method is called by a route (through MvcApplication) prior to generating
        // any link - this gives WebFX a chance to 'veto' the values provided by a route.
        //
        // This method does not take httpmethod or dynamic action constraints into account.
        public virtual bool HasValidAction([NotNull] VirtualPathContext context)
        {
            if (context.ProvidedValues == null)
            {
                // We need the route's values to be able to double check our work.
                return false;
            }

            var actions =
                GetActions().Where(
                    action =>
                        action.RouteConstraints == null ||
                        action.RouteConstraints.All(constraint => constraint.Accept(context.ProvidedValues)));

            return actions.Any();
        }

        private IReadOnlyList<ActionDescriptor> GetActions()
        {
            var descriptors = _actionDescriptorsCollectionProvider.ActionDescriptors;

            if (descriptors == null)
            {
                throw new InvalidOperationException(
                    Resources.FormatPropertyOfTypeCannotBeNull("ActionDescriptors",
                                                               _actionDescriptorsCollectionProvider.GetType()));
            }

            return descriptors.Items;
        }

        private ActionSelectionDecisionTree GetDecisionTree()
        {
            var descriptors = _actionDescriptorsCollectionProvider.ActionDescriptors;
            if (descriptors == null)
            {
                throw new InvalidOperationException(
                    Resources.FormatPropertyOfTypeCannotBeNull("ActionDescriptors",
                                                               _actionDescriptorsCollectionProvider.GetType()));
            }

            if (_tree == null || descriptors.Version != _tree.Version)
            {
                _tree = new ActionSelectionDecisionTree(descriptors);
            }

            return _tree;
        }

        private class ActionDescriptorCandidate
        {
            public ActionDescriptor Action { get; set; }

            public int FoundParameters { get; set; }

            public int FoundOptionalParameters { get; set; }
        }

        private class ActionSelectionDecisionTree
        {
            private readonly DecisionTreeNode<ActionDescriptor, object> _root;

            public ActionSelectionDecisionTree(ActionDescriptorsCollection actions)
            {
                Version = actions.Version;

                var fullTree = DecisionTreeBuilder<ActionDescriptor, object>.GenerateTree(actions.Items, new ActionDescriptorClassifier());
                _root = DecisionTreeBuilder<ActionDescriptor, object>.Optimize(fullTree, new HashSet<ActionDescriptor>());
            }

            public int Version { get; private set; }

            public List<ActionDescriptor> Select(IDictionary<string, object> routeValues)
            {
                var results = new List<ActionDescriptor>();
                Walk(results, routeValues, _root);
                results.Sort(new ActionDescriptorComparer());
                return results;
            }

            private void Walk(List<ActionDescriptor> results, IDictionary<string, object> routeValues, DecisionTreeNode<ActionDescriptor, object> node)
            {
                for (int i = 0; i < node.Matches.Count; i++)
                {
                    results.Add(node.Matches[i].Item);
                }

                for (int i = 0; i < node.Criteria.Count; i++)
                {
                    var criterion = node.Criteria[i];
                    var key = criterion.Key;

                    object value;
                    routeValues.TryGetValue(key, out value);

                    DecisionTreeNode<ActionDescriptor, object> branch;
                    if (criterion.Branches.TryGetValue(value ?? string.Empty, out branch))
                    {
                        Walk(results, routeValues, branch);
                    }
                }
            }
        }

        private class ActionDescriptorComparer : IComparer<ActionDescriptor>
        {
            public int Compare(ActionDescriptor x, ActionDescriptor y)
            {
                var scoreX = GetScore(x);
                var scoreY = GetScore(y);

                return scoreX.CompareTo(scoreY);
            }

            private static int GetScore(ActionDescriptor actionDescriptor)
            {
                return
                    (actionDescriptor.DynamicConstraints != null && actionDescriptor.DynamicConstraints.Count > 0 ||
                    actionDescriptor.MethodConstraints != null && actionDescriptor.MethodConstraints.Count > 0) ? 0 : 1;
            }
        }

        private class ActionDescriptorClassifier : IClassifier<ActionDescriptor, object>
        {
            public ActionDescriptorClassifier()
            {
                ValueComparer = new RouteValueEqualityComparer();
            }

            public IEqualityComparer<object> ValueComparer { get; private set; }

            public IDictionary<string, object> GetCriteria(ActionDescriptor item)
            {
                return item.RouteConstraints == null ?
                    new Dictionary<string, object>() :
                    item.RouteConstraints.ToDictionary<RouteDataActionConstraint, string, object>(rc => rc.RouteKey, rc => rc.RouteValue ?? string.Empty);
            }
        }

        private class RouteValueEqualityComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y)
            {
                var stringX = x as string ?? Convert.ToString(x, CultureInfo.InvariantCulture);
                var stringY = y as string ?? Convert.ToString(y, CultureInfo.InvariantCulture);

                if (string.IsNullOrEmpty(stringX) && string.IsNullOrEmpty(stringY))
                {
                    return true;
                }
                else
                {
                    return string.Equals(stringX, stringY, StringComparison.OrdinalIgnoreCase);
                }
            }

            public int GetHashCode(object obj)
            {
                var stringObj = obj as string ?? Convert.ToString(obj, CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(stringObj))
                {
                    return StringComparer.OrdinalIgnoreCase.GetHashCode(string.Empty);
                }
                else
                {
                    return StringComparer.OrdinalIgnoreCase.GetHashCode(stringObj);
                }
            }
        }
    }
}
