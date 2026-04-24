using Core;
using Infrastructure;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace UI.ViewModels.SearchFilter
{

    public class TableFilter : TableFilterBase<TableFilter>
    {
        private static readonly TreeBuilder _treeBuilder = new();
        public static ParseTree? SearchExpressionTree { get; set; } = null;

        public TableFilter() { }

        protected override Expression<Func<ProgramDataHistorical, bool>> CreateExpression(string expression)
        {
            var tree = _treeBuilder.BuildTree(expression);
            if (tree == null)
            {
                Console.WriteLine("Failed to parse search expression.");
                return x => true;
            }
            var expr = tree.ToExpression();
            if (expr == null)
            {
                Console.WriteLine("Failed to build expression from search expression.");
                return x => true;
            }
            return expr;
        }

        public override void UpdateSearchExpression(string newExpression)
        {
            SearchExpression = newExpression;
            SearchExpressionTree = _treeBuilder.BuildTree(newExpression);
            GetOrCreateCachedExpression(newExpression, out _, out _);
        }

        
    }
}
