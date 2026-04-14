using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.ViewModels
{
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
                    result = result && node.Evaluate();
            }

            else if (Type == CompositeType.OR)
            {
                result = false;
                foreach (IFilterNode node in _children)
                    result = result || node.Evaluate();
            }

            return result;
        }
    }


    public class FilterNode : IFilterNode
    {
        public string FieldName { get; }
        public string FieldValue { get; }
        private string _op;
        TableRow _target;
        public TableRow Target { set => _target = value; }
        public FilterNode(string fieldName, string fieldValue, string op, TableRow target)
        {
            _op = op;
            FieldValue = fieldValue;
            FieldName = fieldName;
            _target = target;
        }

        bool EvaluateField(float expected, float actual)
        {
            return _op switch
            {
                ">=" => expected >= actual,
                ">" => actual < expected,
                "<" => expected < actual,
                "<=" => expected <= actual,
                "=" => actual == expected,
                _ => false
            };
        }


        bool IFilterNode.Evaluate()
        {
            switch (FieldName)
            {
                case "Process": case "process": case "proc":
                    if (_target.AppName == FieldValue && _op == "=")
                        return true;
                    else return false;

                case "System": case "system": case "sys": case "Sys":
                    if (_target.SystemName == FieldValue && _op == "=")
                        return true;
                    else return false;
            }

            if (float.TryParse(FieldValue, out float v)) 
            {
                switch(FieldName)
                {
                    case "Cpu": case "CPU": case "cpu":
                        return EvaluateField(_target.Cpu, v);
                    case "Disk": case "disk":
                        return EvaluateField(_target.Disk, v);
                    case "Memory": case "memory": case "mem": case "Mem":
                        return EvaluateField(_target.Memory, v);
                    case "Network": case "network": case "net": case "Net":
                        return EvaluateField(_target.Network, v);
                }
            }

            return false;
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
        private TableRow? _target;
        public TableRow Target { set => _target = value; }
        public TreeBuilder(TableRow target, char and = '.', char or = '+', char lp = '(', char rp = ')')
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

        private FilterNode? BuildLeafNode(string expression)
        {
            if (ValidateLeafExpressionOperation(expression) == false) return null;
            string[] comboOps = { ">=", "<=" };
            string[] ops = { ">", "<", "=" };

            foreach (string op in comboOps)
                if (expression.Contains(op))
                {
                    string[] parts = expression.Split(op);
                    if (parts.Length == 2)
                        return new FilterNode(parts[0].Trim(), parts[1].Trim(), op, _target!);
                }


            foreach (string op in ops)
                if (expression.Contains(op))
                {
                    string[] parts = expression.Split(op);
                    if (parts.Length == 2)
                        return new FilterNode(parts[0].Trim(), parts[1].Trim(), op, _target!);
                }

            return null;
        }

        private IFilterNode Parse(string expression)
        {
            expression = expression.Trim();
           // if (string.IsNullOrEmpty(expression)) return new CompositeNode(); //safe empty
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

/*
         * Unhandled exception. System.IndexOutOfRangeException: Index was outside the bounds of the array.
         * at UI.ViewModels.TreeBuilder`1.Parse(String expression) in C:\Dev\GitHub\NetVine\UI\ViewModels\ParseSearch.cs:line 230
         * at UI.ViewModels.TreeBuilder`1.Parse(String expression) in C:\Dev\GitHub\NetVine\UI\ViewModels\ParseSearch.cs:line 240
         * at Avalonia.Collections.DataGridCollectionView.PassesFilter(Object item)
         * at Avalonia.Collections.DataGridCollectionView.PrepareLocalArray(IEnumerable enumerable)
         * at Avalonia.Collections.DataGridCollectionView.RefreshOverride()
         * at Avalonia.Collections.DataGridCollectionView.Refresh()
         * at UI.ViewModels.TableViewModel.set_SearchText(String value) in C:\Dev\GitHub\NetVine\UI\ViewModels\TableViewModel.cs:line 74                                         
         * at UI.ViewModels.TableViewModel.OnSearchKeyStroke(String search) in C:\Dev\GitHub\NetVine\UI\ViewModels\TableViewModel.cs:line 171
         * at UI.Views.MainWindow.SearchTextChanged(Object sender, TextChangedEventArgs e) in C:\Dev\GitHub\NetVine\UI\Views\MainWindow.axaml.cs:line 184
         * at Avalonia.Interactivity.EventRoute.RaiseEventImpl(RoutedEventArgs e)   
         * at Avalonia.Interactivity.EventRoute.RaiseEvent(Interactive source, RoutedEventArgs e)       
         * at Avalonia.Interactivity.Interactive.RaiseEvent(RoutedEventArgs e)                         
         * 
         * at Avalonia.Controls.TextBox.<RaiseTextChangeEvents>b__241_0()                                   
         * at Avalonia.Threading.DispatcherOperation.InvokeCore()                                           
         * at Avalonia.Threading.DispatcherOperation.Execute()                                              
         * at Avalonia.Threading.Dispatcher.ExecuteJob(DispatcherOperation job)                             
         * at Avalonia.Threading.Dispatcher.ExecuteJobsCore(Boolean fromExplicitBackgroundProcessingCallback)    
         * at Avalonia.Threading.Dispatcher.Signaled()                                                
         * at Avalonia.Win32.Win32Platform.WndProc(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam)  
         * at Avalonia.Win32.Interop.UnmanagedMethods.DispatchMessage(MSG& lpmsg)                           
         * at Avalonia.Win32.Win32DispatcherImpl.RunLoop(CancellationToken cancellationToken)               
         * at Avalonia.Threading.DispatcherFrame.Run(IControlledDispatcherImpl impl)                           
         * at Avalonia.Threading.Dispatcher.PushFrame(DispatcherFrame frame)                                
         * at Avalonia.Threading.Dispatcher.MainLoop(CancellationToken cancellationToken)                     
         * at Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime.StartCore(String[] args)    
         * at Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime.Start(String[] args)              
         * at Avalonia.ClassicDesktopStyleApplicationLifetimeExtensions.StartWithClassicDesktopLifetime(AppBuilder builder, String[] args, Action`1 lifetimeBuilder)  
         * at UI.Program.Main(String[] args) in C:\Dev\GitHub\NetVine\UI\Program.cs:line 38
         * 
         * 
         */


/*
 * Here is your stack trace formatted into multiple lines for better readability:

Unhandled exception. System.ArgumentOutOfRangeException: Specified argument was out of the range of valid values. (Parameter 'index')

    at Avalonia.Collections.DataGridCollectionView.GetItemAt(Int32 index)

    at Avalonia.Collections.DataGridCollectionView.get_IsCurrentInSync()

    at Avalonia.Collections.DataGridCollectionView.AdjustCurrencyForRemove(Int32 index)

    at Avalonia.Collections.DataGridCollectionView.ProcessRemoveEvent(Object removedItem, Boolean isReplace)

    at Avalonia.Collections.DataGridCollectionView.ProcessCollectionChanged(NotifyCollectionChangedEventArgs args)

    at Avalonia.Collections.DataGridCollectionView.<.ctor>b__27_0(Object _, NotifyCollectionChangedEventArgs args)

    at System.Collections.ObjectModel.ObservableCollection`1.OnCollectionChanged(NotifyCollectionChangedEventArgs e)

    at System.Collections.ObjectModel.Collection`1.Remove(T item)

    at UI.ViewModels.TableDataManager.UpdateLiveData(List`1 data)

        Location: C:\Dev\GitHub\NetVine\UI\ViewModels\TableDataManager.cs:line 52

    at UI.ViewModels.TableViewModel.<>c__DisplayClass74_0.<OnSnapshotLive>b__0()

        Location: C:\Dev\GitHub\NetVine\UI\ViewModels\TableViewModel.cs:line 180

    at Avalonia.Threading.DispatcherOperation.InvokeCore()

    at Avalonia.Threading.DispatcherOperation.Execute()

    at Avalonia.Threading.Dispatcher.ExecuteJob(DispatcherOperation job)

    at Avalonia.Threading.Dispatcher.ExecuteJobsCore(Boolean fromExplicitBackgroundProcessingCallback)

    at Avalonia.Threading.Dispatcher.Signaled()

    at Avalonia.Win32.Win32Platform.WndProc(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam)

    at Avalonia.Win32.Interop.UnmanagedMethods.DispatchMessage(MSG& lpmsg)

    at Avalonia.Win32.Win32DispatcherImpl.RunLoop(CancellationToken cancellationToken)

    at Avalonia.Threading.DispatcherFrame.Run(IControlledDispatcherImpl impl)

    at Avalonia.Threading.Dispatcher.PushFrame(DispatcherFrame frame)

    at Avalonia.Threading.Dispatcher.MainLoop(CancellationToken cancellationToken)

    at Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime.StartCore(String[] args)

    at Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime.Start(String[] args)

    at Avalonia.ClassicDesktopStyleApplicationLifetimeExtensions.StartWithClassicDesktopLifetime(AppBuilder builder, String[] args, Action`1 lifetimeBuilder)

    at UI.Program.Main(String[] args)

        Location: C:\Dev\GitHub\NetVine\UI\Program.cs:line 38

    Quick Observation

    It looks like the crash is happening inside TableDataManager.UpdateLiveData (line 52) when you call .Remove() on an ObservableCollection.

    This is triggering a UI update in the Avalonia DataGrid, but the collection's state and the DataGrid's internal "currency" (the currently 
    selected or active item) have fallen out of sync, causing it to look for an index that no longer exists. If you're removing items in a loop, 
    you might want to try removing from the end of the list first or using a for loop in reverse

    
 */