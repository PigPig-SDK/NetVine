namespace Core;

public interface IProgramDataProducer : IDisposable
{
    /// <summary>
    /// The system name of our data.
    /// </summary>
    public string SystemName { get;}
    /// <summary>
    /// Resolve how much ram the machine has
    /// </summary>
    public double GetTotalRam();

    /// <summary>
    /// Calls to produce the instant program data
    /// </summary>
    /// <param name="rate">Metadata about how long the current collection timespan</param>
    /// <returns></returns>
    public ICollection<IProgramData> Produce(TimeSpan rate);
}
