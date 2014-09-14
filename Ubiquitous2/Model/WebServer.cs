using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class WebServer : HttpServer
    {
        private const string webContentFolder = @"web\";
        public WebServer(int httpPort) : base(httpPort)
        {
            Task.Factory.StartNew(() => Listen());
        }
        private readonly HashSet<KeyValuePair<string, string>> ContentTypes = new HashSet<KeyValuePair<string, string>> {
                new KeyValuePair<string,string>(".png", "image/png"),
                new KeyValuePair<string,string>(".gif", "image/gif"),
                new KeyValuePair<string,string>(".jpg", "image/jpeg"),
                new KeyValuePair<string,string>(".jpeg", "image/jpeg"),
                new KeyValuePair<string,string>(".bmp", "image/bmp"),
                new KeyValuePair<string,string>(".js", "application/javascript"),
                new KeyValuePair<string,string>(".html", "text/html"),
                new KeyValuePair<string,string>(".htm", "text/html"),
                new KeyValuePair<string,string>(".map", "text/html"),
                new KeyValuePair<string,string>(".css", "text/css")
        };

        public override void HandleGETRequest(HttpProcessor processor)
        {
            Uri uri = new Uri("http://localhost" + processor.HttpPath);
            String fileName = String.Empty;
            var contentType = String.Empty;

            if( !uri.AbsolutePath.Equals("/"))
            {
                contentType = ContentTypes.Where(pair => uri.AbsolutePath.ToLower().Contains(pair.Key)).Select(item => item.Value).FirstOrDefault();
                fileName = uri.AbsolutePath;
            }
            if( !SendFileToClient(processor, contentType, fileName) )
            {
                SendFileToClient(processor, "text/html", "index.html");
            }
        }

        private bool SendFileToClient( HttpProcessor processor, string contentType, string fileName )
        {
            processor.OutputStream.AutoFlush = true;
            if (contentType != null)
            {
                processor.WriteSuccess(contentType);

                var stream = GetFile(fileName);
                if (stream != null)
                {
                    stream.CopyTo(processor.OutputStream.BaseStream);
                    return true;
                }
            }
            return false;
        }
        private FileStream GetFile( string relativePath )
        {
            var relativeFilePath = webContentFolder + relativePath.Replace("/", @"\");
            try
            {
                return File.OpenRead(relativeFilePath);
            }
            catch
            {
                Log.WriteError("Web server is unable to read a file: {0}", relativeFilePath);
                return null;
            }
        }

        public override void HandlePOSTRequest(HttpProcessor p, System.IO.StreamReader inputData)
        {
        }
    }
}
