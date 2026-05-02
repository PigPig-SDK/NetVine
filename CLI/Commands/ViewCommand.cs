
using Core;
using Infrastructure;
using Spectre.Console;
using System.Security.Cryptography.X509Certificates;

namespace CLI.Commands;

public class ViewCommand : Command
{
    public ViewCommand(App app) : base(app) { }

    public override string Name => "view";

    public override string Description => "Displays current application usage";

    public override string Usage => "'view live', 'view db', 'view live <p1 p2 p...>', 'view db <p1 p2 p...>'";

    private static List<ProgramDataHistorical> DBPrograms = [];
    private static IProgramData[] LivePrograms = [];
    private static bool _isDirty = false;
    private static bool _isRunning = false;
    private static readonly Lock _lock = new();

    public override void Execute(string[] args)
    {
        if (args.Length <= 0)
        {
            App.PrintError("Invalid arguments", $"Usage: {Usage}");
            return;
        }

        _isRunning = true;
        _isDirty = true;
        var viewType = args[0].ToLower();
        switch (viewType)
        {
            case "live":
                LiveView(args);
                break;
            case "db":
                DBView(args);
                break;
            default:
                App.PrintError("Invalid view type", $"Usage: {Usage}");
                break;
        }
    }

    void Escape()
    {
        _ = Task.Run(() =>
        {
            while (_isRunning)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    _isRunning = false;
                    break;
                }
                Thread.Sleep(50);
            }
        });
    }

    private void DBView(string[] args)
    {
        Escape();
        DBInteract.OnProgramListAdded += OnDBProgramsAdded;
        OnDBProgramsAdded([],true);
        Console.CursorVisible = false;

        while (_isRunning)
        {
            if (_isDirty)
            {
                var table = new Table().Expand();
                table.AddColumn("User");
                table.AddColumn("Process");
                table.AddColumn("CpuAVG %");
                table.AddColumn("DiskAVG");
                table.AddColumn("MemoryAVG");
                table.AddColumn("NetworkAVG");
                table.AddColumn("NetworkTotal");
                lock (_lock)
                {
                    foreach (var program in DBPrograms)
                    {
                        if (!IsProgramInArgs(program.ProcessName, args)) continue;
                        table.AddRow(
                            program.SystemName,
                            program.ProcessName,
                            program.CpuUsageAvg.ToString(),
                            program.DiskUsageAvg.ToString(),
                            program.MemoryUsageAvg.ToString(),
                            program.NetworkUsageAvg.ToString(),
                            program.NetworkUsageTotal.ToString());
                    }
                }
                AnsiConsole.Clear();
                table.Width(App.TableWidth);
                AnsiConsole.Write(table);
                AnsiConsole.MarkupLineInterpolated($"Press [bold yellow]ESC[/] to exit view.");
                _isDirty = false;
            }
            Thread.Sleep(50);
        }

        Console.CursorVisible = true;
        DBPrograms = [];//free.
        AnsiConsole.Write(new Rule());
        App.PrintIntro();
        DBInteract.OnProgramListAdded -= OnDBProgramsAdded;
    }

    private void OnDBProgramsAdded(List<ProgramData> programs, bool isDataLocal)
    {
        lock (_lock)
        {
            DBPrograms = DBArithmetic.HistoricalDataProducer(null, null);
        }
        _isDirty = true;
    }

    private void LiveView(string[] args)
    {
        Escape();
        SystemHistory.Instance.OnSnapshotTaken += OnLiveSnapshotTaken;
        Console.CursorVisible = false;

        while (_isRunning)
        {
            if (_isDirty)
            {
                var table = new Table().Expand();
                table.AddColumn("User");
                table.AddColumn("Process");
                table.AddColumn("Cpu %");
                table.AddColumn("Disk");
                table.AddColumn("Memory");
                table.AddColumn("Network");
                lock (_lock)
                {
                    if(LivePrograms.Length == 0)
                    {
                        table.AddRow("[grey]No data yet...[/]", "", "", "", "", "");
                    }
                    foreach (var program in LivePrograms)
                    {
                        if (!IsProgramInArgs(program.ProcessName, args)) continue;

                        table.AddRow(
                            program.SystemName,
                            program.ProcessName,
                            program.CpuUsage.ToString(),
                            program.DiskUsage.ToString(),
                            program.MemoryUsage.ToString(),
                            program.NetworkUsage.ToString());
                    }
                }
                AnsiConsole.Clear();
                table.Width(App.TableWidth);
                AnsiConsole.Write(table);
                AnsiConsole.MarkupLineInterpolated($"Press [bold yellow]ESC[/] to exit view.");
                _isDirty = false;
            }
            Thread.Sleep(50);
        }

        Console.CursorVisible = true;
        LivePrograms = [];//free.
        AnsiConsole.Write(new Rule());
        App.PrintIntro();
        SystemHistory.Instance.OnSnapshotTaken -= OnLiveSnapshotTaken;
    }

    private bool IsProgramInArgs(string programName, string[] args)
    {
        if(args.Length == 1) return true;
        for(int i = 1; i < args.Length; i++)
        {
            if (programName.Contains(args[i], StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private void OnLiveSnapshotTaken(List<IProgramData> list)
    {
        lock (_lock)
        {
            LivePrograms = list.ToArray();
        }
        _isDirty = true;
    }
}
