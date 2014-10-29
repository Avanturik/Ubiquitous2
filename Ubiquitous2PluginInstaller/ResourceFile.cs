using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ubiquitous2PluginInstaller
{
    public class ResourceFile
    {
        private string resourcePath = null;
        private Assembly assembly = null;
        public ResourceFile(string resourcePath)
        {
            assembly = Assembly.GetExecutingAssembly();
            this.resourcePath = "Ubiquitous2PluginInstaller.Files." + resourcePath;

        }

        public void SaveToFile(string savePath)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                stream.Flush();
                memoryStream.Flush();

                File.WriteAllBytes(savePath, memoryStream.ToArray());

            }
        }

    }
}
