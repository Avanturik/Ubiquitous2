using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            bool success = false;
            while( !success )
            {
                var errors = Install();
                if( !String.IsNullOrWhiteSpace(errors) )
                {
                    var button = MessageBox.Show(errors, "Error", MessageBoxButtons.AbortRetryIgnore);
                    if( button != DialogResult.Retry )
                        success = true;
                }
                else
                {
                    MessageBox.Show("OBS plugin successfuly installed!", "Ubiquitous2 plugin installer", MessageBoxButtons.OK);
                    success = true;
                }
            }

        }

        private static string Install()
        {
            var files = new FileList() { 
                //x64    
                {"CLRHost.Interop.dll", "x64.CLRHostPlugin.CLRHost.Interop.dll", @"64bit\plugins\CLRHostPlugin\"},
                {"CLRHostPlugin.dll", "x64.CLRHostPlugin.dll", @"64bit\plugins\"},
                {"Ubiquitous2Plugin.dll", "Ubiquitous2Plugin.dll", @"64bit\plugins\CLRHostPlugin\"},                
                //x86
                {"CLRHost.Interop.dll", "x86.CLRHostPlugin.CLRHost.Interop.dll", @"plugins\CLRHostPlugin\"},
                {"CLRHostPlugin.dll", "x86.CLRHostPlugin.dll", @"plugins\"},
                {"Ubiquitous2Plugin.dll", "Ubiquitous2Plugin.dll", @"plugins\CLRHostPlugin\"},
            };

            var installer = new Installer();
            var obsDirectory = installer.GetInstallDirectory("Open Broadcaster Software");

            if (String.IsNullOrWhiteSpace(obsDirectory))
            {
                MessageBox.Show("OBS installation folder not found. Please copy plugin files manually!");
                Application.Exit();
            }

            Stack<string> errors = new Stack<string>();
            foreach (var file in files)
            {
                var resourceFile = new ResourceFile(file.ResourcePath);
                resourceFile.SaveToFile(obsDirectory + file.DestinationPath + file.FileName, (error) =>
                {
                    if (!String.IsNullOrWhiteSpace(error))
                        errors.Push(error);
                });
            }
            if (errors.Count > 0)
                return errors.Aggregate((a, b) => a + b + Environment.NewLine);
            else
                return null;

        }

        public class FileListItem
        {
            public FileListItem(string fileName, string resourcePath, string destinationPath)
            {
                FileName = fileName;
                ResourcePath = resourcePath;
                DestinationPath = destinationPath;
            }
            public string FileName { get; set; }
            public string ResourcePath { get; set; }
            public string DestinationPath { get; set; }
        }

        public class FileList : List<FileListItem>
        {
            public void Add(string fileName, string resourcePath, string destinationPath)
            {
                Add(new FileListItem(fileName, resourcePath, destinationPath));
            }
        }

    }
}
