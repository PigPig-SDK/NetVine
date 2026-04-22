using Core;
using Infrastructure;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Dynamic.Core;
using UI.ViewModels.SearchFilter;

namespace UI.ViewModels
{
    public class TableFilter
    {
        public static string SearchExpression { get; set; } = string.Empty;
        private static readonly TreeBuilder _treeBuilder = new();
        public static ParseTree? SearchExpressionTree { get; set; } = null;

        public static void UpdateSearchTree(string newExpression)
        {
            SearchExpression = newExpression;
            try
            {
                SearchExpressionTree = _treeBuilder.BuildTree(newExpression);
                Console.WriteLine("Search expression parsed successfully.");
            }
            catch (Exception ex)
            {
                Core.Debug.Log($"Error parsing search expression: {ex.Message}");
                SearchExpressionTree = null;
            }
        }

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

            if (!isCombination && !date1.HasValue && !date2.HasValue)
            {
                using var db = new DBInteract();
                return [.. db.PDHTable.Where(expr)];
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
