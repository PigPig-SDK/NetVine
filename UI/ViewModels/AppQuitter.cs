using Core;
using Infrastructure;
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

            string sysName = SystemHistory.Instance.SystemName;//?
            if (row.SystemName != sysName)
            {
                //some networking logic can be put here for other systems.
                Core.Debug.Log($"Permission to kill process denied -- must be on your own machine {row.SystemName} != {sysName} ");
            }
            else
            {
                await Task.Run(() => KillProcessByName(row.AppName));
            }

        }

    }
}
