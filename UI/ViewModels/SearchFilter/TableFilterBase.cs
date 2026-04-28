using Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        /// <summary>
        /// Retrieves a list of historical program data rows filtered by the specified criteria, including system
        /// selection, date range, and combination mode.
        /// </summary>
        /// <remarks>
        /// Used in the TableViewModel to fetch historical data for display in the UI, based on filters provided by the user.
        /// </remarks>
        /// <param name="isCombination">true to retrieve data for combination systems; otherwise, false to retrieve data for individual systems.</param>
        /// <param name="systemList">A list of system names to include in the results when combination mode is enabled. Ignored if isCombination
        /// is false.</param>
        /// <param name="date1">The start date of the historical data range to retrieve. If null, no lower bound is applied.</param>
        /// <param name="date2">The end date of the historical data range to retrieve. If null, no upper bound is applied.</param>
        /// <returns>A list of ProgramDataHistorical objects matching the specified filters. The list is empty if no data matches
        /// the criteria.</returns>
        public List<ProgramDataHistorical> GetTableRowsHistorical(bool isCombination
            , List<String> systemList
            , DateTime? date1
            , DateTime? date2)
        {

                return isCombination ? DBArithmetic.HistoricalDataProducer(systemList, date1, date2)
                    : DBArithmetic.HistoricalDataProducer(date1, date2);


        }
    }
}
