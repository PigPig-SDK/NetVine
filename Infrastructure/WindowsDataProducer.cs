using Core;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
        = new(){ {NetworkClassification.Incoming, new() }, { NetworkClassification.Outgoing, new()} };
    /// <summary>
    /// A lock for a tracked resource
    /// Key : The data classification
    /// Value : The lock for that classification
    /// </summary>
    private Dictionary<NetworkClassification, Lock> _networkReadingLocks 
        = new(2) { { NetworkClassification.Incoming, new() }, { NetworkClassification.Outgoing, new() } };

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

            /*
             * Hugely important! We must clean up our KernelSessionName! If we don't between program instances, we might be locked out of data collection!
             * IDisposable is used to ensure we free our unmanaged resource.
             */
            _networkSession = new TraceEventSession(KernelSessionName);

            _networkSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);

            _networkSession.Source.Kernel.TcpIpSend += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.Outgoing);
            _networkSession.Source.Kernel.TcpIpRecv += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.Incoming);

            _networkSession.Source.Kernel.UdpIpSend += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.Outgoing);
            _networkSession.Source.Kernel.UdpIpRecv += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.Incoming);
            
            _networkSession.Source.Kernel.TcpIpRecvIPV6 += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.Incoming);
            _networkSession.Source.Kernel.TcpIpSendIPV6 += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.Outgoing);

            _networkSession.Source.Kernel.UdpIpRecvIPV6 += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.Incoming);
            _networkSession.Source.Kernel.UdpIpSendIPV6 += data => WriteNetworkDataToDictionary(data.ProcessID, data.size, NetworkClassification.Outgoing);

            _networkThread = new Thread(() => _networkSession.Source.Process())
            {
                IsBackground = true//Daemon thread. Shutdown when main thread shutsdown.
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
        //Clamp to 0, ignores negatives incase they are returned due to some weird edge case or bug
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
    /// Produces a collection of program data realitive to time.
    /// </summary>
    /// <param name="rate">The time between Produce() calls.</param>
    public ICollection<IProgramData> Produce(TimeSpan rate)
    {
        Dictionary<string, IProgramData> programs = new();
        Dictionary<int, TimeSpan> freshCpuDelta = new();
        Dictionary<int, IoCounters> freshDiskDelta = new();
        //Reset network usage for next iteration, clone is to avoid losing during transactions
        Dictionary<int, ulong> networkOut;
        Dictionary<int, ulong> networkIn;

        DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
            DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        //Change out network usage
        lock (_networkReadingLocks[NetworkClassification.Outgoing])
        {
            networkOut = _networkUsage[NetworkClassification.Outgoing];
            _networkUsage[NetworkClassification.Outgoing] = [];
        }
        lock (_networkReadingLocks[NetworkClassification.Incoming])
        {
            networkIn = _networkUsage[NetworkClassification.Incoming];
            _networkUsage[NetworkClassification.Incoming] = [];
        }


        foreach (Process process in Process.GetProcesses())
        {
            //Attempt to get IoCounters. If we don't have administration over the process we might get nothing!
            IoCounters? ioCounters = null;
            if (TryGetProcessIoCounters(process, out IoCounters tempCounter))
                ioCounters = tempCounter;

            try
            {
                //Private memory of the application in allocation.
                float mem = process.PrivateMemorySize64 / (1024f * 1024f);

                float networkUsage =
                    (float)((networkOut.GetValueOrDefault(process.Id) + networkIn.GetValueOrDefault(process.Id)) / rate.TotalSeconds / 1_000_000.0f);//Bytes to MB/s
                float diskUsage = 0;
                float cpuUsage = 0;

                //Push current CPU time to next tick.
                freshCpuDelta.TryAdd(process.Id, process.TotalProcessorTime);

                //Compute CPU delta.
                if (_cpuDelta.TryGetValue(process.Id, out TimeSpan oldCpuTime))
                {
                    TimeSpan delta = process.TotalProcessorTime - oldCpuTime;

                    if (delta >= TimeSpan.Zero)
                        cpuUsage = (float)(delta.TotalSeconds / rate.TotalSeconds / Environment.ProcessorCount * 100);//Convert to % per second
                    else
                        cpuUsage = 0f;//Process reset..
                }

                //Compute disk delta.
                if (ioCounters.HasValue)
                {
                    freshDiskDelta.TryAdd(process.Id, ioCounters.Value);

                    if (_diskDelta.TryGetValue(process.Id, out IoCounters oldIoCounter))
                    {
                        //Compute delta for read and write transfer count (This is a ugly line of code)
                        diskUsage = (ioCounters.Value.ReadTransferCount + ioCounters.Value.WriteTransferCount)
                                  - (oldIoCounter.ReadTransferCount + oldIoCounter.WriteTransferCount);

                        diskUsage = diskUsage / (float)rate.TotalSeconds / 1_000_000.0f;//Bytes to MB/s

                        if (diskUsage < 0)//Process reset might yeild false negatives, clamp to 0.
                            diskUsage = 0;
                    }
                }

                //append data.
                if (!programs.ContainsKey(process.ProcessName))
                    programs.TryAdd(process.ProcessName, new ProgramData
                    {
                        SystemName = SystemName,
                        ProcessName = process.ProcessName,
                        MemoryUsage = mem,
                        CpuUsage = cpuUsage,
                        DiskUsage = diskUsage,
                        NetworkUsage = networkUsage,
                        Timespan = (float)rate.TotalSeconds,
                        Date = now,
                    });
                else
                {
                    //TODO: Use + operator overloads instead of manually adding to each field.
                    programs[process.ProcessName].CpuUsage += cpuUsage;
                    programs[process.ProcessName].DiskUsage += diskUsage;
                    programs[process.ProcessName].MemoryUsage += mem;
                    programs[process.ProcessName].NetworkUsage += networkUsage;
                }
            }
            catch (Win32Exception ex)
            {
            }
            catch (InvalidOperationException ex)
            {
                Core.Debug.Log($"Process {process.ProcessName} exited before reading: {ex.Message}");
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
    [DllImport("kernel32.dll")]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);
    public double GetTotalRam()
    {
        var status = new MemoryStatusEx { dwLength = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        GlobalMemoryStatusEx(ref status);
        return status.ullTotalPhys / (1024.0 * 1024.0);
    }

    public MemoryStream? GetProcessIcon(string processName)
    {
        var version = Environment.OSVersion.Version;
        if(version.Major <= 6)
        {
            return null;
        }
        //Safe. Cannot execute on version 6 or below.
#pragma warning disable CA1416 // Validate platform compatibility

        Process? process = System.Diagnostics.Process
        .GetProcessesByName(processName)
        .FirstOrDefault();
        if (process is null)
        {
            
            return null;
        }

        if (process?.MainModule?.FileName is not { } path)
            return null;
        Icon? icon = Icon.ExtractAssociatedIcon(path);

        if (icon is null) return null;

        using Bitmap bitmap = icon.ToBitmap();
        var ms = new MemoryStream();
        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

#pragma warning restore CA1416 // Validate platform compatibility

        return ms;
    }
}