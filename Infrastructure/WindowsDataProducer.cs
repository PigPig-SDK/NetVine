using Core;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;


namespace Infrastructure;

/// <summary>
/// TODO: REMOVE ME!
/// </summary>
public class MockProgramDataClass : IProgramData
{
    private string _systemName, _processName;
    private float _cpuUsage, _diskUsage, _networkUsage, _memoryUsage, _timespan;
    public string SystemName { get => _systemName; set => _systemName = value; }
    public string ProcessName { get => _processName; set => _processName = value; }
    public float CpuUsage { get => _cpuUsage; set => _cpuUsage = value; }
    public float DiskUsage { get => _diskUsage; set => _diskUsage = value; }
    public float NetworkUsage { get => _networkUsage; set => _networkUsage = value; }
    public float MemoryUsage { get => _memoryUsage; set => _memoryUsage = value; }
    public float Timespan { get => _timespan; set => _timespan = value; }
}
public class WindowsDataProducer : IProgramDataProducer, IDisposable
{
    /// <summary>
    /// The current system name, defaulting to "UnknownUser" if it cannot be determined.
    /// </summary>
    public string SystemName { get => _systemName??"UnknownUser"; private set => _systemName = value; }
    private string? _systemName;
    /// <summary>
    /// The network tracker
    /// </summary>
    private TraceEventSession _networkSession = new TraceEventSession("NetworkSession");
    /// <summary>
    /// The network thread for tracking network events.
    /// </summary>
    private Thread? _networkThread;
    /// <summary>
    /// Tracks incoming usage per process. Updated by the network tracing events.
    /// Key : Process name
    /// Value : Total bytes received
    /// </summary>
    private Dictionary<NetworkClassification, Dictionary<int, ulong>> _networkUsage 
        = new(){ {NetworkClassification.NetworkIncoming, new() }, { NetworkClassification.NetworkOutgoing, new()} };
    /// <summary>
    /// A lock for a tracked resource
    /// Key : The data classification
    /// Value : The lock for that classification
    /// </summary>
    private Dictionary<NetworkClassification, Lock> _networkReadingLocks 
        = new(2) { { NetworkClassification.NetworkIncoming, new() }, { NetworkClassification.NetworkOutgoing, new() } };

    private Dictionary<string, TimeSpan> _cpuDelta = new();
    private Dictionary<string, IoCounters> _diskDelta = new();

    public WindowsDataProducer()
    {
        SystemName = Environment.UserName;

        _ = Produce(0.0f);//Initialize deltas.

        //Setup network tracing
        if (Environment.IsPrivilegedProcess)
        {
            _networkSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);

            _networkSession.Source.Kernel.TcpIpSend += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.NetworkIncoming);
            _networkSession.Source.Kernel.TcpIpRecv += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.NetworkIncoming);
            _networkSession.Source.Kernel.UdpIpRecv += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.NetworkIncoming);
            _networkSession.Source.Kernel.UdpIpSend += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.NetworkIncoming);
            _networkSession.Source.Kernel.TcpIpRecvIPV6 += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.NetworkIncoming);
            _networkSession.Source.Kernel.TcpIpSendIPV6 += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.NetworkOutgoing);
            _networkSession.Source.Kernel.UdpIpRecvIPV6 += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.NetworkIncoming);
            _networkSession.Source.Kernel.UdpIpSendIPV6 += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.NetworkOutgoing);

            _networkThread = new Thread(() => _networkSession.Source.Process())
            {
                IsBackground = true
            };
            _networkThread.Start();
        }
        else
        {
            Console.WriteLine("Cannot enable network tracing without admin privileges. Network data will not be collected.");
        }
    }
    /// <summary>
    /// A P/Invoke declaration for the GetProcessIoCounters function, which retrieves I/O accounting information for a specified process.
    /// This function is used to gather disk usage data for processes.
    /// </summary>
    /// <param name="ProcessHandle">An expected handle from a process (process.Handle)</param>
    /// <param name="IoCounters">A struct returned</param>
    /// <returns>True if a process counter was returned</returns>
    /// https://stackoverflow.com/questions/53560561/get-disk-usage-of-a-specific-process-in-c-sharp
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetProcessIoCounters(IntPtr ProcessHandle, out IoCounters IoCounters);

    public static bool TryGetProcessIoCounters(Process process, out IoCounters ioCounters)
    {
        try
        {
            return GetProcessIoCounters(process.Handle, out ioCounters);
        }
        catch (Exception ex) when (ex is Win32Exception || ex is InvalidOperationException)

        {
            ioCounters = default;
            return false;
        }
    }

    /// <summary>
    /// Writes the given "dataInfo" to its respective dictionary.
    /// </summary>
    /// <param name="processid">The process we are appending usage to</param>
    /// <param name="size">The size of the data in bytes</param>
    /// <param name="dataInfo">The data classification</param>
    private void WriteNetworkDataToDictionary(int processid, int size, NetworkClassification dataInfo)
    {
        ulong value = (size < 0) ? 0 : (ulong)size;

        lock (_networkReadingLocks[dataInfo])
        {
            if (_networkUsage[dataInfo].ContainsKey(processid))
                _networkUsage[dataInfo][processid] += value;
            else
                _networkUsage[dataInfo][processid] = value;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="rate"></param>
    public ICollection<IProgramData> Produce(float rate)
    {
        Dictionary<string, IProgramData> programs = new();
        Dictionary<string, TimeSpan> freshCpuDelta = new();
        Dictionary<string, IoCounters> freshDiskDelta = new();
        //Reset network usage for next iteration, clone is to avoid losing during transactions
        Dictionary<int, ulong> networkOut;
        Dictionary<int, ulong> networkIn;
        lock (_networkReadingLocks[NetworkClassification.NetworkOutgoing])
        {
            networkOut = _networkUsage[NetworkClassification.NetworkOutgoing];
            _networkUsage[NetworkClassification.NetworkOutgoing] = [];
        }
        lock (_networkReadingLocks[NetworkClassification.NetworkIncoming])
        {
            networkIn = _networkUsage[NetworkClassification.NetworkIncoming];
            _networkUsage[NetworkClassification.NetworkIncoming] = [];
        }

        //Pull down network data, and reset for next iteration
        foreach (NetworkClassification classification in Enum.GetValues<NetworkClassification>())
        {
            lock (_networkReadingLocks[classification])
                _networkUsage[classification].Clear();
        }

        foreach (Process process in Process.GetProcesses())
        {
            IProgramData? data = null;
            IoCounters? ioCounters = null;
            if (TryGetProcessIoCounters(process, out IoCounters tempCounter))
                ioCounters = tempCounter;

            try
            {
                float mem = process.WorkingSet64 / (1024f * 1024f);
                float networkUsage = 
                    (float)(networkOut.GetValueOrDefault(process.Id)  + networkIn.GetValueOrDefault(process.Id))/rate/1000000.0f;//Bytes to MB
                float diskUsage = 0;
                float cpuUsage = 0;
                //Compute CPU delta.
                if(freshCpuDelta.ContainsKey(process.ProcessName))
                    freshCpuDelta[process.ProcessName] += process.TotalProcessorTime;
                else
                    freshCpuDelta.Add(process.ProcessName, process.TotalProcessorTime);
                if (_cpuDelta.TryGetValue(process.ProcessName, out TimeSpan oldCpuTime))
                {
                    TimeSpan cpuTime = process.TotalProcessorTime;
                    TimeSpan delta = cpuTime - oldCpuTime;

                    if (delta >= TimeSpan.Zero)
                        cpuUsage = (float)(delta.TotalSeconds / rate / Environment.ProcessorCount * 100);//Convert to %
                    else
                        cpuUsage = 0f;//Process reset..
                }
                //Compute disk delta.
                if (ioCounters.HasValue)
                {
                    if(!freshDiskDelta.ContainsKey(process.ProcessName))
                        freshDiskDelta.Add(process.ProcessName, ioCounters.Value);

                    if (_diskDelta.TryGetValue(process.ProcessName, out IoCounters oldIoCounter))
                    {
                        //Compute delta.
                        diskUsage = ioCounters.Value.ReadTransferCount + ioCounters.Value.WriteTransferCount;
                        diskUsage -= oldIoCounter.ReadTransferCount + oldIoCounter.WriteTransferCount;
                    }
                }
                //Finalize
                data = new MockProgramDataClass
                {
                    SystemName = Environment.MachineName,
                    ProcessName = process.ProcessName,
                    MemoryUsage = mem,
                    CpuUsage = cpuUsage,
                    DiskUsage = diskUsage,
                    NetworkUsage = networkUsage,
                    Timespan = rate
                };
            }
            catch (Win32Exception ex)
            {
                Debug.WriteLine($"Access denied to process {process.ProcessName}: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"Process {process.ProcessName} exited before reading: {ex.Message}");
            }
            if (data != null)
            {
                if (!programs.ContainsKey(process.ProcessName))
                    programs.Add(process.ProcessName, data);
                else
                {
                    programs[process.ProcessName].CpuUsage += data.CpuUsage;
                    programs[process.ProcessName].DiskUsage += data.DiskUsage;
                    programs[process.ProcessName].MemoryUsage += data.MemoryUsage;
                    programs[process.ProcessName].NetworkUsage += data.NetworkUsage;
                }
            }
        }
        _cpuDelta = freshCpuDelta;
        _diskDelta = freshDiskDelta;
        return programs.Values;
    }

    public void Dispose()
    {
        _networkSession.Dispose();
        _networkThread?.Join();
    }
}
