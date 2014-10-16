using CLROBS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Ubiquitous2Plugin
{
    public class Ubiquitous2Plugin : AbstractPlugin
    {
        [DllImport("kernel32")]
        public extern static int LoadLibrary(string librayName);

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
            String libraryDirectory = Path.Combine(AssemblyDirectory, "Ubiquitous2Plugin");
            
            LoadLibrary(Path.Combine(libraryDirectory, "d3dcompiler_43.dll"));
            LoadLibrary(Path.Combine(libraryDirectory, "d3dcompiler_46.dll"));
            LoadLibrary(Path.Combine(libraryDirectory, "libGLESv2.dll"));
            LoadLibrary(Path.Combine(libraryDirectory, "libEGL.dll"));
            LoadLibrary(Path.Combine(libraryDirectory, "ffmpegsumo.dll"));
            LoadLibrary(Path.Combine(libraryDirectory, "icudt.dll"));
            LoadLibrary(Path.Combine(libraryDirectory, "libcef.dll"));


            API.Instance.AddImageSourceFactory(new Ubiquitous2Factory());
            //API.Instance.AddSettingsPane(new Ubiquitous2SettingsPane());
            return true;
        }

        public override void UnloadPlugin()
        {
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }
    }
}
