
namespace UI.ViewModels.SearchFilter
{

    public class TableFilter : TableFilterBase<TableFilter>
    {
        private static readonly TreeBuilder _treeBuilder = new();
        private static ParseTree? SearchExpressionTree { get; set; } = null;

        public TableFilter() { }

        public override bool Evaluate(TableRow target)
        {
            if (string.IsNullOrEmpty(SearchExpression)) { return false; } 
            return SearchExpressionTree == null
                || SearchExpressionTree.Evaluate(target);
        }




        public override void UpdateSearchExpression(string newExpression)
        {
            SearchExpression = newExpression;
            SearchExpressionTree = _treeBuilder.BuildTree(newExpression);
        }


    }
}
