using Infrastructure;
using System.Diagnostics;
using System.Drawing;

namespace Core;

public class SystemHistory : IDisposable
{
    private static SystemHistory? _instance;
    public static SystemHistory Instance { get { return _instance ?? throw new InvalidProgramException("Please Initialize SystemHistory before calling for instance."); } private set { _instance = value; } }

    private SystemTracker _tracker;
    private IProgramDataProducer _producer;
    private TimeSpan _snapshotInterval;
    private Timer _timer;
    private readonly Lock _timerLock = new Lock();
    private Stopwatch _stopwatch;

    public uint SnapshotIterationCount { get; private set; }
    public Action<List<IProgramData>>? OnSnapshotTaken { get; set; }

    public string SystemName { get { return _producer.SystemName; } }

    public SystemHistory(IProgramDataProducer producer, TimeSpan? snapshotInterval = null)
    {
        _producer = producer;
        _tracker = new SystemTracker(producer);
        _snapshotInterval = snapshotInterval ?? TimeSpan.FromSeconds(ConfigManager.ReadSetting(SettingFloat.TickRate));
        _stopwatch = Stopwatch.StartNew();
        _timer = new Timer(TakeSnapshot, null, _snapshotInterval, Timeout.InfiniteTimeSpan);
        
        ConfigManager.OnSettingChanged += OnSettingChanged;
    }

    public static void SetupInstance()
    {
        IProgramDataProducer? producer = null;
        if(MockDataProducer.IsBeingUsed())
        {
            producer = new MockDataProducer();
        }
        else if (OperatingSystem.IsWindows())
        {
            producer = new WindowsDataProducer();
        }
        else if(OperatingSystem.IsLinux())
        {
            producer = new LinuxDataProducer();
        }

        if (producer == null)
            throw new InvalidOperationException("No valid producer for your operating system exists!");

        _instance = new(producer, TimeSpan.FromSeconds(ConfigManager.ReadSetting(SettingFloat.TickRate)));
    }

    public List<IProgramData> GetLatestPoll()
    {
        if (_tracker.TryGetSnapshot(0, out var snapshot))
            return snapshot!;//Snapshot is not null. Try/get method avoids null.
        return [];
    }
    private void OnSettingChanged(Enum setting)
    {
        if (setting is not SettingFloat settingfloat) return;

        ChangeSnapshotInterval(TimeSpan.FromSeconds(ConfigManager.ReadSetting(settingfloat)));
    }

    /// <summary>
    /// Resets the current snapshot interval.
    /// </summary>
    public void ChangeSnapshotInterval(TimeSpan newInterval)
    {
        lock (_timerLock)
        {
            _snapshotInterval = newInterval;
            _timer.Change(_snapshotInterval, Timeout.InfiniteTimeSpan);
        }
    }

    /// <param name="_">Discarded object</param>
    private void TakeSnapshot(object? _)
    {
        //Account for timer drift with stopwatch.
        //Incase context switch or lag causes the timer to be delayed.
        TimeSpan elapsed = _stopwatch.Elapsed;
        _stopwatch.Restart();
        
        try
        {
            var list = _tracker.MakeSnapshot(elapsed);
            OnSnapshotTaken?.Invoke(list);
            SnapshotIterationCount++;
            //On DB Storage hit.
            if(SnapshotIterationCount * _snapshotInterval.TotalSeconds >= ConfigManager.ReadSetting(SettingFloat.DatabaseSaveInterval))
            {
                IEnumerable<ProgramData>? average = _tracker.GetAverage()?.Cast<ProgramData>();
                if (average == null) return;

                //Store data...
                DBInteract.Store(average, true);
                AddIcons();

                SnapshotIterationCount = 0;
                _tracker.ClearHistory();
            }
        }
        finally
        {
            //Refire timer.
            lock (_timerLock)
            {
                _timer.Change(_snapshotInterval, Timeout.InfiniteTimeSpan);
            }
        }
    }

    public void AddIcons()
    {
        lock (DBInteract.DBLock)
        {
            using var db = new DBInteract();
            var processes = Process.GetProcesses();
            HashSet<string> seen = new();

            foreach (Process process in processes)
            {
                if (!seen.Add(process.ProcessName)) continue;
                if (db.HasIcon(process.ProcessName)) continue;

                var icon = _producer.GetProcessIcon(process);
                db.AddIcon(process.ProcessName,
                    icon.image?.ToArray() ?? [],
                    icon.image == null ? IconFileType.None : icon.fileType);
            }
            db.SaveChanges();
        }
    }

    /// <summary>
    /// Returns the averages of the last N snapshots. Returns null if no data exists.
    /// </summary>
    public List<IProgramData>? GetAverages() => _tracker.GetAverage();

    ~SystemHistory() => Dispose();

    /// <summary>
    /// IDisposable implementation to dispose
    /// </summary>
    public void Dispose()
    {
        ConfigManager.OnSettingChanged -= OnSettingChanged;
        _producer.Dispose();
    }
    /// <summary>
    /// Returns the machines total ammount of ram.
    /// </summary>
    public double GetTotalRam()
    {
        if (_producer is null) return 0;
        return _producer.GetTotalRam();
    }

    public (MemoryStream? image, IconFileType fileType) GetIcon(string processName) => _producer.GetProcessIcon(processName);
}
