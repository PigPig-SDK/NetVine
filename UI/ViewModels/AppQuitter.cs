using Infrastructure;
using Infrastructure.Networking;
using Infrastructure.Networking.Packets;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace UI.ViewModels
{
    public static class AppQuitter
    {
        private static void KillProcessByName(string processName)
        {
            if (string.IsNullOrEmpty(processName)) return;

            Process[] processes = Process.GetProcessesByName(processName);

            foreach (Process p in processes)
            {
                using (p)
                {
                    p.Kill(entireProcessTree: true);
                }
            }
        }

        public static void KillProcessByNameAsync(string processName)
        {
            Task.Run(() => KillProcessByName(processName));
        }

        public static void KillProcessesByRowAsync(TableRow row)
        {
            Host? host = NetworkManager.Instance.Host;
            if(host is not null)
            {
                var clients = host.OurSessions.ToList();
                var packet = Packet.CreatePacket(new CloseProcessPayload(row.AppName));

                HashSet<string> comboUsers = [.. FolderViewData.SelectedUsers()];

                foreach (var client in clients)
                {
                    if (client is HostSession hostSession)
                    {
                        if (!hostSession.IsPasswordAccepted) continue;//Skip user.

                        //Check for combo mode. bleh.
                        if (CombinationModel.IsCombination)
                        {
                            if (comboUsers.Contains(hostSession.Username))
                                client.SendAsync(packet.ToBytes());
                        }
                        else
                        {
                            if(row.SystemName.Equals(hostSession.Username))
                                client.SendAsync(packet.ToBytes());
                        }
                    }
                }
            }

            KillProcessByNameAsync(row.AppName);
        }
    }
}
