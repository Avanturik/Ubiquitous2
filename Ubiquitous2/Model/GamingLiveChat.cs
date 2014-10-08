using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UB.Utils;

namespace UB.Model
{
    class GamingLiveChat : IChat, IStreamTopic
    {
        public event EventHandler<ChatServiceEventArgs> MessageReceived;
        private WebClientBase loginWebClient = new WebClientBase();
        private List<GamingLiveChannel> gamingLiveChannels = new List<GamingLiveChannel>();
        private GamingLiveGameList jsonGames;
        private object counterLock = new object();
        private object channelsLock = new object();
        private List<WebPoller> counterWebPollers = new List<WebPoller>();
        private bool isAnonymous = false;
        private object lockSearch = new object();
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

            Info = new StreamInfo()
            {
                HasDescription = false,
                HasGame = true,
                CurrentGame = new Game(),
                HasTopic = true,
                ChatName = Config.ChatName,
            };

            Games = new ObservableCollection<Game>();

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

            if (Status.IsStarting || Status.IsConnected || Status.IsLoggedIn || Config == null)
            {
                return true;
            }

            Log.WriteInfo("Starting Gaminglive.tv chat");
            Status.ResetToDefault();
            Status.IsStarting = true;
            ChatChannels.Clear();
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

                    Log.WriteInfo("Remove gaminglive channel {0}", glChannel.ChannelName);

                    if (RemoveChannel != null)
                        RemoveChannel(glChannel.ChannelName, this);

                    if (!Status.IsStarting && !Status.IsStopping)
                        Restart();
                };
                if( !gamingLiveChannels.Any(c => c.ChannelName == channel ))
                {
                    gamingLiveChannel.Join((glChannel) => {
                        if (Status.IsStopping)
                            return;
                        Status.IsConnected = true;


                        if(!isAnonymous)
                            Status.IsLoggedIn = true;

                        if (RemoveChannel != null)
                            RemoveChannel(glChannel.ChannelName, this);

                        ChatChannels.RemoveAll(chan => chan == null);
                        ChatChannels.RemoveAll(chan => chan.Equals(glChannel.ChannelName, StringComparison.InvariantCultureIgnoreCase));
                        ChatChannels.Add((glChannel.ChannelName));

                        Log.WriteInfo("Add gaminglive channel {0}", glChannel.ChannelName);
                        if (AddChannel != null)
                            AddChannel(glChannel.ChannelName, this);

                        WatchChannelStats(glChannel.ChannelName);

                    }, NickName, channel, (String)Config.GetParameterValue("AuthToken"));

                    lock (channelsLock)
                        gamingLiveChannels.Add(gamingLiveChannel);
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
            try
            {
                if( !LoginWithToken())
                {
                    if (!LoginWithUsername())
                    {
                        Status.IsLoginFailed = true;
                        return false;
                    }
                }     
            }
            catch(Exception e)
            {
                Log.WriteInfo("Gaminglive authorization exception {0}", e.Message);
                return false;
            }

            if (!isAnonymous)
            {
                GetTopic();
                GetGameList();
            }

            return true;
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
                isAnonymous = false;
                Config.SetParameterValue("AuthToken", authToken);
                Config.SetParameterValue("AuthTokenCredentials", userName + password);
                return true;

            }
        }
        public bool LoginWithToken()
        {
            var authToken = (string)Config.GetParameterValue("AuthToken");
            var userName = Config.GetParameterValue("Username") as string;            
            var password = Config.GetParameterValue("Password") as string;
            var tokenCredentials = Config.GetParameterValue("AuthTokenCredentials") as string;
            
            if (tokenCredentials != userName + password)
                return false;


            if (String.IsNullOrEmpty(userName))
            {
                isAnonymous = true;
                return true;
            }

            if (String.IsNullOrWhiteSpace(authToken))
                return false;

            SetCommonHeaders();
            loginWebClient.Headers["Auth-Token"] = authToken;

            var response = this.With(x => loginWebClient.Download("https://api.gaminglive.tv/auth/me"))
                .With(x => String.IsNullOrWhiteSpace(x)?null:x)
                .With(x => JToken.Parse(x));

            if (response == null)
            {
                return false;
            }

            var isOk = response.Value<bool>("ok");
            NickName = (string)response.Value<dynamic>("user").login;
        
            if( isOk && !String.IsNullOrWhiteSpace(NickName) )
            {
                isAnonymous = false;
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
            if (!Enabled)
                Status.ResetToDefault();

            if (Status.IsStopping)
                return false;

            Log.WriteInfo("Stopping Gaminglive.tv chat");
            Status.IsStopping = true;
            Status.IsStarting = false;

            lock(channelsLock)
                gamingLiveChannels.ForEach(chan => {
                    StopCounterPoller(chan.ChannelName);
                    chan.Leave();
                    if (RemoveChannel != null)
                        RemoveChannel(chan.ChannelName, this);
                });
            ChatChannels.Clear();
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
            get;
            set;
        }

        public bool HideViewersCounter
        {
            get;
            set;

        }

        public StreamInfo Info
        {
            get;
            set;
        }

        public ObservableCollection<Game> Games
        {
            get;
            set;

        }

        public string SearchQuery
        {
            get;
            set;

        }

        public void QueryGameList(string gameName, Action callback)
        {
            lock (lockSearch)
            {
                Games.Clear();
                jsonGames.games.Where(game => game.name.ToLower().StartsWith(gameName.ToLower())).Select(game => new Game() { Id = game.id, Name = game.name }).ToList().ForEach(game => Games.Add(game));
                if (callback != null)
                    UI.Dispatch(() => callback());
            }
        }

        JToken GetLiveStreamInfo()
        {
            var getUrl = @"https://api.gaminglive.tv/channels/{0}?authToken={1}";
            var userName = Config.GetParameterValue("Username") as string;
            var authToken = Config.GetParameterValue("AuthToken") as string;
            loginWebClient.ContentType = ContentType.JsonUTF8;
            return this.With(x => loginWebClient.Download(String.Format(getUrl, HttpUtility.UrlEncode(userName.ToLower()), authToken)))
                            .With(x => JToken.Parse(x));

        }
        public void GetTopic()
        {
            if (!Status.IsLoggedIn)
                return;

            Task.Factory.StartNew(() =>
            {
                var jsonInfo = GetLiveStreamInfo();             
                
                if (jsonInfo == null)
                    return;

                Info.Topic = jsonInfo["name"].ToObject<string>();
                var game = jsonInfo["game"];
                if( game != null )
                {
                    Info.CurrentGame.Name = game["name"].ToObject<string>();
                    Info.CurrentGame.Id = game["id"].ToObject<string>();
                }
                
                Info.CanBeRead = true;
                Info.CanBeChanged = true;

                if (StreamTopicAcquired != null)
                    UI.Dispatch(() => StreamTopicAcquired());
            });
        }
        private void GetGameList()
        {
            Task.Factory.StartNew(() =>
            {
                jsonGames = Json.DeserializeUrl<GamingLiveGameList>(@"http://api.gaminglive.tv/games");
                if (jsonGames == null)
                    return;
            });
        }
        public void SetTopic()
        {
            var userName = Config.GetParameterValue("Username") as string;
            var authToken = Config.GetParameterValue("AuthToken") as string;
            var gameId = this.With( x => jsonGames.games.FirstOrDefault( game => game.name.Equals(Info.CurrentGame.Name,StringComparison.InvariantCultureIgnoreCase) ))
                            .With( x => x.id);

            var jsonInfo = new GamingLiveChannelUpdate
            {
                slug = userName.ToLower(),
                owner = userName.ToLower(),
                name = Info.Topic,
                authToken = authToken,
                gameId = gameId,
            };



            Json.SerializeToStream<GamingLiveChannelUpdate>(jsonInfo, (stream) =>
            {
                var putUrl = @"https://api.gaminglive.tv/channels/";
                if (null == loginWebClient.PatchStream(putUrl, stream))
                {
                    //Authentication data expired ? Let's login again
                    LoginWithUsername();
                    jsonInfo.authToken = Config.GetParameterValue("AuthToken") as string;

                    Json.SerializeToStream<GamingLiveChannelUpdate>(jsonInfo, (streamRetry) =>
                    {
                        loginWebClient.PatchStream(putUrl, streamRetry);
                    });
                }
            });
        }

        public Action StreamTopicAcquired
        {
            get;
            set;

        }
    }

    public class GamingLiveChannel
    {
        private Timer pingTimer;
        private const int pingInterval = 60000;
        private WebSocketBase webSocket;
        private WebSocketBase secondWebSocket;
        private IChat _chat;
        bool isAnonymous = false;
        object lockConnect = new object();
        public GamingLiveChannel(IChat chat)
        {
            _chat = chat;
        }
        public void Join(Action<GamingLiveChannel> callback, string nickName, string channel, string authToken)
        {
            JoinCallback = callback;

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
            //webSocket.PingInterval = 0;
            webSocket.Origin = "http://www.gaminglive.tv";
            webSocket.Path = String.Format("/chat/{0}?nick={1}&authToken={2}", ChannelName.Replace("#",""), nickName, authToken );
            webSocket.ConnectHandler = () =>
            {
                //webSocket.Send("{}");
            };

            webSocket.DisconnectHandler = () =>
            {
                if (LeaveCallback != null)
                    LeaveCallback(this);
            };
            webSocket.ReceiveMessageHandler = ReadRawMessage;
            webSocket.Connect();

            pingTimer = new Timer((sender) =>
            {
                secondWebSocket = new WebSocketBase();
                secondWebSocket.Host = webSocket.Host;
                secondWebSocket.Origin = webSocket.Origin;
                secondWebSocket.Path = String.Format("/chat/{0}?nick={1}&authToken={2}", ChannelName.Replace("#", ""), "__$anonymous", "__$anonymous");
                secondWebSocket.Connect();
                Thread.Sleep(5000);
                secondWebSocket.Disconnect();

            }, this, 500, pingInterval);

        }
        private void ReadRawMessage(string rawMessage)        
        {
            if( !_chat.Status.IsConnected )
            {
                _chat.Status.IsStarting = false;
            }
            
            if (JoinCallback != null)
                JoinCallback(this);
            
            //Log.WriteInfo("gaminglive raw message: {0}", rawMessage);
            if( !String.IsNullOrWhiteSpace(rawMessage))
            {
                var json = JToken.Parse(rawMessage);
                var nickName = (string)json.Value<dynamic>("user").nick;
                var text = json.Value<string>("message");
                var memberType = json.Value<string>("mtype");
                if (memberType != null && memberType.Equals("BOT", StringComparison.InvariantCultureIgnoreCase))
                {
                    Log.WriteInfo("Gaminglive bot greeting from {0} on {1}", nickName, ChannelName);
                    return;
                }

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
            Log.WriteInfo("Gaminglive leaving {0}", ChannelName);
            webSocket.Disconnect();
        }

        public Action<GamingLiveChannel> LeaveCallback { get; set; }
        public Action<GamingLiveChannel> JoinCallback { get; set; }
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

    public class GamingLiveChannelUpdate
    {
        public string slug { get; set; }
        public string owner { get; set; }
        public string name { get; set; }
        public string gameId { get; set; }
        public string authToken { get; set; }
    }

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

    public class GamingLiveGameList
    {
        public List<GamingLiveGame> games { get; set; }
    }
    #endregion
}
