using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.Mvc.DecisionTree
{
    public static class DecisionTreeBuilder<T, U>
    {
        public static DecisionTreeNode<T, U> GenerateTree(IReadOnlyList<T> items, IClassifier<T, U> classifer)
        {
            var itemDescriptors = new List<ItemDescriptor<T, U>>();
            for (int i = 0; i < items.Count; i++)
            {
                itemDescriptors.Add(new ItemDescriptor<T, U>()
                {
                    Criteria = classifer.GetCriteria(items[i]),
                    Index = i,
                    Item = items[i],
                });
            }

            return GenerateNode(default(U), itemDescriptors, classifer.ValueComparer);
        }

        private static DecisionTreeNode<T, U> GenerateNode(U value, IList<ItemDescriptor<T, U>> items, IEqualityComparer<U> comparer)
        {
            var criteria = items
                .Aggregate(
                    Enumerable.Empty<string>(),
                    (agg, item) => item.Criteria.Keys.Union(agg),
                    agg => agg.Select(c => new DecisionCriterion<T, U>() { Key = c }))
                .ToList();

            foreach (var criterion in criteria)
            {
                var branches = items
                    .Where(item => item.Criteria.ContainsKey(criterion.Key))
                    .Aggregate(
                        Enumerable.Empty<U>(),
                        (agg, item) => Enumerable.Repeat(item.Criteria[criterion.Key], 1).Union(agg));

                criterion.Branches = new Dictionary<U, DecisionTreeNode<T, U>>(comparer);
                foreach (var branch in branches)
                {
                    var allMatches = items.Where(item =>
                    {
                        return item.Criteria.ContainsKey(criterion.Key) && comparer.Equals(item.Criteria[criterion.Key], branch);
                    })
                    .Select(item => Reduce(item, criterion.Key)).ToList();

                    criterion.Branches.Add(branch, GenerateNode(branch, allMatches, comparer));
                }
            }

            return new DecisionTreeNode<T, U>()
            {
                Criteria = criteria,
                Matches = items.Where(item => item.Criteria.Count == 0).ToList(),
                Value = value,
            };
        }

        private static ItemDescriptor<T, U> Reduce(ItemDescriptor<T, U> item, string key)
        {
            var reduced = item.Criteria
                .Where(c => !c.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return new ItemDescriptor<T, U>()
            {
                Criteria = reduced,
                Index = item.Index,
                Item = item.Item,
            };
        }

        public static DecisionTreeNode<T, U> Optimize(DecisionTreeNode<T, U> node, HashSet<T> seenItems)
        {
            var optimizedMatches = new List<ItemDescriptor<T, U>>();
            foreach (var item in node.Matches)
            {
                if (!seenItems.Contains(item.Item))
                {
                    seenItems.Add(item.Item);
                    optimizedMatches.Add(item);
                }
            }

            var optimizedCriteria = new List<DecisionCriterion<T, U>>();
            foreach (var criterion in node.Criteria)
            {
                var optimizedBranches = new Dictionary<U, DecisionTreeNode<T, U>>(criterion.Branches.Comparer);
                foreach (var branch in criterion.Branches)
                {
                    var optimizedBranch = Optimize(branch.Value, seenItems);
                    if (optimizedBranch.Criteria.Any() || optimizedBranch.Matches.Any())
                    {
                        optimizedBranches.Add(branch.Key, optimizedBranch);
                    }
                }

                if (optimizedBranches.Any())
                {
                    optimizedCriteria.Add(new DecisionCriterion<T, U>()
                    {
                        Branches = optimizedBranches,
                        Key = criterion.Key,
                    });
                }
            }

            return new DecisionTreeNode<T, U>()
            {
                Criteria = optimizedCriteria,
                Matches = optimizedMatches,
                Value = node.Value,
            };
        }
    }
}
