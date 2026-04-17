using Infrastructure;
using System.Diagnostics;
using System.Drawing;

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
    /// Gets a process icon as a memory stream.
    /// </summary>
    /// <param name="processName">The process to get an icon of</param>
    /// <returns>An MemoryStream of a PNG/BMP/JPG</returns>
    public (MemoryStream? image, IconFileType fileType) GetProcessIcon(string processName);
    /// <summary>
    /// Gets a process icon as a memory stream.
    /// </summary>
    /// <param name="processName">The process to get an icon of</param>
    /// <returns>An MemoryStream of a PNG/BMP/JPG</returns>
    public (MemoryStream? image, IconFileType fileType) GetProcessIcon(Process process);
    /// <summary>
    /// Calls to produce the instant program data
    /// </summary>
    /// <param name="rate">Metadata about how long the current collection timespan</param>
    /// <returns></returns>
    public ICollection<IProgramData> Produce(TimeSpan rate);
}
