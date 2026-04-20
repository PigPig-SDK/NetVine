using Core;
using Infrastructure;
using Infrastructure.Networking;
using Infrastructure.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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

    }
}
