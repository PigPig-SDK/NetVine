

namespace UI.ViewModels.SearchFilter
{
    public class ParseTree(IFilterNode root)
    {
        readonly IFilterNode _root = root;

        public bool Evaluate(TableRow target)
        {
            if (_root == null) return false;
            return _root.Evaluate(target);
        }

    }
}
