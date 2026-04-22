using Core;
using Infrastructure;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace UI.ViewModels.SearchFilter
{
    public class TableFilter
    {
        public static string SearchExpression { get; set; } = string.Empty;
        private static readonly TreeBuilder _treeBuilder = new();
        public static ParseTree? SearchExpressionTree { get; set; } = null;

        /// <summary>
        /// Update the search tree expression by parsing in a new string expression.
        /// </summary>
        /// <param name="newExpression"></param>
        public static void UpdateSearchTree(string newExpression)
        {
            SearchExpression = newExpression;

            SearchExpressionTree = _treeBuilder.BuildTree(newExpression);
            Console.WriteLine("Search expression parsed successfully.");

        }


        /// <summary>
        /// Retrieves a list of historical program data rows filtered by the specified criteria, including system
        /// selection, date range, and combination mode.
        /// </summary>
        /// <remarks>If both date1 and date2 are null, all available historical data is returned for the
        /// specified system(s) and combination mode. The method applies additional filtering based on a search
        /// expression if one is defined.
        /// 
        /// Used in the TableViewModel to fetch historical data for display in the UI, based on filters provided by the user.
        /// 
        /// </remarks>
        /// <param name="isCombination">true to retrieve data for combination systems; otherwise, false to retrieve data for individual systems.</param>
        /// <param name="systemList">A list of system names to include in the results when combination mode is enabled. Ignored if isCombination
        /// is false.</param>
        /// <param name="date1">The start date of the historical data range to retrieve. If null, no lower bound is applied.</param>
        /// <param name="date2">The end date of the historical data range to retrieve. If null, no upper bound is applied.</param>
        /// <returns>A list of ProgramDataHistorical objects matching the specified filters. The list is empty if no data matches
        /// the criteria.</returns>
        public static List<ProgramDataHistorical> GetTableRowsHistorical(bool isCombination
            , List<String> systemList
            , DateTime? date1
            , DateTime? date2)
        {
            List<ProgramDataHistorical> data;

            if (string.IsNullOrWhiteSpace(SearchExpression) || SearchExpressionTree == null)
                return isCombination? DBArithmetic.HistoricalDataProducer(systemList, date1, date2) 
                    : DBArithmetic.HistoricalDataProducer(date1, date2);

            var expr = SearchExpressionTree.ToExpression();
            var predicate = expr.Compile();

            if (!date1.HasValue && !date2.HasValue)
            {
                using var db = new DBInteract();
                return [.. db.PDHTable.Where(expr).Where(x => isCombination == (x.SystemName == "Combination"))];
            }

            if (isCombination)
                data = DBArithmetic.HistoricalDataProducer(systemList, date1, date2);
            else
                data = DBArithmetic.HistoricalDataProducer(date1, date2);

            var dataQueryable = data.Where(predicate);
            return [.. dataQueryable];
        }


    }
}
