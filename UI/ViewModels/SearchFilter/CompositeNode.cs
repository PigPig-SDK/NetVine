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

            //if (Type == CompositeType.AND)
            //{
            //    result = true;
            //    foreach (IFilterNode node in _children)
            //        if (node != null)
            //            result = result && node.Evaluate(target);
            //}

            //else if (Type == CompositeType.OR)
            //{
            //    result = false;
            //    foreach (IFilterNode node in _children)
            //        if (node != null)
            //            result = result || node.Evaluate(target);
            //}

            return result;
        }


        Expression<Func<ProgramDataHistorical, bool>> IFilterNode.ToExpression(ParameterExpression param)
        {
            Expression result = Expression.Constant(Type == CompositeType.AND); //true if and, false if or


            foreach (var child in _children)
            {
                bool isAnd = Type == CompositeType.AND;
                var childExprBody = child == null ? Expression.Constant(isAnd) : child.ToExpression(param).Body;
                if (isAnd)
                    result = Expression.AndAlso(result, childExprBody);
                else
                    result = Expression.OrElse(result, childExprBody);
            }

            return Expression.Lambda<Func<ProgramDataHistorical, bool>>(result, param);
        }
    }
}
