using Core;
using Infrastructure.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure;

public class ApiTracker
{
    public static ApiTracker Instance { get; private set; } = null!;
    private HttpClient client = null!;

    public ApiTracker()
    {
        client = new HttpClient();
    }
    ~ApiTracker()
    {
        client.Dispose();
    }

    public static void Setup()
    {
        Instance = new();
        DBInteract.OnProgramListAdded += Instance.DbSnapshot;
        SystemHistory.Instance.OnSnapshotTaken += Instance.Snapshot;
        NotificationManager.OnNotified += Instance.OnNotified;
    }

    public static void Shutdown()
    {
        DBInteract.OnProgramListAdded -= Instance.DbSnapshot;
        SystemHistory.Instance.OnSnapshotTaken -= Instance.Snapshot;
        NotificationManager.OnNotified -= Instance.OnNotified;
        Instance = null!;
    }

    private void DbSnapshot(List<ProgramData> programs, bool isDataLocal)
    {
        if (!GetUrlSetting(SettingString.ApiDataBaseSnapshot, out string? url)) return;
        client.PostAsJsonAsync(url, (programs, isDataLocal));
    }

    private void OnNotified(Notification notification)
    {
        if (!GetUrlSetting(SettingString.ApiNotification, out string? url)) return;
        client.PostAsJsonAsync(url, notification);
    }

    private void Snapshot(List<IProgramData> list)
    {
        if (!GetUrlSetting(SettingString.ApiSnapshot, out string? url)) return;
        client.PostAsJsonAsync(url, list);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="setting"> The setting to evaluate</param>
    /// <param name="url"> The users setting</param>
    /// <returns>True if a URL setting exists</returns>
    private bool GetUrlSetting(SettingString setting, out string? url)
    {
        url = ConfigManager.ReadSetting(setting);
        return !string.IsNullOrEmpty(url);//Has valid value
    }
}
