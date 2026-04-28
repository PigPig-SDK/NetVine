using System;
using System.Threading;

namespace UI.ViewModels.SearchFilter
{
    public abstract class TableFilterBase<T>
    {
        //lazy needed to ensure thread safety
        private static readonly Lazy<T> _lazyInstance = new(() => Activator.CreateInstance<T>(), LazyThreadSafetyMode.ExecutionAndPublication);
        public static T Instance => _lazyInstance.Value;
        public static string SearchExpression { get; set; } = string.Empty;

        /// <summary>
        /// Update the search tree expression by parsing in a new string expression.
        /// </summary>
        /// <param name="newExpression"></param>
        public abstract void UpdateSearchExpression(string newExpression);


        //Evaluates the current search expression against a given TableRow, returning true if the row matches the criteria defined by the expression.
        public abstract bool Evaluate(TableRow target);

    }
}
