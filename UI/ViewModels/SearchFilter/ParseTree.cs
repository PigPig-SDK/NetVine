namespace UI.ViewModels.SearchFilter
{
    public class ParseTree<T>
    {
        IFilterNode _root;
        public ParseTree(IFilterNode root)
        {
            _root = root;
        }

        public bool Evaluate()
        {
            if (_root == null) return false;
            return _root.Evaluate();
        }
    }
}
