using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UB.Utils;

namespace UB.Model
{
    class GamingLiveChat : IChat
    {
        public event EventHandler<ChatServiceEventArgs> MessageReceived;
        private WebClientBase loginWebClient = new WebClientBase();
        private List<GamingLiveChannel> gamingLiveChannels = new List<GamingLiveChannel>();

        public GamingLiveChat(ChatConfig config)
        {
            Config = config;
            ChatChannels = new List<string>();
            Emoticons = new List<Emoticon>();
            Status = new StatusBase();
            Users = new Dictionary<string, ChatUser>();
            Enabled = Config.Enabled;
        }
        public string ChatName
        {
            get;
            set;
        }

        public string IconURL
        {
            get;
            set;
        }

        public string NickName
        {
            get;
            set;
        }

        public bool Enabled
        {
            get;
            set;
        }

        public bool Start()
        {
            if( Status.IsStarting || Status.IsConnected || Status.IsLoggedIn || Config == null)
            {
                return true;
            }
            Status.IsStarting = true;

            if( Login() )
            {
                Status.IsConnecting = true;
                Task.Factory.StartNew( () => JoinChannels());
            }
            return false;
        }
        private void JoinChannels()
        {
            var channels = Config.Parameters.StringArrayValue("Channels").Select(chan => "#" + chan.ToLower().Replace("#","")).ToArray();
            if( !channels.Contains( "#" + NickName.ToLower()))
                channels = channels.Union(new String[] { NickName.ToLower() }).ToArray();

            foreach( var channel in channels )
            {
                var gamingLiveChannel = new GamingLiveChannel(this);
                gamingLiveChannel.ReadMessage = ReadMessage;
                gamingLiveChannel.LeaveCallback = (glChannel) => {
                    gamingLiveChannels.RemoveAll(item => item.ChannelName == glChannel.ChannelName);
                };
                
                gamingLiveChannel.Join((glChannel) => {
                    gamingLiveChannels.Add(glChannel);
                }, NickName, channel, (String)Config.GetParameterValue("AuthToken"));
            }
        }
        private void ReadMessage( ChatMessage message )
        {
            if (MessageReceived != null)
                MessageReceived(this, new ChatServiceEventArgs() { Message = message });
        }
        
        private bool Login()
        {
            var authToken = Config.GetParameterValue("AuthToken") as string;
            if( !LoginWithToken())
            {
                if (!LoginWithUsername())
                {
                    Status.IsLoginFailed = true;
                    return false;
                }
                else
                {
                    Status.IsLoggedIn = true;
                    return true;
                }
            }     
            else
            {
                return true;
            }
        }
        public bool LoginWithUsername()
        {
            var userName = Config.GetParameterValue("Username") as string;
            var password = Config.GetParameterValue("Password") as string;

            if( String.IsNullOrWhiteSpace(userName) || String.IsNullOrWhiteSpace(password))
            {
                Log.WriteError("Unable to login to Gaminglive.tv. Please set correct credentials!");
                return false;
            }
            NickName = userName;
            
            var authString =  String.Format(@"{{""email"":""{0}"",""password"":""{1}""}}", userName, password);

            SetCommonHeaders();
            var authToken = this.With(x => loginWebClient.Upload("https://api.gaminglive.tv/auth/session", authString))
                                .With(x => JToken.Parse(x))
                                .With(x => x.Value<string>("authToken"));

            if (authToken == null)
            {
                Log.WriteError("Login to gaminglive.tv failed.");
                return false;
            }
            else
            {
                Config.SetParameterValue("AuthToken", authToken);
                return true;
            }
        }
        public bool LoginWithToken()
        {
            var authToken = (string)Config.GetParameterValue("AuthToken");
            if( String.IsNullOrWhiteSpace( authToken ))
                return false;

            SetCommonHeaders();
            loginWebClient.Headers["Auth-Token"] = authToken;

            var response = this.With(x => loginWebClient.Download("https://api.gaminglive.tv/auth/me"))
                                .With(x => JToken.Parse(x));

            if (response == null)
                return false;

            var isOk = response.Value<bool>("ok");
            NickName = (string)response.Value<dynamic>("user").login;
        
            if( isOk && !String.IsNullOrWhiteSpace(NickName) )
            {
                return true;
            }

            Config.SetParameterValue("AuthToken", String.Empty);
            return false;
        }
        private void SetCommonHeaders()
        {
            loginWebClient.Headers["Content-Type"] = @"application/json;charset=UTF-8";
            loginWebClient.Headers["Accept"] = @"application/json, text/plain, */*";
            loginWebClient.Headers["Accept-Encoding"] = "gzip,deflate";
        }
        public bool Stop()
        {
            gamingLiveChannels.ForEach(chan => chan.Leave());
            Status.ResetToDefault();
            return true;
        }

        public bool Restart()
        {
            return true;
        }

        public bool SendMessage(ChatMessage message)
        {
            var gamingLiveChannel = gamingLiveChannels.FirstOrDefault(channel => channel.ChannelName.Equals(message.Channel, StringComparison.InvariantCultureIgnoreCase));
            if (gamingLiveChannel != null)
                Task.Factory.StartNew(() => gamingLiveChannel.SendMessage(message));

            return true;
        }

        public Dictionary<string, ChatUser> Users
        {
            get;
            set;
        }

        public List<string> ChatChannels
        {
            get;
            set;
        }

        public Action<string, IChat> AddChannel
        {
            get;
            set;
        }

        public Action<string, IChat> RemoveChannel
        {
            get;
            set;
        }

        public List<Action<ChatMessage, IChat>> ContentParsers
        {
            get;
            set;
        }

        public List<Emoticon> Emoticons
        {
            get;
            set;
        }

        public void DownloadEmoticons(string url)
        {
            
        }

        public ChatConfig Config
        {
            get;
            set;
        }

        public StatusBase Status
        {
            get;
            set;
        }
    }

    public class GamingLiveChannel
    {

        private WebSocketBase webSocket;
        private Action<GamingLiveChannel> joinCallback;
        private IChat _chat;
        public GamingLiveChannel(IChat chat)
        {
            _chat = chat;
        }
        public void Join(Action<GamingLiveChannel> callback, string nickName, string channel, string authToken)
        {
            if( String.IsNullOrWhiteSpace(channel) || String.IsNullOrWhiteSpace(authToken) )
                return;

            ChannelName = "#" + channel.Replace("#", "");
            webSocket = new WebSocketBase();
            webSocket.Host = "54.76.144.150";
            webSocket.Origin = "http://www.gaminglive.tv";
            webSocket.Path = String.Format("/chat/{0}?nick={1}&authToken={2}", ChannelName.Replace("#",""), nickName, authToken );
            webSocket.ConnectHandler = () =>
            {
                if( callback != null )
                    callback(this);
            };

            webSocket.DisconnectHandler = () =>
            {
                if (LeaveCallback != null)
                    LeaveCallback(this);
            };
            webSocket.ReceiveMessageHandler = ReadRawMessage;
            webSocket.Connect();
        }
        private void ReadRawMessage(string rawMessage)
        {
            if( !String.IsNullOrWhiteSpace(rawMessage))
            {
                var json = JToken.Parse(rawMessage);
                var nickName = (string)json.Value<dynamic>("user").nick;
                var text = json.Value<string>("message");

                if (String.IsNullOrWhiteSpace(nickName) || String.IsNullOrWhiteSpace(text))
                    return;

                if(ReadMessage != null)
                    ReadMessage(new ChatMessage() { 
                        Channel = ChannelName,
                        ChatIconURL = _chat.IconURL,
                        ChatName = _chat.ChatName,
                        FromUserName = nickName,
                        HighlyImportant = false,
                        IsSentByMe = false,
                        Text = text
                    });

            }
        }
        public string ChannelName { get; set; }
        public void Leave()
        {
            webSocket.Disconnect();
        }

        public Action<GamingLiveChannel> LeaveCallback { get; set; }
        public Action<ChatMessage> ReadMessage { get; set; }
        public void SendMessage( ChatMessage message )
        {
            webSocket.Send(message);
        }
    }
}
