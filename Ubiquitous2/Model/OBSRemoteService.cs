using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UB.Utils;
using WebSocket4Net;

namespace UB.Model
{
    public class OBSRemoteService : IService
    {
        private object startStopLock = new object();
        private WebSocketBase obsRemoteWebSocket;
        private Dictionary<string, Action<string,object>> socketMessageHandlers = new Dictionary<string, Action<string, object>>()
        {
            {"StreamStatus", StreamStatusHandler },
            {"StreamStopping", StreamStopHandler },
            {"StreamStarting", StreamStartHandler},
            {@"""current-scene"":", CurrentSceneHandler},
            {@"""SwitchScenes"",", SwitchSceneHandler},
            {@"""SourceChanged"",", SourceChangeHandler},
            {"desktop-volume", DesktopVolumeHandler},
            {"VolumeChanged", VolumeChangeHandler},
            {"authRequired", AuthenticationHandler},
            {@"error"":", ErrorHandler},
        };
        public OBSRemoteService( ServiceConfig config )
        {
            Config = config;
            Status = new StatusBase();
        }
        public bool Start()
        {
            lock(startStopLock)
            {
                if (!Config.Enabled)
                    return false;

                Log.WriteInfo("Starting OBSRemote service");

                if (Status.IsInProgress || Status.IsConnected )
                    return false;

                Status.LastError = String.Empty;

                Status.IsStarting = true;
                Task.Factory.StartNew(() => Connect());
            
                return true;
            }
        }

        public bool Stop()
        {
            lock( startStopLock )
            {

                Log.WriteInfo("Stopping OBSRemote service");
            
                if (Status.IsInProgress)
                    return false;

                Status.IsStopping = true;
                if( !obsRemoteWebSocket.IsClosed )
                    obsRemoteWebSocket.Disconnect();

                Status.ResetToDefault();

                return true;
            }
        }
        public void Restart()
        {
            Log.WriteInfo("Restarting OBS service...");
            
            Status.ResetToDefault();
            Stop();
            Start();

        }
        public Action AfterStart
        {
            get;
            set;
        }

        public ServiceConfig Config
        {
            get;
            set;

        }

        public StatusBase Status
        {
            get;
            set;
        }

        public void GetData(Action<object> callback)
        {
            if( callback != null )
                callback(this);
        }

        #region OBSRemote methods
        private bool Login()
        {
            return true;
        }
        private void Connect()
        {
            if (obsRemoteWebSocket != null && !obsRemoteWebSocket.IsClosed)
            {
                obsRemoteWebSocket.Disconnect();
                obsRemoteWebSocket = null;
            }

            obsRemoteWebSocket = new WebSocketBase()
            {
                Host = Config.GetParameterValue("Host") as string,
                Port = "4444",
                SubProtocol = "obsapi",
                Origin = "http://client.obsremote.com",
                Path = String.Empty,
                ReceiveMessageHandler = HandleOBSRemoteMessage,
            };

            obsRemoteWebSocket.ConnectHandler = () =>
            {
                Status.IsConnected = true;
                Status.IsConnecting = false;
                if( !Login() )
                {
                    Status.IsLoginFailed = true;
                }
                else
                {
                    Status.IsLoggedIn = true;
                    GetVersion();
                    GetAuthRequired();
                    Status.IsStarting = false;
                    //GetSceneList();
                    //GetStreamingStatus();
                    //GetVolumes();
                }
            };

            obsRemoteWebSocket.DisconnectHandler = () =>
            {
                Log.WriteWarning("OBS socket disconnected!");
                if( !Status.IsStopping )
                {
                    if (Status.IsLoginFailed)
                        return;
                    
                    Status.ResetToDefault();
                    Thread.Sleep(1000);
                    Start();

                }
                          
                else
                {
                    Status.ResetToDefault();
                }
            };
            
            Status.IsConnecting = true;
            obsRemoteWebSocket.Connect();
        }

        private void HandleOBSRemoteMessage(string message)
        {
            Log.WriteInfo("OBSRemote service got a message: {0}", message);

            socketMessageHandlers.ToList().ForEach(handler =>
            {
                if (message.Contains(handler.Key))
                    handler.Value(message,this);
            });
        }

        static void StreamStatusHandler( string message, object sender )
        {

        }
        static void StreamStopHandler(string message, object sender)
        {

        }
        static void StreamStartHandler(string message, object sender)
        {

        }
        static void CurrentSceneHandler(string message, object sender)
        {

        }
        static void SwitchSceneHandler(string message, object sender)
        {

        }
        static void SourceChangeHandler(string message, object sender)
        {

        }
        static void DesktopVolumeHandler(string message, object sender)
        {

        }
        static void VolumeChangeHandler(string message, object sender)
        {

        }
        static void ErrorHandler(string message, object sender)
        {
            var packet = JsonConvert.DeserializeObject<OBSRemoteResult>(message);
            var obsRemote = sender as OBSRemoteService;
            if (obsRemote == null || packet == null)
                return;

            if( packet.error.Equals("Not Authenticated",StringComparison.InvariantCultureIgnoreCase))
            {
                obsRemote.Status.LastError = "Authentication required!";
            }
            else if( packet.error.Contains("Authentication Failed") )
            {
                obsRemote.Status.LastError = "Authentication failed!";
                obsRemote.Status.IsLoginFailed = true;
            }
            else
            {
                obsRemote.Status.LastError = packet.error;
            }
        }
        static void AuthenticationHandler(string message, object sender)
        {
            var packet = JsonConvert.DeserializeObject<OBSRemoteAuthenticationRequest>(message);
            var obsRemote = sender as OBSRemoteService;
            if (packet == null || obsRemote == null)
                return;
                        
            var password = obsRemote.Config.GetParameterValue("Password") as string; 
            if( String.IsNullOrEmpty(password))
                return;

            var hash = Crypt.GetSHA256Hash( password + packet.salt).Base64Encode();
            var authResponse = Crypt.GetSHA256Hash(hash + packet.challenge).Base64Encode();

            obsRemote.Send(new OBSRemotePacket("Authenticate", obsRemote.MessageId)
            {
                Auth = authResponse,
            }.ToString());

        }
        private void Send(string message)
        {
            Log.WriteInfo("OBSRemote service is sending a message:{0}", message);
            obsRemoteWebSocket.Send(message);
        }

        public void GetAuthRequired()
        {
            Send(new OBSRemotePacket("GetAuthRequired", MessageId).ToString());
        }
        public void GetVersion()
        {
            Send(new OBSRemotePacket("GetVersion", MessageId).ToString());
        }
        public void GetStreamingStatus()
        {
            Send(new OBSRemotePacket("GetStreamingStatus", MessageId).ToString());
        }
        public void GetSceneList()
        {
            Send(new OBSRemotePacket("GetSceneList", MessageId).ToString());
        }
        public void GetVolumes()
        {
            Send(new OBSRemotePacket("GetVolumes", MessageId).ToString());
        }

        public void SetCurrentScene(string sceneName)
        {
            //TODO: OBSRemote, implement handling of partial Scene name
            Send(new OBSRemotePacket("SetCurrentScene", MessageId)
            {
                SceneName = sceneName
            }.ToString());
        }
        public void SetSourceRenderer(String sourceName, bool enableRender)
        {
            //TODO: OBSRemote, implement handling of partial Source name
            Send(new OBSRemotePacket("SetSourceRender", MessageId)
            {
                SourceName = sourceName,
                EnableRender = enableRender,
            }.ToString());
        }
        private int messageId = 0;

        public string MessageId
        {
            get { return (messageId++).ToString(); }
            set { messageId = int.Parse(value); }
        }
        
        #endregion
    }
    [DataContract]
    public class OBSRemotePacket
    {
        public OBSRemotePacket(string type, string id)
        {
            RequestType = type;
            MessageId = id;
        }
        [DataMember(Name = "request-type")]
        public string RequestType;
        [DataMember(Name = "message-id")]
        public string MessageId;
        [DataMember(Name = "scene-name", EmitDefaultValue = false, IsRequired = false)]
        public string SceneName;
        [DataMember(Name = "source", EmitDefaultValue = false, IsRequired = false)]
        public string SourceName;
        [DataMember(Name = "render", EmitDefaultValue = false, IsRequired = false)]
        public bool EnableRender;
        [DataMember(Name = "auth", EmitDefaultValue = false, IsRequired = false)]
        public string Auth {get;set;}
        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class OBSRemoteResult
    {
        public string status { get; set; }        
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public string error { get; set; }
        [DataMember(Name = "message-id")]
        public string messageId { get; set; }
    }
    //{"status": "error", "error": "Not Authenticated", "message-id": "5"}

    public class OBSRemoteAuthenticationRequest
    {
        public bool authRequired { get; set; }
        public string status { get; set; }
        public string challenge { get; set; }
        public string salt { get; set; }
        [DataMember(Name = "message-id")]
        public string messageId { get; set; }
    }

}
