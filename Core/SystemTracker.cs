using System.Data.Common;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    /// Returns the average of the last N snapshots. Returns null if no data exists.
    /// </summary>
    /// <returns></returns>
    public List<IProgramData>? GetAverage()
    {
        if (History.Count == 0) return null;

        Dictionary<string, (IProgramData data, float sum)> programs = [];
        //Place into buckets
        foreach (List<IProgramData> snapshot in History)
        {
            foreach (IProgramData data in snapshot)
            {
                if (!programs.ContainsKey(data.ProcessName))
                {
                    programs[data.ProcessName] = (data, 1.0f);
                }
                else
                {
                    programs[data.ProcessName].data.CpuUsage += data.CpuUsage;
                    programs[data.ProcessName].data.DiskUsage += data.DiskUsage;
                    programs[data.ProcessName].data.MemoryUsage += data.MemoryUsage;
                    programs[data.ProcessName].data.NetworkUsage += data.NetworkUsage;
                    programs[data.ProcessName].data.Timespan += data.Timespan;
                    programs[data.ProcessName] = (programs[data.ProcessName].data, programs[data.ProcessName].sum + 1.0f);
                }
            }
        }

        //Normalize
        foreach ((IProgramData data, float sum) key in programs.Values)
        {
            key.data.CpuUsage /= key.sum;
            key.data.DiskUsage /= key.sum;
            key.data.MemoryUsage /= key.sum;
            key.data.NetworkUsage /= key.sum;
            key.data.Timespan /= key.sum;
        }

        return programs.Values.Select(e => e.data).ToList();
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
