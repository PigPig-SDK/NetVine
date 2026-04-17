using System;
using System.Collections.Generic;

namespace UI.ViewModels.SearchFilter
{
    public class FilterTester
    {
        int _rowCount = 20;
        List<TableRow> _testRows;
        List<string> _testStrings;

        public FilterTester()
        {
            _testRows = new List<TableRow>();
            _testStrings = new List<string>();
        }

        ProgramData getDummyDataLive(string pname, string sname)
        {
            float min = 0.0f;
            float max = 100.0f;
            var l = new ProgramData();
            l.CpuUsage = min + (float)Random.Shared.NextDouble() * (max - min);
            l.DiskUsage = min + (float)Random.Shared.NextDouble() * (max - min);
            l.NetworkUsage = min + (float)Random.Shared.NextDouble() * (max - min);
            l.MemoryUsage = min + (float)Random.Shared.NextDouble() * (max - min);
            l.Date = DateTime.Now;
            l.ProcessName = pname;
            l.SystemName = sname;
            return l;
        }

        void PopulateTestRows()
        {

            for (int i = 0; i < _rowCount; i++)
            {
                var t = new TableRow();
                char name = (char)(('A' + i) % 26);
                string n = "";
                n += name;
                t.AppName = n;
                t.SystemName = "S";
                t.LiveData = getDummyDataLive(n, "S");
                _testRows.Add(t);

                Console.WriteLine($"[Row {i,2}] Process={n,-2} | CPU={t.LiveData.CpuUsage,6:F2} | Disk={t.LiveData.DiskUsage,6:F2} | Mem={t.LiveData.MemoryUsage,6:F2} | Net={t.LiveData.NetworkUsage,6:F2}");
            }

            Console.WriteLine($"PopulateTestRows: {_testRows.Count} rows created.\n");
        }

        void PopulateTestStrings()
        {
            _testStrings = new List<string>
        {
            "Cpu>=50",
            "Memory<80",
            "Cpu>=50&Memory<80",
            "Disk>=50+Network>=50",
            "Process=A",
            "System=S",
            "Cpu>=50&Memory<80 & Process=A",
            "(Cpu>=50+Memory>=50) & System=S",
        };

            Console.WriteLine("PopulateTestStrings: test expressions loaded:");
            foreach (var s in _testStrings)
                Console.WriteLine($"  \"{s}\"");
            Console.WriteLine();
        }

        void RunTests()
        {
            Console.WriteLine("=== Running Filter Tests ===\n");

            foreach (var expr in _testStrings)
            {
                Console.WriteLine($"Filter: \"{expr}\"");
                int matchCount = 0;

                foreach (var row in _testRows)
                {
                    var builder = new TreeBuilder<TableRow>();
                    var tree = builder.BuildTree(expr);
                    bool result = tree?.Evaluate(row) ?? false;

                    if (result)
                    {
                        Console.WriteLine($"  PASS -> Process={row.LiveData.ProcessName} | CPU={row.LiveData.CpuUsage:F2} | Mem={row.LiveData.MemoryUsage:F2}");
                        matchCount++;
                    }
                }

                if (matchCount == 0)
                    Console.WriteLine("  (no rows matched)");

                Console.WriteLine($"  Matched {matchCount}/{_testRows.Count} rows.\n");
            }
        }

        public void Run()
        {
            Console.WriteLine("=== FilterTester Start ===\n");

            while (true)
            {
                var t = new TableRow();
                char name = 'A';
                string n = "";
                n += name;
                t.AppName = n;
                t.SystemName = "S";
                t.LiveData = getDummyDataLive(n, "S");
                _testRows.Add(t);

                Console.WriteLine($" Process={n} | CPU={t.LiveData.CpuUsage,6:F2} | Disk={t.LiveData.DiskUsage,6:F2} | Mem={t.LiveData.MemoryUsage,6:F2} | Net={t.LiveData.NetworkUsage,6:F2}");
                
                Console.Write("Enter filter: ");
                string input = Console.ReadLine() ?? "";

                if (input == "quit" || input == "exit") break;

                var builder = new TreeBuilder<TableRow>();
                bool result = builder.BuildTree(input)?.Evaluate(t) ?? false;
                Console.WriteLine(result ? "PASS" : "FAIL");
            }

            Console.WriteLine("=== FilterTester Done ===");
        }
    }
}
