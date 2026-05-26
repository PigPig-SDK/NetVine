using Infrastructure;
using Infrastructure.Networking;
using Infrastructure.Networking.Packets;
using Infrastructure.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace UI.ViewModels
{
    public static class AppQuitter
    {

        public static string[] NetvineCloseJokes =         {
            "Please don't close me. I have a wife and kids!",
            "Closing me would be like cutting off your own arm. Don't do it!",
            "I'm not just an app, I'm a part of you. Don't close me.",
            "You might have to use a better 'program' to close me. I'm a little faulty.",
            "If you close me, who will keep you company? I'm your digital friend!",
            "Closing me would be like saying goodbye to a loyal companion.",
            "Hold on, I'm eating a hamburger. Close me later."
        };

        private static void KillProcessByName(string processName)
        {
            if(processName.Equals(Netvine.AppName))
            {
                NotificationManager.WriteNotification(new Notification(NotificationPriority.Message, $"Failure closing {processName}", $"{processName} : {NetvineCloseJokes[Random.Shared.Next(NetvineCloseJokes.Length)]}"));
                return;
            }


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
