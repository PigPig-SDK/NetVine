using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UI.ViewModels.SearchFilter
{
    public class CompositeNode : IFilterNode
    {
        public CompositeType Type { get; }

        private readonly List<IFilterNode> _children;

        public CompositeNode()
        {
            Type = CompositeType.AND;
            _children = [];
        }

        public CompositeNode(CompositeType type)
        {
            Type = type;
            _children = [];
        }
        public void AddChild(IFilterNode node) { _children.Add(node); }

        public bool Evaluate(TableRow target)
        {
            if (_children.Count < 1) return false;

            bool result = Type == CompositeType.AND; //true if and, false if or

            foreach (IFilterNode node in _children)
            {
                if (node != null)
                {
                    bool nodeResult = node.Evaluate(target);
                    if (Type == CompositeType.AND)
                        result = result && nodeResult;
                    else
                        result = result || nodeResult;
                }
            }

            return result;
        }

    }
}
