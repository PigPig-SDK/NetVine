using Core;
using Infrastructure.Networking;
using Infrastructure.Networking.Packets;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UI.ViewModels
{


    public static class AppQuitter
    {

        // on the current PC:
        public static void KillProcessByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            Process[] processes = Process.GetProcessesByName(name);

            foreach (Process p in processes)
            {
                using (p)
                {
                    p.Kill(entireProcessTree: true);
                }
            }
        }

        public static async Task KillProcessByNameAsync(string name)
        {
            Core.Debug.Log($"Ending processes: {name}");
            await Task.Run(() => KillProcessByName(name));
            Core.Debug.Log($"{name} Ended");
        }

        public static async Task KillProcessesByRowAsync(TableRow row)
        {
            Core.Debug.Log($"Ending processes: {row.AppName}");

            Host? host = NetworkManager.Instance.Host;
            if(host is not null)
            {
                var packet = Packet.CreatePacket(new CloseProcessPayload(row.AppName));
                host.Multicast(packet.ToBytes());
            }

            await Task.Run(() => KillProcessByName(row.AppName));

        }

        public static async Task KillProcessByKeyAsync(Tuple<string, string>? key)
        {
            if (key == null)
            {
                Core.Debug.Log("null key");
                return;
            }


            string sysName = SystemHistory.Instance.SystemName;

            if (key.Item1 != sysName)
            {
                //some networking logic can be put here for other systems.
                Core.Debug.Log($"Permission to kill process denied -- must be on your own machine {key.Item1} != {sysName} ");
            }
            else
            {
                await Task.Run(() => KillProcessByName(key.Item2));
            }

        }

    }
}
