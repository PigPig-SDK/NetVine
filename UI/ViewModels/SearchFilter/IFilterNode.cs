using System;
using System.Linq.Expressions;
using Infrastructure;

namespace UI.ViewModels.SearchFilter
{
    public interface IFilterNode 
    {
        public bool Evaluate(TableRow target);
    }

}
