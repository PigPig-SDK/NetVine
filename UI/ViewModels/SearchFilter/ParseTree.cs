using Infrastructure;
using System;
using System.Linq.Expressions;

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

        public Expression<Func<ProgramDataHistorical, bool>> ToExpression()
        {
            var parameter = Expression.Parameter(typeof(ProgramDataHistorical), "x");
            if (_root == null) return x => true;
            return _root.ToExpression(parameter);
        }
    }
}
