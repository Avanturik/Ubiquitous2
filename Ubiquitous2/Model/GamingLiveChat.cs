using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UB.Utils;

namespace UB.Model
{
    //nick=__$anonymous&authToken=__$anonymous
    class GamingLiveChat : IChat
    {
        public event EventHandler<ChatServiceEventArgs> MessageReceived;
        private WebClientBase loginWebClient = new WebClientBase();
        private List<GamingLiveChannel> gamingLiveChannels = new List<GamingLiveChannel>();
        private object counterLock = new object();
        private object channelsLock = new object();
        private List<WebPoller> counterWebPollers = new List<WebPoller>();
        private bool isAnonymous = false;
        public GamingLiveChat(ChatConfig config)
        {
            Config = config;
            ContentParsers = new List<Action<ChatMessage, IChat>>();
            ChatChannels = new List<string>();
            Emoticons = new List<Emoticon>();
            Status = new StatusBase();
            Users = new Dictionary<string, ChatUser>();

            ContentParsers.Add(MessageParser.ParseURLs);
            ContentParsers.Add(MessageParser.ParseSimpleImageTags);

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
            Log.WriteInfo("Starting Gaminglive.tv chat");
            if (Status.IsStarting || Status.IsConnected || Status.IsLoggedIn || Config == null)
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
        void StopCounterPoller(string channelName)
        {
            UI.Dispatch(() => Status.ToolTips.RemoveAll(t => t.Header == channelName));
            var poller = counterWebPollers.FirstOrDefault(p => p.Id == channelName);
            if( poller != null)
            {
                poller.Stop();
                lock(counterLock)
                    counterWebPollers.Remove(poller);
            }
        }
        private void JoinChannels()
        {


            var channels = Config.Parameters.StringArrayValue("Channels").Select(chan => "#" + chan.ToLower().Replace("#","")).ToArray();


            if (NickName != null && !NickName.Equals("__$anonymous", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!channels.Contains("#" + NickName.ToLower()))
                    channels = channels.Union(new String[] { NickName.ToLower() }).ToArray();
            }



            foreach( var channel in channels )
            {

                if (Status.IsStopping)
                    return;

                Log.WriteInfo("Joining {0}@gaminglive", channel);
                lock (channelsLock)
                    gamingLiveChannels.RemoveAll(chan => chan.ChannelName.Equals(channel, StringComparison.InvariantCultureIgnoreCase));

                var gamingLiveChannel = new GamingLiveChannel(this);
                gamingLiveChannel.ReadMessage = ReadMessage;
                gamingLiveChannel.LeaveCallback = (glChannel) => {
                    StopCounterPoller(glChannel.ChannelName);
                    
                    lock(channelsLock)
                        gamingLiveChannels.RemoveAll(item => item.ChannelName == glChannel.ChannelName);
                    ChatChannels.RemoveAll(chan => chan == null);
                    ChatChannels.RemoveAll(chan => chan.Equals(glChannel.ChannelName, StringComparison.InvariantCultureIgnoreCase));
                    if (RemoveChannel != null)
                        RemoveChannel(gamingLiveChannel.ChannelName, this);

                    if (!Status.IsStopping)
                        Restart();
                };
                lock(channelsLock)
                {

                    if( !gamingLiveChannels.Any(c => c.ChannelName == channel ))
                    {
                        gamingLiveChannel.Join((glChannel) => {
                            if (Status.IsStopping)
                                return;
                            Status.IsConnected = true;
                            lock(channelsLock)
                                gamingLiveChannels.Add(glChannel);
                            if (glChannel.ChannelName.Equals("#" + NickName, StringComparison.InvariantCultureIgnoreCase))
                                Status.IsLoggedIn = true;

                            ChatChannels.RemoveAll(chan => chan == null);
                            ChatChannels.RemoveAll(chan => chan.Equals(glChannel.ChannelName, StringComparison.InvariantCultureIgnoreCase));
                            ChatChannels.Add((glChannel.ChannelName));
                            if (AddChannel != null)
                                AddChannel(gamingLiveChannel.ChannelName, this);

                            WatchChannelStats(gamingLiveChannel.ChannelName);

                        }, NickName, channel, (String)Config.GetParameterValue("AuthToken"));
                    }
                }
            }
        }
        private void ReadMessage( ChatMessage message )
        {
            if (MessageReceived != null)
            {
                if (ContentParsers != null)
                    ContentParsers.ForEach(parser => parser(message, this));

                MessageReceived(this, new ChatServiceEventArgs() { Message = message });

            }
        }
        
        private bool Login()
        {
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
                isAnonymous = true;
                return true;
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
            var userName = Config.GetParameterValue("Username") as string;

            if (String.IsNullOrWhiteSpace(authToken) || String.IsNullOrWhiteSpace(userName))
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
            Log.WriteInfo("Stopping Gaminglive.tv chat");

            if (Status.IsStopping)
                return false;

            lock(channelsLock)
                gamingLiveChannels.ForEach(chan => {
                    StopCounterPoller(chan.ChannelName);
                    chan.Leave(); 
                });
            ChatChannels.Clear();
            Status.ResetToDefault();
            return true;
        }

        public bool Restart()
        {
            if (Status.IsStopping || Status.IsStarting)
                return false;

            Stop();
            Start();
            Status.ResetToDefault();
            return true;
        }

        public bool SendMessage(ChatMessage message)
        {
            var gamingLiveChannel = gamingLiveChannels.FirstOrDefault(channel => channel.ChannelName.Equals(message.Channel, StringComparison.InvariantCultureIgnoreCase));
            if (gamingLiveChannel != null)
            {
                Task.Factory.StartNew(() => gamingLiveChannel.SendMessage(message));
            }
                

            return true;
        }

        public void WatchChannelStats(string channel)
        {
            var poller = new WebPoller()
            {
                Id = channel,
                Uri = new Uri(String.Format(@"http://api.gaminglive.tv/channels/{0}", channel.Replace("#", ""))),
            };

            UI.Dispatch(() => Status.ToolTips.RemoveAll(t => t.Header == poller.Id));
            UI.Dispatch(() => Status.ToolTips.Add(new ToolTip(poller.Id, "")));

            poller.ReadStream = (stream) =>
            {
                lock (counterLock)
                {
                    var channelInfo = Json.DeserializeStream<GamingLiveChannelStats>(stream);
                    poller.LastValue = channelInfo;
                    var viewers = 0;
                    foreach (var webPoller in counterWebPollers.ToList())
                    {
                        var streamInfo = this.With(x => (GamingLiveChannelStats)webPoller.LastValue)
                            .With(x => x.state);

                        var tooltip = Status.ToolTips.ToList().FirstOrDefault(t => t.Header == webPoller.Id);
                        if (tooltip == null)
                            return;

                        if (streamInfo != null)
                        {
                            viewers += streamInfo.viewers;
                            tooltip.Text = streamInfo.viewers.ToString();
                            tooltip.Number = streamInfo.viewers;
                        }
                        else
                        {
                            tooltip.Text = "0";
                            tooltip.Number = 0;
                        }

                    }
                    UI.Dispatch(() => Status.ViewersCount = viewers);
                }
            };
            poller.Start();

            lock(counterLock)
            {
                counterWebPollers.RemoveAll(p => p.Id == poller.Id);
                counterWebPollers.Add(poller);
            }
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


        public Func<string, object> RequestData
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public bool HideViewersCounter
        {
            get;
            set;

        }
    }

    public class GamingLiveChannel
    {

        private WebSocketBase webSocket;
        private IChat _chat;
        bool isAnonymous = false;
        public GamingLiveChannel(IChat chat)
        {
            _chat = chat;
        }
        public void Join(Action<GamingLiveChannel> callback, string nickName, string channel, string authToken)
        {
            isAnonymous = nickName == null || nickName.Equals("__$anonymous",StringComparison.InvariantCultureIgnoreCase) 
                || String.IsNullOrWhiteSpace(nickName) 
                || String.IsNullOrWhiteSpace(authToken);

            if( isAnonymous )
            {
                nickName = "__$anonymous";
                authToken = "__$anonymous";
            }
            ChannelName = "#" + channel.Replace("#", "");
            webSocket = new WebSocketBase();
            webSocket.Host = "54.76.144.150";
            webSocket.PingInterval = 0;
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
            Log.WriteInfo("gaminglive raw message: {0}", rawMessage);
            if( !String.IsNullOrWhiteSpace(rawMessage))
            {
                var json = JToken.Parse(rawMessage);
                var nickName = (string)json.Value<dynamic>("user").nick;
                var text = json.Value<string>("message");
                var memberType = json.Value<string>("mtype");
                if (memberType != null && memberType.Equals("BOT", StringComparison.InvariantCultureIgnoreCase))
                    return;

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
            if (isAnonymous)
                return;
            dynamic jsonMessage = new { message = message.Text, color = "orange" };
            webSocket.Send(JsonConvert.SerializeObject(jsonMessage));
        }
    }
    #region Gaminglive json classes
    public class GamingLiveGame
    {
        public string id { get; set; }
        public string name { get; set; }
        public string miniImg { get; set; }
        public string largeImg { get; set; }
    }

    public class GamingLiveStream
    {
        public string rootUrl { get; set; }
        public List<string> qualities { get; set; }
    }

    public class GamingLiveState
    {
        public int viewers { get; set; }
        public string thumbnailUrl { get; set; }
        public GamingLiveStream stream { get; set; }
    }

    public class GamingLiveChannelStats
    {
        public string slug { get; set; }
        public string name { get; set; }
        public string owner { get; set; }
        public string offlineImg { get; set; }
        public GamingLiveGame game { get; set; }
        public GamingLiveState state { get; set; }
        public int views { get; set; }
        public int followers { get; set; }
    }
    #endregion
}
