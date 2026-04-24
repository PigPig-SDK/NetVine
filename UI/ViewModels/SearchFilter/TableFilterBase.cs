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
        private static readonly Lazy<T> _lazyInstance = new(() => Activator.CreateInstance<T>(), LazyThreadSafetyMode.ExecutionAndPublication);
        public static T Instance => _lazyInstance.Value;
        public static string SearchExpression { get; set; } = string.Empty;

        public ConcurrentDictionary<string, Func<ProgramDataHistorical, bool>> _cachedPredicates = new();
        public ConcurrentDictionary<string, Expression<Func<ProgramDataHistorical, bool>>> _cachedExpressions = new();
        public ConcurrentDictionary<string, ParseTree> _cachedTrees = new();

        protected abstract Expression<Func<ProgramDataHistorical, bool>>? CreateExpression(string expression);

        /// <summary>
        /// Update the search tree expression by parsing in a new string expression.
        /// </summary>
        /// <param name="newExpression"></param>
        public abstract void UpdateSearchExpression(string newExpression);
        protected bool TryCreateExpression(string newExpression, out Expression<Func<ProgramDataHistorical, bool>> expression)
        {
            expression = CreateExpression(newExpression) ?? (x => true);
            return expression != null;
        }

        protected void GetOrCreateCachedExpression(string newExpression
            , out Expression<Func<ProgramDataHistorical, bool>> cachedExpression
            , out Func<ProgramDataHistorical, bool> cachedPredicate)
        {
            if (!_cachedExpressions.TryGetValue(newExpression, out var cachedExpr))
            {
                if (!TryCreateExpression(newExpression, out var expr))
                {
                    Console.WriteLine("Failed to build expression from search expression.");
                    cachedExpression = (x => true);
                    cachedPredicate = cachedExpression.Compile();
                    return;
                }
                cachedExpr = expr;
                Console.WriteLine("Search expression loaded from cache.");
            }
            if (!_cachedPredicates.TryGetValue(newExpression, out var predicate))
            {
                predicate = cachedExpr.Compile();
            }
            else
            {
                Console.WriteLine("Search expression loaded from cache.");
            }
            _cachedExpressions[newExpression] = cachedExpr;
            _cachedPredicates[newExpression] = predicate;
            cachedPredicate = predicate;
            cachedExpression = cachedExpr;
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
        public List<ProgramDataHistorical> GetTableRowsHistorical(bool isCombination
            , List<String> systemList
            , DateTime? date1
            , DateTime? date2)
        {
            List<ProgramDataHistorical> data;

            if (string.IsNullOrWhiteSpace(SearchExpression))
                return isCombination ? DBArithmetic.HistoricalDataProducer(systemList, date1, date2)
                    : DBArithmetic.HistoricalDataProducer(date1, date2);

            GetOrCreateCachedExpression(SearchExpression, out var expr, out var predicate);

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
