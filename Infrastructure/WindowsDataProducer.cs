using Core;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using System.ComponentModel;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;


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
    private string? _systemName;
    public string SystemName { get => _systemName??"UnknownUser"; private set => _systemName = value; }
    private TraceEventSession _networkSession = new TraceEventSession("NetworkSession");
    private Thread? _networkThread;
    private Dictionary<int, string> _procmap = new Dictionary<int, string>();


    public WindowsDataProducer()
    {
        SystemName = Environment.UserName;
        //Setup network tracing
        if (Environment.IsPrivilegedProcess)
        {
            _networkSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);
            _networkSession.Source.Kernel.TcpIpSend += KernelTcpIpSend;
            _networkSession.Source.Kernel.TcpIpRecv += KernelTcpIpRecv;
            _networkSession.Source.Kernel.UdpIpRecv += KernelUdpIpRecev;
            _networkSession.Source.Kernel.UdpIpSend += KernelTcpIpSend;

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

    private void KernelTcpIpSend(UdpIpTraceData data)
    {
        if (!_procmap.ContainsKey(data.ProcessID)) return;
        Console.WriteLine("TCP Send: " + _procmap[data.ProcessID] + " -> " + data.daddr + " Size: " + data.size);
    }

    private void KernelUdpIpRecev(UdpIpTraceData data)
    {
        if (!_procmap.ContainsKey(data.ProcessID)) return;
        Console.WriteLine("UDP Recv: " + _procmap[data.ProcessID] + " -> " + data.daddr + " Size: " + data.size);
    }

    private void KernelTcpIpRecv(TcpIpTraceData data)
    {
        if (!_procmap.ContainsKey(data.ProcessID)) return;
        Console.WriteLine("TCP Recv: " + _procmap[data.ProcessID] + " -> " + data.daddr + " Size: " + data.size);
    }

    private void KernelTcpIpSend(TcpIpSendTraceData data)
    {
        if (!_procmap.ContainsKey(data.ProcessID)) return;
        Console.WriteLine("TCP Send: " + _procmap[data.ProcessID] + " -> " + data.daddr + " Size: " + data.size);
    }

    public IEnumerable<IProgramData> Produce(float rate)
    {
        foreach (Process process in Process.GetProcesses())
        {
            IProgramData? data = null;
            try
            {
                var mem = process.WorkingSet64 / (1024f * 1024f);//Memory usage in MB as float
                var time = (float)process.TotalProcessorTime.TotalSeconds;
                

                data = new MockProgramDataClass
                {
                    SystemName = Environment.MachineName,
                    ProcessName = process.ProcessName,
                    MemoryUsage = mem,
                    CpuUsage = time,
                    DiskUsage = 0,
                    NetworkUsage = 0,
                    Timespan = time
                };
                if(!_procmap.ContainsKey(process.Id))
                    _procmap[process.Id] = process.ProcessName;
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine($"Access denied to process {process.ProcessName}: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Process {process.ProcessName} exited before reading: {ex.Message}");
            }

            if (data != null)
                yield return data;
        }
    }

    public void Dispose()
    {
        _networkSession.Dispose();
        _networkThread?.Join();
    }
}
