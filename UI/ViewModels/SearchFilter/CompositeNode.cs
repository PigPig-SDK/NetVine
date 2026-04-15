using System.Collections.Generic;

namespace UI.ViewModels.SearchFilter
{
    public class CompositeNode : IFilterNode
    {
        public CompositeType Type { get; }

        private List<IFilterNode> _children;

        public CompositeNode()
        {
            Type = CompositeType.AND;
            _children = new List<IFilterNode>();
        }

        public CompositeNode(CompositeType type)
        {
            Type = type;
            _children = new List<IFilterNode>();
        }
        public void AddChild(IFilterNode node) { _children.Add(node); }

        public bool Evaluate()
        {
            bool result = true;
            if (_children.Count < 1) return false;

            if (Type == CompositeType.AND)
            {
                result = true;
                foreach (IFilterNode node in _children)
                    if (node != null)
                        result = result && node.Evaluate();
            }

            else if (Type == CompositeType.OR)
            {
                result = false;
                foreach (IFilterNode node in _children)
                    if (node != null)
                        result = result || node.Evaluate();
            }

            return result;
        }
    }
}
