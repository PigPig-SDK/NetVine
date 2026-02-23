using System.Diagnostics;

namespace Core;

public class SystemHistory : IDisposable
{
    private SystemTracker _tracker;
    private IProgramDataProducer _producer;
    private TimeSpan _snapshotInterval;
    private Timer _timer;
    private readonly Lock _timerLock = new Lock();
    public Action<List<IProgramData>>? OnSnapshotTaken { get; set; }
    private Stopwatch _stopwatch;
    public uint TotalSnapshotCount { get; private set; }

    public SystemHistory(IProgramDataProducer producer, uint capacity, TimeSpan snapshotInterval)
    {
        _producer = producer;
        _tracker = new SystemTracker(producer, capacity);
        _snapshotInterval = snapshotInterval;
        _stopwatch = Stopwatch.StartNew();
        _timer = new Timer(TakeSnapshot, null, snapshotInterval, Timeout.InfiniteTimeSpan);
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

    /// <summary>
    /// IDisposable implementation to dispose
    /// </summary>
    public void Dispose()
    {
        _producer.Dispose();
    }
}
