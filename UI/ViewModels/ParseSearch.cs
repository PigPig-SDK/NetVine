using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.ViewModels
{
    //from seperate project. Will break this up and integrate tomorrow
    class Row
    {
        public float Id { get; set; }
        public string Name { get; set; }
        public int RowId { get; set; }
        public string p { get; set; }

        public Row(float id, string name, int rowId, string p)
        {
            Id = id;
            Name = name;
            RowId = rowId;
            this.p = p;
        }
    }

    public interface IFilterNode { public bool Evaluate(); }
    public enum CompositeType { AND, OR };
    public class CompositeNode : IFilterNode
    {
        public CompositeType Type { get; }

        private List<IFilterNode> children;

        public CompositeNode()
        {
            Type = CompositeType.AND;
            children = new List<IFilterNode>();
        }

        public CompositeNode(CompositeType type)
        {
            Type = type;
            children = new List<IFilterNode>();
        }
        public void AddChild(IFilterNode node) { children.Add(node); }

        public bool Evaluate()
        {
            bool result = true;

            if (Type == CompositeType.AND)
            {
                result = true;
                foreach (IFilterNode node in children)
                    result = result && node.Evaluate();
            }

            else if (Type == CompositeType.OR)
            {
                result = false;
                foreach (IFilterNode node in children)
                    result = result || node.Evaluate();
            }

            return result;
        }
    }

    public class FilterNode<TypeRow> : IFilterNode
    {
        public string FieldName { get; }
        public string FieldValue { get; }
        private string _op;
        TypeRow _target;
        public TypeRow Target { set => _target = value; }
        public FilterNode(string fieldName, string fieldValue, string op, TypeRow target)
        {
            _op = op;
            FieldValue = fieldValue;
            FieldName = fieldName;
            _target = target;
        }

        bool IFilterNode.Evaluate()
        {
            if (_target == null) return false;
            var prop = _target.GetType().GetProperty(FieldName);
            if (prop == null) return false;
            try
            {
                object? propertyValue = prop.GetValue(_target);
                object? convertedInput = Convert.ChangeType(FieldValue, prop.PropertyType);

                if (propertyValue is IComparable comp)
                {
                    int result = comp.CompareTo(convertedInput);

                    switch (_op) // e.g., "<", ">", "<=", ">="
                    {
                        case "<": return result < 0;
                        case ">": return result > 0;
                        case "<=": return result <= 0;
                        case ">=": return result >= 0;
                        case "=": return result == 0;
                        default: return false;
                    }
                }
                else return false;

            }
            catch { return false; }

        }
    }



    public class TreeBuilder<TypeRow>
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

        private char _and;
        private char _or;
        private char _leftP;
        private char _rightP;
        private TypeRow? _target;
        public TypeRow Target { set => _target = value; }
        public TreeBuilder(TypeRow target, char and = '.', char or = '+', char lp = '(', char rp = ')')
        {
            _target = target;
            _and = and; _or = or; _leftP = lp; _rightP = rp;
        }

        private List<string> SplitByLevel0(string input, char op)
        {
            var parts = new List<string>();
            int start = 0;
            int level = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == _leftP) level++;
                else if (input[i] == _rightP) level--;
                else if (level == 0 && input[i] == op)
                {
                    parts.Add(input.Substring(start, i - start));
                    start = i + 1;
                }
            }
            parts.Add(input.Substring(start));
            return parts;
        }

        private bool ValidateLeafExpressionOperation(string expression)
        {
            if (expression == null) return false;
            string[] comboOps = { ">=", "<=" };
            string[] ops = { ">", "<", "=" };
            int singleCount = 0;
            int comboCount = 0;

            foreach (string op in comboOps)
                comboCount += ((expression.Split(op)).Length - 1);

            foreach (string op in ops)
                singleCount += (expression.Split(op)).Length - 1;

            if ((singleCount == 1 && comboCount == 0)
                || (singleCount == 2 && comboCount == 1)) return true;
            else return false;

        }

        private FilterNode<TypeRow>? BuildLeafNode(string expression)
        {
            if (ValidateLeafExpressionOperation(expression) == false) return null;
            string[] comboOps = { ">=", "<=" };
            string[] ops = { ">", "<", "=" };

            foreach (string op in comboOps)
                if (expression.Contains(op))
                {
                    string[] parts = expression.Split(op);
                    if (parts.Length == 2)
                        return new FilterNode<TypeRow>(parts[0].Trim(), parts[1].Trim(), op, _target!);
                }


            foreach (string op in ops)
                if (expression.Contains(op))
                {
                    string[] parts = expression.Split(op);
                    if (parts.Length == 2)
                        return new FilterNode<TypeRow>(parts[0].Trim(), parts[1].Trim(), op, _target!);
                }

            return null;
        }

        private IFilterNode Parse(string expression)
        {
            expression = expression.Trim();
            if (expression[0] == '(' && expression[expression.Length - 1] == ')')
            {
                expression = expression.Substring(1, expression.Length - 2);
            }

            var parts = SplitByLevel0(expression, _or);
            if (parts.Count > 1)
            {
                var node = new CompositeNode(CompositeType.OR);
                foreach (var p in parts) node.AddChild(Parse(p));
                return node;
            }
            parts = SplitByLevel0(expression, _and);
            if (parts.Count > 1)
            {
                var node = new CompositeNode(CompositeType.AND);
                foreach (var p in parts) node.AddChild(Parse(p));
                return node;
            }
            return BuildLeafNode(expression)!;
        }


        public ParseTree<TypeRow>? BuildTree(string expression)
        {
            if (expression == null) return null;
            return new ParseTree<TypeRow>(this.Parse(expression));
        }

    }
}
