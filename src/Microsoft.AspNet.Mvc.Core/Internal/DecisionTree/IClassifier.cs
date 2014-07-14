using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.DecisionTree
{
    public interface IClassifier<T, U>
    {
        IDictionary<string, U> GetCriteria(T item);

        IEqualityComparer<U> ValueComparer { get; }
    }
}