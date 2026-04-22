using Core;
using ScottPlot.Plottables;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace UI.ViewModels.SearchFilter
{


    public class TreeBuilder(char and = '&', char or = '+', char lp = '(', char rp = ')')
    {
        private readonly char _and = and;
        private readonly char _or = or;
        private readonly char _leftP = lp;
        private readonly char _rightP = rp;

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

        private static bool ValidateLeafExpressionOperation(string expression)
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

        private static FilterNode? BuildLeafNode(string expression)
        {
            if (ValidateLeafExpressionOperation(expression) == false) return null;
            string[] comboOps = { ">=", "<=" };
            string[] ops = { ">", "<", "=" };

            foreach (string op in comboOps)
                if (expression.Contains(op))
                {
                    string[] parts = expression.Split(op);
                    if (parts.Length == 2)
                        return new FilterNode(parts[0].Trim(), parts[1].Trim(), op);
                }


            foreach (string op in ops)
                if (expression.Contains(op))
                {
                    string[] parts = expression.Split(op);
                    if (parts.Length == 2)
                        return new FilterNode(parts[0].Trim(), parts[1].Trim(), op);

                }

            return null;
        }

        private IFilterNode Parse(string expression)
        {
            expression = expression.Trim();
            if (string.IsNullOrEmpty(expression)) return new CompositeNode(); //safe empty

            if (expression[0] == '(' && expression[^1] == ')')
                expression = expression[1..^1];

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


        public ParseTree? BuildTree(string expression)
        {
            if (expression == null) return null;
            return new ParseTree(this.Parse(expression));
        }

    }
}
