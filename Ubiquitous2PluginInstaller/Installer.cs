using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Diagnostics;

namespace Ubiquitous2PluginInstaller
{
    public class Installer
    {
        private const string UninstallRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
        public string GetInstallDirectory( string partialName )
        {
            if (String.IsNullOrWhiteSpace(partialName))
                return null;

            RegistryKey rootKey = Registry.LocalMachine.OpenSubKey(UninstallRegistryKey);
            var nameList = rootKey.GetSubKeyNames();
            string result = null;

            Parallel.For(0, nameList.Length, (i, loopState) =>
            {
                RegistryKey regKey = rootKey.OpenSubKey(nameList[i]);
                var displayName = regKey.GetValue("DisplayName");
                if (displayName != null && displayName.ToString().Contains(partialName)) 
                {
                    var installLocation = regKey.GetValue("InstallLocation");
                    if( installLocation != null )
                    {
                        result = regKey.GetValue("InstallLocation").ToString();
                        loopState.Stop();
                    }
                }
            });

            return result;
        }
    }
}
