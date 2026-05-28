using Core;
using Infrastructure.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure;

public static class NotificationTracker
{
    public static void Setup()
    {
        SystemHistory.Instance.OnSnapshotTakenTotal += TotalSnapshot;
        SystemHistory.Instance.OnSnapshotTaken += Snapshot;
    }

    private static void Snapshot(List<IProgramData> obj)
    {
        foreach (IProgramData programData in obj)
        {
            if (programData is not ProgramData progdata) continue;
            MonitorProcess(progdata,
                SettingFloat.NoteIndividualCpuUsage,
                SettingFloat.NoteIndividualRamUsage,
                SettingFloat.NoteIndividualDiskUsage,
                SettingFloat.NoteIndividualNetworkUsage,
                (ResourceTypes resource) => {
                    NotificationManager.ProgramResourceNotification(resource, progdata);
                });
        }
    }

    private static void TotalSnapshot(IProgramData total, int count)
    {
        if (total is not ProgramData progdata) return;
        MonitorProcess(progdata,
            SettingFloat.NoteCpuUsage,
            SettingFloat.NoteRamUsage,
            SettingFloat.NoteDiskUsage,
            SettingFloat.NoteNetworkUsage,
            (ResourceTypes resource) => {
                NotificationManager.SystemResourceNotification(resource);
            });
    }

    private static void MonitorProcess(ProgramData data,
        SettingFloat cpuUsage,
        SettingFloat memoryUsage,
        SettingFloat diskUsage,
        SettingFloat networkUsage,
        Action<ResourceTypes> OnExceeding)
    {
        float cpuCheck = ConfigManager.ReadSetting(cpuUsage);
        float diskCheck = ConfigManager.ReadSetting(diskUsage);
        float networkCheck = ConfigManager.ReadSetting(networkUsage);
        float memoryCheck = ConfigManager.ReadSetting(memoryUsage);

        if (data.CpuUsage > cpuCheck && cpuCheck != 0)
            OnExceeding.Invoke(ResourceTypes.CPU);
        else if (data.DiskUsage > diskCheck && diskCheck != 0)
            OnExceeding.Invoke(ResourceTypes.Disk);
        else if (data.NetworkUsage > networkCheck && networkCheck != 0)
            OnExceeding.Invoke(ResourceTypes.Network);
        else if (data.MemoryUsage > memoryCheck && memoryCheck != 0)
            OnExceeding.Invoke(ResourceTypes.RAM);
    }
}
