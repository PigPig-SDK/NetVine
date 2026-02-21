using Core;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Infrastructure;

public class WindowsDataProducer : IProgramDataProducer
{
    /// <summary>
    /// The current system name, defaulting to "UnknownUser" if it cannot be determined.
    /// </summary>
    public string SystemName { get => _systemName??"UnknownUser"; private set => _systemName = value; }
    private string? _systemName;

    private const string KernelSessionName = "NetVine-Kernel";
    /// <summary>
    /// The network tracker
    /// </summary>
    private TraceEventSession? _networkSession;
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

    private Dictionary<int, TimeSpan> _cpuDelta = new();
    private Dictionary<int, IoCounters> _diskDelta = new();

    public WindowsDataProducer()
    {
        SystemName = Environment.UserName;

        _ = Produce(TimeSpan.Zero);//Initialize deltas.

        //Setup network tracing
        if (Environment.IsPrivilegedProcess)
        {
            TraceEventSession.GetActiveSession(KernelSessionName)?.Stop();

            _networkSession = new TraceEventSession(KernelSessionName);
            _networkSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);

            _networkSession.Source.Kernel.TcpIpSend += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.NetworkOutgoing);
            _networkSession.Source.Kernel.TcpIpRecv += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.NetworkIncoming);

            _networkSession.Source.Kernel.UdpIpSend += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.NetworkOutgoing);
            _networkSession.Source.Kernel.UdpIpRecv += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.NetworkIncoming);
            
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
    public ICollection<IProgramData> Produce(TimeSpan rate)
    {
        Dictionary<string, IProgramData> programs = new();
        Dictionary<int, TimeSpan> freshCpuDelta = new();
        Dictionary<int, IoCounters> freshDiskDelta = new();
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

        foreach (Process process in Process.GetProcesses())
        {
            IoCounters? ioCounters = null;
            if (TryGetProcessIoCounters(process, out IoCounters tempCounter))
                ioCounters = tempCounter;

            try
            {
                float mem = process.PrivateMemorySize64 / (1024f * 1024f);
                float networkUsage = 
                    (float)((networkOut.GetValueOrDefault(process.Id)  + networkIn.GetValueOrDefault(process.Id)) / rate.TotalSeconds / 1_000_000.0f);//Bytes to MB

                float diskUsage = 0;
                float cpuUsage = 0;
                //Compute CPU delta.
                freshCpuDelta.TryAdd(process.Id, process.TotalProcessorTime);

                if (_cpuDelta.TryGetValue(process.Id, out TimeSpan oldCpuTime))
                {
                    TimeSpan delta = process.TotalProcessorTime - oldCpuTime;

                    if (delta >= TimeSpan.Zero)
                        cpuUsage = (float)(delta.TotalSeconds / rate.TotalSeconds / Environment.ProcessorCount * 100);//Convert to %
                    else
                        cpuUsage = 0f;//Process reset..
                }

                //Compute disk delta.
                if (ioCounters.HasValue)
                {
                    freshDiskDelta.TryAdd(process.Id, ioCounters.Value);

                    if (_diskDelta.TryGetValue(process.Id, out IoCounters oldIoCounter))
                    {
                        diskUsage = (ioCounters.Value.ReadTransferCount + ioCounters.Value.WriteTransferCount) - (oldIoCounter.ReadTransferCount + oldIoCounter.WriteTransferCount);
                        if(diskUsage < 0)//Process reset..
                            diskUsage = 0;
                    }
                }
                //append data.
                if (!programs.ContainsKey(process.ProcessName))
                    programs.TryAdd(process.ProcessName, new MockProgramDataClass
                    {
                        SystemName = Environment.MachineName,
                        ProcessName = process.ProcessName,
                        MemoryUsage = mem,
                        CpuUsage = cpuUsage,
                        DiskUsage = diskUsage,
                        NetworkUsage = networkUsage,
                        Timespan = (float)rate.TotalSeconds,
                    });
                else
                {
                    programs[process.ProcessName].CpuUsage += cpuUsage;
                    programs[process.ProcessName].DiskUsage += diskUsage;
                    programs[process.ProcessName].MemoryUsage += mem;
                    programs[process.ProcessName].NetworkUsage += networkUsage;
                }
            }
            catch (Win32Exception ex)
            {
                Debug.WriteLine($"Access denied to process {process.ProcessName}: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"Process {process.ProcessName} exited before reading: {ex.Message}");
            }
        }

        _cpuDelta = freshCpuDelta;
        _diskDelta = freshDiskDelta;
        return programs.Values;
    }

    ~WindowsDataProducer() 
    {
        Dispose();
    }

    public void Dispose()
    {
        _networkSession?.Dispose();
        _networkThread?.Join();
    }
}
