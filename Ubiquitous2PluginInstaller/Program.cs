using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ubiquitous2PluginInstaller
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var installer = new Installer();
            var obsDirectory = installer.GetInstallDirectory("Open Broadcaster Software");

            var resourceFile = new ResourceFile("x64.CLRHostPlugin.CLRHost.Interop.dll");
            //resourceFile.SaveToFile(@"c:\test.dll");

        }

    }
}
