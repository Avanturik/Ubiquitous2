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

        public void SaveToFile(string savePath, Action<string> callback)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                if (stream == null && callback != null)
                {
                    callback("File not found in embedded resources: " + resourcePath);
                    return;
                }

                try
                {
                    stream.CopyTo(memoryStream);
                    stream.Flush();
                    memoryStream.Flush();
                    var directory =  Path.GetDirectoryName( savePath);
                    if (!System.IO.Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    File.WriteAllBytes(savePath, memoryStream.ToArray());

                } catch(Exception e)
                {
                    if (callback != null)
                    {
                        callback("Error saving " + resourcePath + " to " + savePath + ": " + e.Message);
                        return;
                    }

                }
            }
        }

    }
}
