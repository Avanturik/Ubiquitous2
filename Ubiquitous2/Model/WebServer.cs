using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
                new KeyValuePair<string,string>(".ico", "image/x-icon"),
                new KeyValuePair<string,string>(".json", "application/json"),
                new KeyValuePair<string,string>(".css", "text/css")
        };
        public Func<Uri, HttpProcessor,bool> GetUserHandler { get; set; }

        public override void HandleGETRequest(HttpProcessor processor)
        {
            Uri uri = new Uri("http://localhost" + processor.HttpPath);
            string contentType = ContentTypes.Where(pair => uri.AbsolutePath.ToLower().Contains(pair.Key)).Select(item => item.Value).FirstOrDefault();
            
            if (String.IsNullOrWhiteSpace(contentType))
                contentType = "text/html";

            if( !uri.AbsolutePath.Equals("/"))
            {
                if (GetUserHandler( uri,processor ) )
                    return;

                if( uri.LocalPath.ToLower().Contains( "/ubiquitous2;component/resources/"))
                {
                    var url = uri.LocalPath;
                    SendResourceToClient(contentType, new Uri(url, UriKind.Relative),processor);
                }
            }

            Log.WriteInfo("Httpserver sending {0} to client as {1}", uri.LocalPath,contentType);

            if (!SendFileToClient(contentType, uri.LocalPath, processor))
            {
                SendFileToClient("text/html", "index.html", processor);
            }
        }

        private bool SendResourceToClient( string contentType, Uri resourceUri,HttpProcessor httpProcessor )
        {
            var resource = Application.GetResourceStream(resourceUri);
            SendStreamToClient(resource.Stream, contentType,httpProcessor);
            return true;
        }

        private bool SendFileToClient(string contentType, string fileName, HttpProcessor httpProcessor)
        {
            if (!String.IsNullOrWhiteSpace(contentType))
            {
                var stream = GetFile(fileName);
                return SendStreamToClient(stream, contentType,httpProcessor);
            }
            return false;
        }

        private bool SendStreamToClient( Stream stream, string contentType, HttpProcessor httpProcessor)
        {
            if (stream != null)
            {
                httpProcessor.OutputStream.AutoFlush = true;
                httpProcessor.WriteSuccess(contentType);

                stream.CopyTo(httpProcessor.OutputStream.BaseStream);
                return true;
            }
            return false;
        }

        public void SendJsonToClient(Stream jsonStream, HttpProcessor httpProcessor)
        {
            httpProcessor.OutputStream.AutoFlush = true;
            httpProcessor.WriteSuccess("application/json");
            jsonStream.CopyTo(httpProcessor.OutputStream.BaseStream);
        }
        private FileStream GetFile( string relativePath )
        {
            var relativeFilePath = (webContentFolder + relativePath.Replace("/", @"\")).Replace(@"\\",@"\");
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
