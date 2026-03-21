using Infrastructure;
using System.Diagnostics;

namespace Core;

public class SystemHistory : IDisposable
{
    private static SystemHistory _instance;
    public static SystemHistory Instance { get { return _instance; } private set { _instance = value; } }

    private SystemTracker _tracker;
    private IProgramDataProducer _producer;
    private TimeSpan _snapshotInterval;
    private Timer _timer;
    private readonly Lock _timerLock = new Lock();
    private Stopwatch _stopwatch;

    public uint TotalSnapshotCount { get; private set; }
    public Action<List<IProgramData>>? OnSnapshotTaken { get; set; }

    public string SystemName { get { return _producer.SystemName; } }

    public SystemHistory(IProgramDataProducer producer, uint capacity, TimeSpan? snapshotInterval = null)
    {
        _producer = producer;
        _tracker = new SystemTracker(producer, capacity);
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

        if (producer == null)
            throw new InvalidOperationException("No valid producer for your operating system exists!");

        _instance = new(producer, 30, TimeSpan.FromSeconds(ConfigManager.ReadSetting(SettingFloat.TickRate)));
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
            TotalSnapshotCount++;
            if(TotalSnapshotCount * _snapshotInterval.TotalSeconds >= ConfigManager.ReadSetting(SettingFloat.DatabaseSaveInterval))
            {
                var average = _tracker.GetAverage();
                if (average == null) return;

                if(average != null) DBInteract.Store(average);
                TotalSnapshotCount = 0;
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
}
