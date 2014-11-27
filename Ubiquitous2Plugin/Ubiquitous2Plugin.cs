using CLROBS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Diagnostics;
using UB.Model;
using UB.Utils;

namespace Ubiquitous2Plugin
{
    public class Ubiquitous2Plugin : AbstractPlugin
    {
        private const int VERSION = 1;

        public Ubiquitous2Plugin()
        {
            Name = "Ubiquitous2 Chat Source Plugin";
            Description = "Adds Ubiquitous2 chat as OBS source";
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public override bool LoadPlugin()
        {
            Debug.Print("Ubiquitous2 OBS plugin: loading...");
            Task.Run(() => CheckUpdates());
            API.Instance.AddImageSourceFactory(new Ubiquitous2Factory());
            //API.Instance.AddSettingsPane(new Ubiquitous2SettingsPane());
            return true;
        }
        private async void CheckUpdates()
        {
            using( WebClientBase webClient = new WebClientBase() )
            {
                int currentVersion = 0;
                Log.WriteInfo("OBSPlugin: Downloading current version info");

                var content = webClient.Download("http://www.xedocproject.com/apps/ubiquitous2obs/ub2plugin.ver");


                if (content != null)
                {
                    content = Re.GetSubString(content, @"(\d+)");

                    int.TryParse(content, out currentVersion);

                    Log.WriteInfo("OBSPlugin: current version number is {0}", currentVersion);
                    bool isError = false;
                    string saveFileName = Path.GetTempPath() + @"Ubiquitous2PluginInstaller.exe";
                    if (currentVersion > VERSION)
                    {
                        try
                        {
                            if (File.Exists(saveFileName))
                                File.Delete(saveFileName);
                        }
                        catch( Exception e)
                        {
                            isError = true;
                            Log.WriteError("OBSPlugin: error deleting old installer {0}", e.Message);
                        }

                        await Task.Run(() => 
                            {
                                try
                                {
                                    webClient.DownloadFile(@"http://www.xedocproject.com/apps/ubiquitous2obs/Ubiquitous2PluginInstaller.exe",
                                            Path.GetTempPath() + @"Ubiquitous2PluginInstaller.exe");
                                }
                                catch(Exception e) {
                                    isError = true;
                                    Log.WriteError("OBSPlugin: Error downloading update {0}", e.Message);
                                }

                            });

                        if( !isError )
                        {
                            var result = MessageBox.Show("New version of Ubiquitous2 plugin is available. Install it now ?", "New Ubiquitous2 plugin", MessageBoxButtons.YesNo);
                            if (result == DialogResult.No)
                                return;
                            var installDir = AssemblyDirectory.Replace(@"plugins\CLRHostPlugin","");

                            ProcessStartInfo startInfo = new ProcessStartInfo(saveFileName,string.Format("\"{0}\"", installDir));
                            Process.Start(startInfo);
                        }

                    }
                }
                else
                {
                    Log.WriteError("OBSPlugin: couldn't get current plugin version");
                }
            }
        }

        public override void UnloadPlugin()
        {
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }
    }
}
