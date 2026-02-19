namespace Core;

/// <summary>
/// This class is used to store immediate program data from a given producer
/// </summary>
public class SystemTracker
{
    /// <summary>
    /// The name of the system collecting data. (Could be from another computer)
    /// </summary>
    public string SystemName => ProgramDataProducer.SystemName;
    /// <summary>
    /// The data producer for our Data
    /// </summary>
    protected readonly IProgramDataProducer ProgramDataProducer;
    /// <summary>
    /// The last N queries
    /// </summary>
    private LinkedList<List<IProgramData>> Data = [];
    /// <summary>
    /// A property for reading the history directly.
    /// </summary>
    public IReadOnlyCollection<List<IProgramData>> History => Data;
    /// <summary>
    /// Maximum number of snapshots
    /// </summary>
    public uint Capacity { get; set; }

    public SystemTracker(IProgramDataProducer programDataProducer, uint capacity)
    {
        ProgramDataProducer = programDataProducer;
        Capacity = capacity;
    }
    /// <summary>
    /// Gives a snapshot of the
    /// </summary>
    /// <param name="index">The position in history</param>
    /// <returns>Null if no data exists for your position</returns>
    public virtual bool TryGetSnapshot(int index, out List<IProgramData>? data)
    {
        data = null;
        if(index < 0 || index >= Data.Count)
            return false;

        try
        {
            data = Data.ElementAt(index);
            return true;
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is ArgumentOutOfRangeException)
        {
            return false;
        }
    }
    /// <summary>
    /// Takes a snapshot of the current ProgramDataProducer and reshuffles the history.
    /// </summary>
    public List<IProgramData> MakeSnapshot(TimeSpan rate)
    {
        List<IProgramData> programData = [.. ProgramDataProducer.Produce(rate)];

        //Make room for new data.
        if(Data.Count >= Capacity) Data.RemoveLast();

        Data.AddFirst(programData);
        return programData;
    }
}
