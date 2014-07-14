using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.DecisionTree
{
    public class DecisionCriterion<T, U>
    {
        public string Key { get; set; }

        public Dictionary<U, DecisionTreeNode<T, U>> Branches { get; set; }
    }
}