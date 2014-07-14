using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.DecisionTree
{
    public class DecisionTreeNode<T, U>
    {
        public List<ItemDescriptor<T, U>> Matches { get; set; }

        public List<DecisionCriterion<T, U>> Criteria { get; set; }

        public U Value { get; set; }
    }
}