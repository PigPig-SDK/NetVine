namespace Core;

public interface IProgramDataProducer
{
    /// <summary>
    /// The system name of our data.
    /// </summary>
    public string SystemName { get;}
    /// <summary>
    /// Calls to produce the instant program data
    /// </summary>
    /// <param name="rate">Metadata about how long the current collection timespan</param>
    /// <returns></returns>
    public IEnumerable<IProgramData> Produce(float rate);
}
