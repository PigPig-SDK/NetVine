using Infrastructure;
using System;
using System.Linq.Expressions;

namespace UI.ViewModels.SearchFilter
{

    public class TableFilter : TableFilterBase<TableFilter>
    {
        private static readonly TreeBuilder _treeBuilder = new();
        private static ParseTree? SearchExpressionTree { get; set; } = null;

        public TableFilter() { }

        public override bool Evaluate(TableRow target)
        {
            return SearchExpressionTree == null
                || SearchExpressionTree.Evaluate(target);
        }

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
