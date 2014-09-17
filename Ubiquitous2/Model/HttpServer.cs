using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UB.Model
{
    // offered to the public domain for any use with no restriction
    // and also with no warranty of any kind, please enjoy. - David Jeske. 

    // simple HTTP explanation
    // http://www.jmarshall.com/easy/http/

    public class HttpProcessor
    {
        public TcpClient Socket { get; set; }
        public HttpServer Server {get;set;}

        private Stream inputStream;
        public StreamWriter OutputStream { get; set; }

        public String HttpMethod { get; set; }
        public String HttpPath { get; set; }
        public String HttpProtocolVersionstring { get; set; }
        public Hashtable HttpHeaders = new Hashtable();

        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        public HttpProcessor(TcpClient s, HttpServer srv)
        {
            this.Socket = s;
            this.Server = srv;
        }

        private string streamReadLine(Stream inputStream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }
        public void Process()
        {
            // we can't use a StreamReader for input, because it buffers up extra data on us inside it's
            // "processed" view of the world, and we want the data raw after the headers
            inputStream = new BufferedStream(Socket.GetStream());

            // we probably shouldn't be using a streamwriter for all output from handlers either
            OutputStream = new StreamWriter(new BufferedStream(Socket.GetStream()));
            try
            {
                ParseRequest();
                ReadHeaders();
                if (HttpMethod.Equals("GET"))
                {
                    HandleGETRequest();
                }
                else if (HttpMethod.Equals("POST"))
                {
                    HandlePOSTRequest();
                }
            }
            catch (Exception e)
            {
                Log.WriteInfo("HttpServer failed to process a request. {0}", e.Message);
                WriteFailure();
            }
            try
            {
                OutputStream.Flush();
            }
            catch
            {
            }
            inputStream = null; 
            OutputStream = null;     
            Socket.Close();
        }

        public void ParseRequest()
        {
            String request = streamReadLine(inputStream);
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            HttpMethod = tokens[0].ToUpper();
            HttpPath = tokens[1];
            HttpProtocolVersionstring = tokens[2];
        }

        public void ReadHeaders()
        {
            String line;
            while ((line = streamReadLine(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    return;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                String name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++; // strip any spaces
                }
                string value = line.Substring(pos, line.Length - pos);
                HttpHeaders[name] = value;
            }
        }

        public void HandleGETRequest()
        {
            Server.HandleGETRequest(this);
        }

        private const int BUF_SIZE = 4096;
        public void HandlePOSTRequest()
        {
            // this post data processing just reads everything into a memory stream.
            // this is fine for smallish things, but for large stuff we should really
            // hand an input stream to the request processor. However, the input stream 
            // we hand him needs to let him see the "end of the stream" at this content 
            // length, because otherwise he won't know when he's seen it all! 

            int content_len = 0;
            MemoryStream ms = new MemoryStream();
            if (this.HttpHeaders.ContainsKey("Content-Length"))
            {
                content_len = Convert.ToInt32(this.HttpHeaders["Content-Length"]);
                if (content_len > MAX_POST_SIZE)
                {
                    throw new Exception(
                        String.Format("POST Content-Length({0}) too big for this simple server",
                          content_len));
                }
                byte[] buf = new byte[BUF_SIZE];
                int to_read = content_len;
                while (to_read > 0)
                {


                    int numread = this.inputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read)); 
                    if (numread == 0)
                    {
                        if (to_read == 0)
                        {
                            break;
                        }
                        else
                        {
                            throw new Exception("client disconnected during post");
                        }
                    }
                    to_read -= numread;
                    ms.Write(buf, 0, numread);
                }
                ms.Seek(0, SeekOrigin.Begin);
            }    
            Server.HandlePOSTRequest(this, new StreamReader(ms));

        }

        public void WriteSuccess(string content_type = "text/html")
        {
            try
            {
                OutputStream.WriteLine("HTTP/1.0 200 OK");
                OutputStream.WriteLine("Content-Type: " + content_type);
                OutputStream.WriteLine("Connection: close");
                OutputStream.WriteLine("");
            }
            catch { }
        }

        public void WriteFailure()
        {
            try
            {
                OutputStream.WriteLine("HTTP/1.0 404 File not found");
                OutputStream.WriteLine("Connection: close");
                OutputStream.WriteLine("");
            }
            catch
            {
            }

        }
    }

    public abstract class HttpServer
    {
        protected int port = 0;
        TcpListener listener;
        bool is_active = true;

        public HttpServer(object port)
        {
            if( port != null )
                int.TryParse(port.ToString(), out this.port);
        }

        public void Listen()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                while (is_active)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    HttpProcessor processor = new HttpProcessor(client, this);
                    var task = Task.Factory.StartNew(() => processor.Process());
                    Thread.Sleep(1);
                }
            }
            catch( SocketException e )
            {
                Log.WriteError("Web server unable to start! {0}", e.Message);
            }
        }
        public void StartHttpServer()
        {
            if (this.port <=0 || this.port > 65535)
            {
                Log.WriteError("Invalid port specified for web server");
                return;
            }
            if( !is_active )
                Task.Factory.StartNew(() => Listen());
            
        }

        public void StopHttpServer()
        {
            if( is_active )
            {
                is_active = false;
                listener.Stop();
            }
        }

        public abstract void HandleGETRequest(HttpProcessor p);
        public abstract void HandlePOSTRequest(HttpProcessor p, StreamReader inputData);
    }


}
