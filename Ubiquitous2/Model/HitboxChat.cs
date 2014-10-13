using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UB.Utils;

namespace UB.Model
{
    public class HitboxChat : IChat, IStreamTopic
    {
        public event EventHandler<ChatServiceEventArgs> MessageReceived;
        private WebClientBase loginWebClient = new WebClientBase();
        private const string emoticonUrl = "https://www.hitbox.tv/api/chat/icons/UnknownSoldier";
        private const string emoticonFallbackUrl = @"Content\hitboxemoticons.json";
        private List<HitboxChannel> hitboxChannels = new List<HitboxChannel>();
        private object counterLock = new object();
        private List<WebPoller> counterWebPollers = new List<WebPoller>();
        private bool isAnonymous = false;
        private object iconParseLock = new object();
        private bool isFallbackEmoticons = false;
        private bool isWebEmoticons = false;
        private object channelsLock = new object();
        private object pollerLock = new object();
        private object toolTipLock = new object();
        private object lockSearch = new object();

        public HitboxChat(ChatConfig config)
        {
            Config = config;
            Enabled = Config.Enabled;
            ContentParsers = new List<Action<ChatMessage, IChat>>();
            ChatChannels = new List<string>();
            Emoticons = new List<Emoticon>();
            Status = new StatusBase();
            Users = new Dictionary<string, ChatUser>();

            //ContentParsers.Add(MessageParser.ParseImageUrlsAsImages);
            ContentParsers.Add(MessageParser.RemoveRedundantTags);
            ContentParsers.Add(MessageParser.ParseURLs);
            ContentParsers.Add(MessageParser.ParseSpaceSeparatedEmoticons);

            Info = new StreamInfo()
            {
                HasDescription = false,
                HasGame = true,
                HasTopic = true,
                ChatName = Config.ChatName,
            };

            Games = new ObservableCollection<Game>();
        }
        #region IChat implementation
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
            
            Log.WriteInfo("Starting Hitbox.tv chat");
            Status.ResetToDefault();
            Status.IsStarting = true;

            if (Login())
            {
                Status.IsConnecting = true;
                Task.Factory.StartNew(() => JoinChannels());
            }
            InitEmoticons();

            return false;
        }

        public bool Stop()
        {
            if (!Enabled)
                Status.ResetToDefault();

            if (Status.IsStopping)
                return false;

            Log.WriteInfo("Stopping Hitbox.tv chat");

            Status.IsStopping = true;
            Status.IsStarting = false;

            lock( channelsLock )
            {
                hitboxChannels.ForEach(chan =>
                {
                    StopCounterPoller(chan.ChannelName);
                    chan.Leave();
                });
            }
            ChatChannels.Clear();
            return true;
        }

        public bool Restart()
        {
            if (Status.IsStopping || Status.IsStarting )
                return false;

            Status.ResetToDefault();
            Stop();
            Start();
            return true;
        }

        public bool SendMessage(ChatMessage message)
        {
            if (isAnonymous)
                return false;

            var hitBoxChannel = hitboxChannels.FirstOrDefault(channel => channel.ChannelName.Equals(message.Channel, StringComparison.InvariantCultureIgnoreCase));
            if (hitBoxChannel != null)
            {
                if( String.IsNullOrWhiteSpace (message.FromUserName))
                {
                    message.FromUserName = NickName;
                }
                Task.Factory.StartNew(() => hitBoxChannel.SendMessage(message));
            }
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
        #endregion


        private bool Login()
        {
            try
            {
                if (LoginWithToken())
                {
                    if (!LoginWithUsername())
                    {
                        Status.IsLoginFailed = true;
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteInfo("Hitbox authorization exception {0}", e.Message);
                return false;
            }
            if (!isAnonymous)
            {
                Status.IsLoggedIn = true;
                GetTopic();
            }

            return true;
        }

        public bool LoginWithToken()
        {
            var authToken = Config.GetParameterValue("AuthToken") as string;
            var userName = Config.GetParameterValue("Username") as string;
            var password = Config.GetParameterValue("Password") as string;
            var tokenCredentials = Config.GetParameterValue("AuthTokenCredentials") as string;

            if (tokenCredentials != userName + password)
                return false;

            if( String.IsNullOrEmpty(userName))
            {
                isAnonymous = true;
                return true;
            }

            if (String.IsNullOrWhiteSpace(authToken))
                return false;

            NickName = userName;

            var test = this.With(x => Json.DeserializeUrl<dynamic>(String.Format("https://www.hitbox.tv/api/teams/codex?authToken={0}",authToken)));
            
            if (test.teams != null)
            {
                return true;
            }

            
            Config.SetParameterValue("AuthToken", String.Empty);

            return false;
        }
        public bool LoginWithUsername()
        {
            var userName = Config.GetParameterValue("Username") as string;
            var password = Config.GetParameterValue("Password") as string;

            if (String.IsNullOrWhiteSpace(userName) || String.IsNullOrWhiteSpace(password))
            {
                isAnonymous = true;
                return true;
            }

            NickName = userName;

            var authString = String.Format(@"{{""login"":""{0}"",""pass"":""{1}"",""rememberme"":""""}}", userName, password);

            SetCommonHeaders();
            var authToken = this.With(x => loginWebClient.Upload("http://www.hitbox.tv/api/auth/login", authString))
                                .With(x => String.IsNullOrWhiteSpace(x) ? null : x)
                                .With(x => JToken.Parse(x))
                                .With(x => x.Value<string>("authToken"));

            if (authToken == null)
            {
                Log.WriteError("Login to hitbox.tv failed. Joining anonymously");
                isAnonymous = true;
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
        private void SetCommonHeaders()
        {
            loginWebClient.Headers["Accept"] = @"application/json, text/plain, */*";
            loginWebClient.Headers["Content-Type"] = @"application/json;charset=UTF-8";
            loginWebClient.Headers["Accept-Encoding"] = "gzip,deflate,sdch";
        }
        private void ReadMessage(ChatMessage message)
        {
            if (MessageReceived != null)
            {
                var original = message.Text;
                Log.WriteInfo("Original string:{0}", message.Text);
                if (ContentParsers != null)
                {
                    var number = 1;
                    ContentParsers.ForEach(parser =>
                    {

                        parser(message, this);
                        if( original != message.Text)
                            Log.WriteInfo("After parsing with {0}: {1}", number, message.Text);
                        number++;
                    });
                }

                MessageReceived(this, new ChatServiceEventArgs() { Message = message });

            }
        }
        public void WatchChannelStats(string channel)
        {
            var poller = new WebPoller()
            {
                Id = channel,
                Uri = new Uri(String.Format(@"http://api.hitbox.tv/media/live/{0}", channel.Replace("#", ""))),
            };

            UI.Dispatch(() => { 
                lock(toolTipLock)
                    Status.ToolTips.RemoveAll(t => t.Header == poller.Id);
            });
            UI.Dispatch(() => {
                lock (toolTipLock)
                    Status.ToolTips.Add(new ToolTip(poller.Id, ""));
            });

            poller.ReadStream = (stream) =>
            {
                lock (counterLock)
                {
                    var channelInfo = Json.DeserializeStream<HitboxChannelStats>(stream);
                    poller.LastValue = channelInfo;
                    var viewers = 0;
                    foreach (var webPoller in counterWebPollers.ToList())
                    {
                        var streamInfo = this.With(x => (HitboxChannelStats)webPoller.LastValue)
                            .With(x => x.livestream)
                            .With(x => x.FirstOrDefault(livestream => livestream.media_name.Equals(webPoller.Id.Replace("#",""), StringComparison.InvariantCultureIgnoreCase)));

                        lock(toolTipLock)
                        {
                            var tooltip = Status.ToolTips.FirstOrDefault(t => t.Header.Equals(webPoller.Id));
                            if (tooltip == null)
                                return;

                            if (streamInfo != null)
                            {
                                viewers += streamInfo.media_views;
                                tooltip.Text = streamInfo.media_views.ToString();
                                tooltip.Number = streamInfo.media_views;
                            }
                            else
                            {
                                tooltip.Text = "0";
                                tooltip.Number = 0;
                            }
                        }

                    }
                    UI.Dispatch(() => Status.ViewersCount = viewers);
                }
            };
            poller.Start();
            
            lock( pollerLock )
            {
                counterWebPollers.RemoveAll(p => p.Id == poller.Id);
                counterWebPollers.Add(poller);
            }
        }
        void StopCounterPoller(string channelName)
        {
            UI.Dispatch(() => {
                lock (toolTipLock)
                    Status.ToolTips.RemoveAll(t => t.Header == channelName);
            } );
            var poller = counterWebPollers.FirstOrDefault(p => p.Id == channelName);
            if (poller != null)
            {
                poller.Stop();
                counterWebPollers.Remove(poller);
            }
        }
        private void JoinChannels()
        {

            if (Status.IsStopping)
                return;

            var channels = Config.Parameters.StringArrayValue("Channels").Select(chan => "#" + chan.ToLower().Replace("#", "")).ToArray();

            if (NickName != null && !NickName.Equals("UnknownSoldier", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!channels.Contains("#" + NickName.ToLower()))
                    channels = channels.Union(new String[] { NickName.ToLower() }).ToArray();
            }

            foreach (var channel in channels)
            {
                if (channel.Equals("UnknownSoldier", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var hitboxChannel = new HitboxChannel(this);
                hitboxChannel.ReadMessage = ReadMessage;
                hitboxChannel.LeaveCallback = (hbChannel) =>
                {
                    StopCounterPoller(hbChannel.ChannelName);
                    lock(channelsLock)
                        hitboxChannels.RemoveAll(item => item.ChannelName == hbChannel.ChannelName);

                    ChatChannels.RemoveAll(chan => chan == null);
                    ChatChannels.RemoveAll(chan => chan.Equals(hbChannel.ChannelName, StringComparison.InvariantCultureIgnoreCase));
                    if (RemoveChannel != null)
                        RemoveChannel(hbChannel.ChannelName, this);

                    if (!Status.IsStarting && !Status.IsStopping)
                    {
                        Restart();
                        return;
                    }
                };
                if( !hitboxChannels.Any(c => c.ChannelName == channel))
                hitboxChannel.Join((hbChannel) =>
                {
                    if (Status.IsStopping)
                        return;

                    Status.IsConnected = true;
                    lock (channelsLock)
                        hitboxChannels.Add(hbChannel);


                    if (RemoveChannel != null)
                        RemoveChannel(hbChannel.ChannelName, this);

                    ChatChannels.RemoveAll(chan => chan.Equals(hbChannel.ChannelName, StringComparison.InvariantCultureIgnoreCase));
                    ChatChannels.Add((hbChannel.ChannelName));
                    if (AddChannel != null)
                        AddChannel(hbChannel.ChannelName, this);

                    WatchChannelStats(hbChannel.ChannelName);

                }, NickName, channel, (String)Config.GetParameterValue("AuthToken"));
            }
        }
        private void InitEmoticons()
        {
            //Fallback icon list
            DownloadEmoticons(AppDomain.CurrentDomain.BaseDirectory + emoticonFallbackUrl);
            //Web icons
            Task.Factory.StartNew(() => DownloadEmoticons(emoticonUrl));
        }
        public void DownloadEmoticons(string url)
        {
            if (isFallbackEmoticons && isWebEmoticons )
                return;

            lock (iconParseLock)
            {
                var list = new List<Emoticon>();
                if (Emoticons == null)
                    Emoticons = new List<Emoticon>();

                var jsonEmoticons = this.With(x => Json.DeserializeUrl<JObject>(url));

                if (jsonEmoticons == null)
                {
                    Log.WriteError("Unable to get Hitbox.tv emoticons!");
                    return;
                }
                else
                {
                    var icons = jsonEmoticons.Value<JObject>("icons");
                    if (icons == null)
                        return;
                    var dictionary = icons.Properties().Select(p => new { name = p.Name, value = p.Value.ToArray() }).ToDictionary(p => p.name, p => p.value.Select(x=>x.Value<string>()).ToArray() );

                    if( dictionary == null )
                        return;
                    var defaultWidth = 24;
                    var defaultHeight = 24;
                    foreach( KeyValuePair<string, string[]> pair in dictionary )
                    {
                        var smileUrl = "http://edge.sf.hitbox.tv" + pair.Value[0];
                        if( pair.Value.Length >= 3 )
                        {
                            foreach (var word in new string[] { pair.Key, pair.Value[1], pair.Value[2] })
                            {
                                if( !list.Any( emoticon => emoticon.ExactWord.Equals(word,StringComparison.CurrentCultureIgnoreCase)))
                                    list.Add(new Emoticon(null, smileUrl, defaultWidth, defaultHeight)
                                    {
                                        ExactWord = word,
                                    });
                            }
                        }
                    }

                    if (list.Count > 0)
                    {
                        Emoticons = list.ToList();
                        if (isFallbackEmoticons)
                            isWebEmoticons = true;

                        isFallbackEmoticons = true;
                    }
                }
            }
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

        #region IStreamTopic
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

        private dynamic[] GetHitboxGameList(string partName, int count = 100)
        {
            var url = "https://www.hitbox.tv/api/games?q={0}&limit={2}&_={1}";

            return this.With(x => loginWebClient.Download(String.Format(url, HttpUtility.UrlEncode(partName), Time.UnixTimestamp(),count)))
                .With(x => JToken.Parse(x))
                .With(x => x["categories"])
                .With(x => x.ToArray<dynamic>());

        }

        public void QueryGameList(string gameName, Action callback)
        {
            lock (lockSearch)
            {
                Log.WriteInfo("Searching hitbox game {0}", gameName);
                Games.Clear();
                Games.Add(new Game() { Name = "Loading..." });

                if (callback != null)
                    UI.Dispatch(() => callback());
                
                var jsonGames = GetHitboxGameList(gameName);

                if (jsonGames == null)
                {
                    Log.WriteInfo("Hitbox search returned empty result", gameName);
                    return;
                }

                Games.Clear();
                foreach (var obj in jsonGames)
                {
                    Games.Add(new Game()
                    {
                        Id = obj.category_id,
                        Name = obj.category_name,
                    });
                }

                if (callback != null)
                    UI.Dispatch(() => callback());
            }

        }

        public void GetTopic()
        {
            if( !Status.IsLoggedIn || !Enabled)
                return;

            Task.Factory.StartNew(() =>
            {
                var jsonInfo = GetLiveStreamInfo();

                if (jsonInfo == null)
                    return;

                var livestream = this.With(x => jsonInfo["livestream"].ToArray<JToken>())
                    .With(x => x.Count() <= 0 ? null : x[0].ToObject<JToken>());
                    

                if (livestream == null)
                    return;

                Info.Topic = livestream["media_status"].ToObject<string>();
                Info.CurrentGame.Name = livestream["category_name"].ToObject<string>();
                Info.CurrentGame.Id = livestream["category_id"].ToObject<string>();

                Info.CanBeRead = true;
                Info.CanBeChanged = true;

                if (StreamTopicAcquired != null)
                    UI.Dispatch(() => StreamTopicAcquired());

            });
        }

        JToken GetLiveStreamInfo()
        {
            var getUrl = @"https://www.hitbox.tv/api/media/live/{0}/list?authToken={1}&filter=recent&hiddenOnly=false&limit=10&nocache=true&publicOnly=false&search=&showHidden=true&yt=false&_" + Time.UnixTimestamp().ToString(); ;
            var userName = Config.GetParameterValue("Username") as string;
            var authToken = Config.GetParameterValue("AuthToken") as string;

            return this.With(x => loginWebClient.Download(String.Format(getUrl, HttpUtility.UrlEncode(userName.ToLower()), authToken)))
                .With(x => !String.IsNullOrWhiteSpace(x)?JToken.Parse(x):null);
            
        }

        public void SetTopic()
        {
            if (!Status.IsLoggedIn)
                return;

            LoginWithUsername();

            var currentInfo = GetLiveStreamInfo();
            var livestream = this.With( x => currentInfo )
                            .With(x => x["livestream"])
                            .With(x => x[0]);

            if (livestream == null)
                return;

            currentInfo["livestream"][0]["media_status"] = Info.Topic;
            
            if( !String.IsNullOrWhiteSpace( Info.CurrentGame.Name) )
            {
                var gameList = GetHitboxGameList( Info.CurrentGame.Name, 1 );                    
                if( gameList.Count() > 0 )
                {
                    var game = gameList[0];
                    currentInfo["livestream"][0]["category_name"] = Info.CurrentGame.Name;
                    currentInfo["livestream"][0]["category_id"] = (string)game.category_id;
                    currentInfo["livestream"][0]["category_name_short"] = (string)game.category_name_short;
                    currentInfo["livestream"][0]["category_seo_key"] = (string)game.category_seo_key;
                    currentInfo["livestream"][0]["category_viewers"] = (string)game.category_viewers;
                    currentInfo["livestream"][0]["category_logo_small"] = (string)game.category_logo_small;
                    currentInfo["livestream"][0]["category_logo_large"] = (string)game.category_logo_large;
                    currentInfo["livestream"][0]["category_updated"] = (string)game.category_updated;
                    currentInfo["livestream"][0]["media_category_id"] = (string)game.category_id;
                }
            }
            
            
            Json.SerializeToStream<JToken>(currentInfo, (stream) => {
                var putUrl = @"https://www.hitbox.tv/api/media/live/{0}/list?authToken={1}&filter=recent&hiddenOnly=false&limit=10&nocache=true&publicOnly=false&search=&showHidden=true&yt=false";
                var userName = Config.GetParameterValue("Username") as string;
                var authToken = Config.GetParameterValue("AuthToken") as string;

                if (null == loginWebClient.PutStream(String.Format(putUrl, HttpUtility.UrlEncode(userName.ToLower()), authToken), stream))
                {
                    //Authentication data expired ? Let's login again
                    LoginWithUsername();

                    userName = Config.GetParameterValue("Username") as string;
                    authToken = Config.GetParameterValue("AuthToken") as string;
                    stream.Position = 0;
                    loginWebClient.PutStream(String.Format(putUrl, HttpUtility.UrlEncode(userName.ToLower()), authToken), stream);
                }
            });
        }

        public Action StreamTopicAcquired
        {
            get;
            set;
        }
        #endregion
    }

    public class HitboxChannel
    {
        private WebSocketBase webSocket;
        private HitboxChat _chat;
        private Random random = new Random();
        private bool isAnonymous = false;

        public HitboxChannel(HitboxChat chat)
        {
            _chat = chat;
            HitboxChannelStatus = new StatusBase();
        }
        public StatusBase HitboxChannelStatus { get; set; }
        public string NickName { get; set; }
        public string AuthToken { get; set; }
        private void GetRandomIP(string[] hosts, int port, Action<string> callback)
        {
            List<string> resultList = new List<string>();
            object arrayLock = new object();
            
            if (hosts == null || hosts.Count() <= 0)
                return;

            Task[] hostTestTasks = new Task[hosts.Count()];

            for (int i = 0; i < hostTestTasks.Length; i++)
            {
                var index = i;
                hostTestTasks[i] = Task.Factory.StartNew(() =>
                    {
                        var host = hosts[index];
                        Utils.Net.TestTCPPort(host, port, (list, error) =>
                        {
                            if( list != null && list.AddressList.Count() > 0 )
                            {
                                foreach(IPAddress ip in list.AddressList )
                                {
                                    lock( arrayLock )
                                    {
                                        resultList.Add(ip.ToString());
                                    }
                                }
                            }
                        });

                    });
                Thread.Sleep(1);
            }
            Task.WaitAll(hostTestTasks, 3000);

            if (resultList.Count() <= 0)
            {
                if( hosts.Count() > 0 )
                {
                    resultList.Add(hosts[0]);
                }
                else
                {
                    Log.WriteInfo("All hitbox servers are down!");
                    callback(null);
                    return;
                }
            }

            var randomHost = resultList[random.Next(0, resultList.Count())];
            if (randomHost == null)
            {
                callback(null);
                return;
            }

            callback(randomHost);

        }
        public void Join(Action<HitboxChannel> callback, string nickName, string channel, string authToken)
        {

            if( String.IsNullOrWhiteSpace(channel) )
                return;
            NickName = nickName;
            AuthToken = authToken;
            ChannelName = "#" + channel.Replace("#", "");
            webSocket = new WebSocketBase();
            webSocket.PingInterval = 0;
            webSocket.Origin = "http://www.hitbox.tv";
            webSocket.ConnectHandler = () =>
            {
                SendCredentials(NickName, channel, authToken);

                if (callback != null)
                    callback(this);
            };
            
            webSocket.DisconnectHandler = () =>
            {
                Log.WriteError("Hitbox disconnected {0}", ChannelName);
                if (LeaveCallback != null)
                    LeaveCallback(this);
            };
            webSocket.ReceiveMessageHandler = ReadRawMessage;
            Connect();
        }

        private void SendCredentials(string nickname, string channel, string authToken)
        {
            isAnonymous = String.IsNullOrWhiteSpace(authToken) || String.IsNullOrWhiteSpace(nickname);

            var loginData = new HitboxWebSocketPacket()
            {
                name = "message",
                args = new List<HitboxArg>()
                {
                    new HitboxArg() {
                        method = "joinChannel",
                        @params = new {
                            channel = channel.Replace("#",""),
                            name =  isAnonymous ? "UnknownSoldier" : nickname ?? "",
                            token = isAnonymous ? "" : authToken,
                            isAdmin = !isAnonymous && channel.Replace("#","").Equals(nickname,StringComparison.InvariantCultureIgnoreCase) ? true :false,
                        }   
                    }
                }
            };
            var jsonString = JsonConvert.SerializeObject(loginData);
            webSocket.Send("5:::" + jsonString);
        }
        private void Connect()
        {
            HitboxChannelStatus.ResetToDefault();
            var servers = Json.DeserializeUrl<HitboxServer[]>("https://www.hitbox.tv/api/chat/servers?redis=true");
            if (servers == null)
                return;

            int port = 0;            
            if( int.TryParse(webSocket.Port, out port) )
            {
                GetRandomIP(servers.Select(s => s.server_ip).ToArray(), int.Parse(webSocket.Port), (ip) =>
                {
                    if( !String.IsNullOrWhiteSpace(ip) )
                    {
                        using (var webClient = new WebClientBase())
                        {
                            string pathHash = this.With(x => webClient.Download(String.Format("http://{0}/socket.io/1/{1}", ip, Time.UnixTimestamp())))
                                        .With(x => new String(x.TakeWhile(c => !c.Equals(':')).ToArray()));

                            webSocket.Path = String.Format("/socket.io/1/websocket/{0}", pathHash);
                            webSocket.Host = ip;
                            webSocket.Connect();
                        }

                    }
                });
            }            
        }
        private void ReadRawMessage(string rawMessage)
        {
            const string jsonArgsRe = @".*args"":\[""(.*?)""\]}$";

            if (rawMessage.Equals("1::"))
            {
                HitboxChannelStatus.IsConnected = true;
            }
            else if (rawMessage.Equals("2::"))
            {
                Thread.Sleep(random.Next(100, 1000));
                webSocket.Send("2::");
                return;
            }
            
            if( rawMessage.Contains( @":""message"))
            {
                var json = Re.GetSubString(rawMessage, jsonArgsRe);
                if (json == null)
                    return;

                dynamic msg = this.With(x => JToken.Parse(json.Replace(@"\""", @"""").Replace(@"\\", @"\")))
                    .With(x => x.Value<dynamic>("params"));

                if (msg == null)
                    return;

                if (rawMessage.Contains(@":\""loginMsg"))
                {
                    var role = (string)msg.role;
                    switch( role.ToLower() )
                    {
                        case "guest":
                            if (!isAnonymous)
                            {
                                _chat.Status.IsLoggedIn = false;
                                if( !_chat.Status.IsLoginFailed )
                                {
                                    _chat.Status.IsConnected = false;
                                    _chat.Status.IsLoggedIn = false;
                                    _chat.Status.IsLoginFailed = true;
                                    _chat.Status.IsStarting = false;
                                    _chat.Config.SetParameterValue("AuthToken", String.Empty);
                                    _chat.Restart();
                                }
                            }
                            else
                            {
                                _chat.Status.IsLoggedIn = true;
                                _chat.Status.IsLoginFailed = false;
                            }

                            break;
                        case "admin":
                            {
                                _chat.Status.IsLoggedIn = true;
                                _chat.Status.IsLoginFailed = false;
                            }
                            break;
                        case "anon":
                            {
                                _chat.Status.IsLoggedIn = true;
                                _chat.Status.IsLoginFailed = false;
                            }
                            break;
                        default:
                           break;
                    }
                    SendCredentials(NickName, ChannelName, AuthToken);
                }
                else if (!String.IsNullOrWhiteSpace(rawMessage) && rawMessage.Contains("chatMsg"))
                {
                    var nickName = (string)msg.name;
                    var text = (string)msg.text;

                    if (String.IsNullOrWhiteSpace(nickName) || String.IsNullOrWhiteSpace(text))
                        return;

                    if (ReadMessage != null)
                        ReadMessage(new ChatMessage()
                        {
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
            
        }
        public string ChannelName { get; set; }
        public void Leave()
        {
            Log.WriteInfo("Hitbox leaving {0}", ChannelName);
            webSocket.Disconnect();
        }
        
        public Action<HitboxChannel> LeaveCallback { get; set; }
        public Action<ChatMessage> ReadMessage { get; set; }
        public void SendMessage( ChatMessage message )
        {
            if (isAnonymous || String.IsNullOrWhiteSpace(message.Channel) ||
                String.IsNullOrWhiteSpace(message.FromUserName) ||
                String.IsNullOrWhiteSpace(message.Text))
                return;

            HitboxWebSocketPacket jsonMessage = new HitboxWebSocketPacket() { 
                name = "message",
                args = new List<HitboxArg>()
                {
                    new HitboxArg() {
                            method = "chatMsg",
                            @params = new {
                                channel = message.Channel.Replace("#",""),
                                name =  message.FromUserName,
                                nameColor = "B404AE",
                                text = message.Text,
                        }
                    }
                }
            };
            webSocket.Send("5:::" + JsonConvert.SerializeObject(jsonMessage));
        }
    }

    #region Hitbox json 
    public class HitboxRequest
    {
        public string @this { get; set; }
    }

    public class HitboxChannelData
    {
        public string followers { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string user_status { get; set; }
        public string user_logo { get; set; }
        public string user_cover { get; set; }
        public string user_logo_small { get; set; }
        public string user_partner { get; set; }
        public string livestream_count { get; set; }
        public string channel_link { get; set; }
    }

    public class HitboxLivestream
    {
        public string media_user_name { get; set; }
        public string media_id { get; set; }
        public string media_file { get; set; }
        public string media_user_id { get; set; }
        public string media_profiles { get; set; }
        public string media_type_id { get; set; }
        public string media_is_live { get; set; }
        public string media_live_delay { get; set; }
        public object media_featured { get; set; }
        public string media_date_added { get; set; }
        public string media_live_since { get; set; }
        public string media_transcoding { get; set; }
        public string media_chat_enabled { get; set; }
        public string media_name { get; set; }
        public string media_display_name { get; set; }
        public string media_status { get; set; }
        public string media_title { get; set; }
        public string media_description { get; set; }
        public string media_description_md { get; set; }
        public string media_tags { get; set; }
        public string media_duration { get; set; }
        public string media_bg_image { get; set; }
        public int media_views { get; set; }
        public string media_views_daily { get; set; }
        public string media_views_weekly { get; set; }
        public string media_views_monthly { get; set; }
        public string category_id { get; set; }
        public string category_name { get; set; }
        public object category_name_short { get; set; }
        public string category_seo_key { get; set; }
        public string category_viewers { get; set; }
        public object category_logo_small { get; set; }
        public string category_logo_large { get; set; }
        public string category_updated { get; set; }
        public object team_name { get; set; }
        public string media_start_in_sec { get; set; }
        public string media_duration_format { get; set; }
        public string media_time_ago { get; set; }
        public string media_thumbnail { get; set; }
        public string media_thumbnail_large { get; set; }
        public HitboxChannelData channel { get; set; }
    }

    public class HitboxChannelStats
    {
        public HitboxRequest request { get; set; }
        [JsonProperty( Required = Required.Default)]
        public string authToken { get; set; }
        public string media_type { get; set; }
        public List<HitboxLivestream> livestream { get; set; }
    }

    public class HitboxServer
    {
        public string server_ip { get; set; }
    }

    public class HitboxArg
    {
        public string method { get; set; }
        public dynamic @params { get; set; }
    }

    public class HitboxWebSocketPacket
    {
        public string name { get; set; }
        public List<HitboxArg> args { get; set; }
    }
    #endregion
}
