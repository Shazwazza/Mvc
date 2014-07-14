using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.DecisionTree
{
    public class ItemDescriptor<T, U>
    {
        public IDictionary<string, U>  Criteria { get; set; }

        public int Index { get; set; }

        public T Item { get; set; }
    }
}