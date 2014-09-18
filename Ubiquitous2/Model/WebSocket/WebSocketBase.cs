using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Utils;
using WebSocket4Net;

namespace UB.Model
{
    public class WebSocketBase
    {
        private WebSocket socket;

        public WebSocketBase()
        {
            Port = "80";
            PingInterval = 25000;
        }
        public List<KeyValuePair<string, string>> Cookies
        {
            get;
            set;
        }
        public String Host
        {
            get;
            set;
        }
        public String Path
        {
            get;
            set;
        }
        public String Port
        {
            get;
            set;
        }
        public void Disconnect()
        {
            try
            {
                socket.Close();
            }
            catch{}
        }
        public string Origin { get; set; }
        public void Connect()
        {
            String url;

            if (String.IsNullOrEmpty(Port) ||
                String.IsNullOrEmpty(Host))
                return;

            if( Port == "80")
            {
                url = String.Format("ws://{0}{1}", Host, Path);
            }
            else
            {
                url = String.Format("ws://{0}:{1}{2}", Host, Port, Path);
            }

            try
            {
                socket = new WebSocket(
                    url,
                    "",
                    Cookies,
                    null,
                    null,
                    String.IsNullOrWhiteSpace(Origin) ? "http://" + Host : Origin,
                    WebSocketVersion.DraftHybi10
                    );
                socket.Opened += new EventHandler(socket_Opened);
                socket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(socket_MessageReceived);
                socket.Closed += new EventHandler(socket_Closed);
                socket.Error += socket_Error;
                socket.Open();
            }
            catch (Exception e)
            {
                Log.WriteError(String.Format("Websocket connection failed. {0} {1}", url, e.Message));
            }
        }

        void socket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            if (DisconnectHandler != null)
                DisconnectHandler();
        }

        public bool IsClosed { get {return socket.State == WebSocketState.Closed;} }

        void socket_Closed(object sender, EventArgs e)
        {
            if (DisconnectHandler != null)
                DisconnectHandler();
        }
        
        public Action DisconnectHandler { get; set; }
        public Action ConnectHandler { get; set; }
        public Action<string> ReceiveMessageHandler { get; set; }

        public int PingInterval
        {
            set
            {
                if (socket != null)
                {
                    if (value <= 0)
                    {
                        socket.AutoSendPingInterval = 0;
                        socket.EnableAutoSendPing = false;
                    }
                    else
                    {
                        socket.AutoSendPingInterval = value;
                        socket.EnableAutoSendPing = true;
                    }
                }
            }
            get
            {
                return socket.AutoSendPingInterval;
            }
        }
        public void Send(string message)
        {
            socket.Send(message);
        }
        void socket_Opened(object sender, EventArgs e)
        {
            if (ConnectHandler != null)
                ConnectHandler();
        }


        void socket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (ReceiveMessageHandler != null)
                ReceiveMessageHandler(e.Message);
        }
    }
}
