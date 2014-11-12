using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UB.Utils;

namespace UB.Model
{
    public class GoodgameChat : ChatBase, IStreamTopic
    {
        //API: https://docs.google.com/document/d/1vVtEhiqHhyW7wYor3zLh6PNu0_v_3LTboTQBPaRR1SY/edit?pli=1    
        private const string emoticonImageUrl = @"http://goodgame.ru/images/generated/smiles.png";
        private string ownChannel;
        private string ownChannelId;
        private object iconParseLock = new object();
        private WebClientBase webClient = new WebClientBase();

        #region IChat
        public GoodgameChat(ChatConfig config)
            : base(config)
        {
            EmoticonUrl = @"http://goodgame.ru/css/compiled/chat.css";
            EmoticonFallbackUrl = @"Content\goodgame_smiles.css";

            CreateChannel = () => { return new GoodgameChannel(this); };

            ContentParsers.Add(MessageParser.ConvertToPlainText);
            ContentParsers.Add(MessageParser.ParseURLs);
            ContentParsers.Add(MessageParser.ParseEmoticons);

            Info = new StreamInfo()
            {
                HasDescription = false,
                HasGame = true,
                HasTopic = true,
                ChatName = Config.ChatName,
            };

            Games = new ObservableCollection<Game>();
        }

        public override bool Login()
        {
            try
            {
                if (!LoginWithToken())
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
                Log.WriteInfo("Goodgame authorization exception {0}", e.Message);
                return false;
            }
            if (!IsAnonymous)
            {
                Status.IsLoggedIn = true;
            }

            return true;
        }

        private bool LoginWithToken()
        {
            var authToken = Config.GetParameterValue("AuthToken") as string;
            var userName = Config.GetParameterValue("Username") as string;
            var password = Config.GetParameterValue("Password") as string;
            var tokenCredentials = Config.GetParameterValue("AuthTokenCredentials") as string;

            if (tokenCredentials != userName + password)
                return false;

            if (String.IsNullOrEmpty(userName))
            {
                IsAnonymous = true;
                ResetAuthData();
                return true;
            }

            if (String.IsNullOrWhiteSpace(authToken))
                return false;

            NickName = userName;

            webClient.SetCookie("PHPSESSID", authToken, "goodgame.ru");

            var content = GoodgameGet("http://goodgame.ru/chat/");

            if (String.IsNullOrWhiteSpace(content))
                return false;

            uint userId = 0;

            if (!UInt32.TryParse(Re.GetSubString(content, @"userId.*?'(\d+)"), out userId))
            {
                IsAnonymous = true;
                ResetAuthData();
                return false;
            }
            else
            {
                if (userId == 0)
                    LoginWithUsername();

                IsAnonymous = false;
                Info.CanBeRead = true;
                Info.CanBeChanged = true;
                Config.SetParameterValue("AuthToken", authToken);
                Config.SetParameterValue("AuthTokenCredentials", userName + password);
                Config.SetParameterValue("ChatToken", Re.GetSubString(content, @"token.*?'(.*?)'"));
                Config.SetParameterValue("UserId", userId.ToString());
            }

            return true;
        }

        private bool LoginWithUsername()
        {
            var userName = Config.GetParameterValue("Username") as string;
            var password = Config.GetParameterValue("Password") as string;

            if (String.IsNullOrWhiteSpace(userName) || String.IsNullOrWhiteSpace(password))
            {
                IsAnonymous = true;
                ResetAuthData();
                return true;
            }

            NickName = userName;

            var authString = String.Format(@"nickname={0}&password={1}&remember=1", HttpUtility.UrlEncode(userName), HttpUtility.UrlEncode(password));

            webClient.ContentType = ContentType.UrlEncoded;
            webClient.Headers["X-Requested-With"] = "XMLHttpRequest";

            webClient.Upload(@"http://goodgame.ru/ajax/login/", authString);
            var authToken = webClient.CookieValue("PHPSESSID", "http://goodgame.ru");
            var uid = webClient.CookieValue("uid", "http://goodgame.ru");

            if (String.IsNullOrWhiteSpace(authToken))
            {
                Log.WriteError("Login to goodgame.ru failed. Joining anonymously");
                IsAnonymous = true;
                ResetAuthData();
                return false;
            }
            else
            {
                Config.SetParameterValue("AuthToken", authToken);
                Config.SetParameterValue("AuthTokenCredentials", userName + password);
                Config.SetParameterValue("UserId", uid);
                if (!String.IsNullOrEmpty(uid) && uid != "0")
                    return LoginWithToken();
                else
                    return false;
            }
        }

        public override void JoinChannels()
        {
            var serverUri = GetServerUri();
            if (serverUri != null)
                Config.SetParameterValue("ServerUri", serverUri.OriginalString);

            base.JoinChannels();
        }

        public override void DownloadEmoticons(string url)
        {
                lock (iconParseLock)
                {
                    if (IsFallbackEmoticons && IsWebEmoticons)
                        return;

                    var list = new List<Emoticon>();
                    if (Emoticons == null)
                        Emoticons = new List<Emoticon>();


                    var content = GoodgameGet(url);

                    MatchCollection matches = Regex.Matches(content, @"}[^\.]*\.smile-([^-|\s]*)\s*{(.*?)}", RegexOptions.IgnoreCase);

                    if (matches.Count <= 0)
                    {
                        Log.WriteError("Unable to get Goodgame.ru emoticons!");
                    }
                    else
                    {
                        string originalUrl = null;
                        foreach (Match match in matches)
                        {
                            if (match.Groups.Count >= 2)
                            {
                                var smileName = match.Groups[1].Value;
                                var cssClassDefinition = match.Groups[2].Value;

                                var background = Css.GetBackground(cssClassDefinition);

                                if (background != null && !String.IsNullOrWhiteSpace(background.url) && background.width > 0 && background.height > 0)
                                {
                                    originalUrl = String.Format("http://goodgame.ru/{0}", background.url.Replace("../../", ""));
                                    var modifiedUrl = String.Format(@"/ubiquitous/cache?ubx={0}&uby={1}&ubw={2}&ubh={3}&uburl={4}",
                                        background.x, background.y, background.width, background.height, HttpUtility.UrlEncode(originalUrl));

                                    list.Add(new Emoticon(String.Format(":{0}:", smileName),
                                        modifiedUrl,
                                        background.width,
                                        background.height
                                    ));
                                }
                            }
                        }
                        if (list.Count > 0)
                        {
                            Uri uri;
                            if (!String.IsNullOrWhiteSpace(originalUrl) && Uri.TryCreate(originalUrl, UriKind.Absolute, out uri))
                            {
                                var ddosCookieGet = GoodgameGet("http://goodgame.ru");
                                if (ddosCookieGet != null)
                                {
                                    var imageDataService = SimpleIoc.Default.GetInstance<IImageDataSource>();
                                    using (var memoryStream = webClient.DownloadToMemoryStream(originalUrl))
                                    {
                                        imageDataService.AddImage(uri, memoryStream);
                                    }
                                }
                                else
                                {
                                    //get local image
                                }
                            }
                            Emoticons = list;
                            if (IsFallbackEmoticons)
                                IsWebEmoticons = true;

                            IsFallbackEmoticons = true;
                        }
                    }
                }
        }        
        #endregion

        public UInt32 GetChannelId(string channelName)
        {
            UInt32 channelId = 0;
            var content = GoodgameGet(String.Format(@"http://goodgame.ru/api/getupcomingbroadcast?id={0}&fmt=json", channelName.Replace("#", "")));
            var textChannelId = Re.GetSubString(content, @"stream_id.*?(\d+)");
            if (!String.IsNullOrWhiteSpace(textChannelId))
            {
                UInt32.TryParse(textChannelId, out channelId);
            }
            return channelId;
        }

        private Uri GetServerUri()
        {
            var re = @"this\.server=.*?""(.*?)""";
            var serverUrl = this.With(x => GoodgameGet("http://goodgame.ru/js/minified/chat.js"))
                .With(x => Re.GetSubString(x, re));

            Uri uri;

            if( Uri.TryCreate( serverUrl, UriKind.Absolute, out uri))
                return uri;
            else
                return null;

        }

        private string GoodgameGet(string url)
        {
            var content = webClient.Download(url);
            if (content != null && content.Length < 1000 && content.Contains("location.href="))
            {
                var cookieName = Re.GetSubString(content, @"\.cookie=\""(.*?)=");
                var cookieValue = Re.GetSubString(content, @"\.cookie=\"".*?=(.*?)""");
                var newHref = Re.GetSubString(content, @"location\.href=""(.*?)""");
                if (!String.IsNullOrWhiteSpace(cookieName) && !String.IsNullOrWhiteSpace(cookieValue) && !String.IsNullOrWhiteSpace(newHref))
                {
                    webClient.Encoding = System.Text.Encoding.UTF8;
                    webClient.SetCookie(cookieName, cookieValue, "goodgame.ru");
                    content = webClient.Download(newHref);
                }
            }
            return content;
        }

        private void ResetAuthData()
        {
            Config.SetParameterValue("AuthToken", String.Empty);
            Config.SetParameterValue("AuthTokenCredentials", String.Empty);
            Config.SetParameterValue("ChatToken", String.Empty);
            Config.SetParameterValue("UserId", 0);
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

        public void QueryGameList(string gameName, Action callback)
        {
            Games.Clear();

            webClient.ContentType = ContentType.UrlEncodedUTF8;
            webClient.Headers["X-Requested-With"] = "XMLHttpRequest";

            this.With(x => GoodgameGet(String.Format("http://goodgame.ru/ajax/games/?q={0}&limit=10", HttpUtility.UrlEncode(gameName))))
                .With(x => String.IsNullOrWhiteSpace(x) ? null : x)
                .With(x => JArray.Parse(x))
                .With(x => x.Select(game => new Game() { Id = game[2].ToString(), Name = game[0].ToString() }))
                .ToList()
                .ForEach(g => Games.Add(g));

            if (callback != null)
                UI.Dispatch(() => callback());
        }

        public void GetTopic()
        {
            if (!Status.IsLoggedIn || !Enabled)
                return;

            Task.Factory.StartNew(() =>
            {
                var content = GoodgameGet("http://goodgame.ru/");
                ownChannel = Re.GetSubString(content, @"\/channel\/([^\/]+)\/add");

                content = GoodgameGet(String.Format("http://goodgame.ru/chat/{0}/", ownChannel));
                ownChannelId = Re.GetSubString(content, @"channelId:[^\d]*(\d+)");

                if (!String.IsNullOrWhiteSpace(ownChannel))
                {

                    content = GoodgameGet(String.Format(@"http://goodgame.ru/channel/{0}", ownChannel));
                    if (!String.IsNullOrWhiteSpace(content))
                    {
                        Info.Topic = Re.GetSubString(content, @"<title>([^<]*)</title>");
                        Info.CurrentGame.Name = this.With(x => Re.GetSubString(content, @"StreamTitleEdit[^,]*,[^,]*,[^,]*,[^,]*,[^']*'([^']*)'"))
                            .With(x => x.Trim());
                        Info.CurrentGame.Id = this.With(x => Re.GetSubString(content, @"StreamTitleEdit[^,]*,[^,]*,[^,]*,([^,]*),[^']*'[^']*'"))
                            .With(x => x.Trim());
                    }
                }
            });
        }

        public void SetTopic()
        {
            if (string.IsNullOrWhiteSpace(ownChannelId) || !Status.IsLoggedIn)
                return;

            var searchGame = Games.FirstOrDefault(game => game.Name.Equals(Info.CurrentGame.Name, StringComparison.InvariantCultureIgnoreCase));
            if (searchGame == null)
            {
                QueryGameList(Info.CurrentGame.Name, null);
                searchGame = Games.FirstOrDefault(game => game.Name.Equals(Info.CurrentGame.Name, StringComparison.InvariantCultureIgnoreCase));
            }

            if (searchGame != null)
                Info.CurrentGame.Id = searchGame.Id;

            var parameters = String.Format("objType=7&objId={0}&title={1}&gameId={2}", ownChannelId, HttpUtility.UrlEncode(Info.Topic), Info.CurrentGame.Id);
            webClient.ContentType = ContentType.UrlEncoded;
            webClient.Headers["X-Requested-With"] = "XMLHttpRequest";
            webClient.Upload("http://goodgame.ru/ajax/channel/update_title/", parameters);
        }

        public Action StreamTopicAcquired
        {
            get;
            set;

        }
        
        #endregion
    }


    public class GoodgameChannel : ChatChannelBase
    {
        private WebSocketBase webSocket;
        private Random random = new Random();
        private WebClientBase webClient = new WebClientBase();
        private Timer timer;
        private Timer disconnectTimer;
        private const int counterInterval = 20000;
        private const int disconnectTimeout = 40000;
        private Dictionary<string, Action<GoodgameChannel, GoodGameData>>
            packetHandlers = new Dictionary<string, Action<GoodgameChannel, GoodGameData>>() {
                {"welcome", WelcomeHandler},
                {"success_auth", SuccessAuthHandler},
                {"success_join", SuccessJoinHandler},
                {"channel_counters", ChannelCountersHandler},
                {"message", MessageHandler},
                {"viewers", ViewersHandler},
                {"pong", PongHandler},
            };

        public GoodgameChannel(GoodgameChat chat)
        {
            Chat = chat;
            timer = new Timer((obj) => {
                RequestCounters();
            }, this, Timeout.Infinite, Timeout.Infinite);

            disconnectTimer = new Timer((obj) =>
            {
                (obj as GoodgameChannel).webSocket.Disconnect();
            }, this, Timeout.Infinite, Timeout.Infinite);
        }
        private static void ViewersHandler(GoodgameChannel channel, GoodGameData data)
        {
            if (data.Count == null)
                channel.ChannelStats.ViewersCount = 0;
            else
                channel.ChannelStats.ViewersCount = (int)data.Count;

            channel.Chat.UpdateStats();
        }
        private static void PongHandler(GoodgameChannel channel, GoodGameData data)
        {
            Log.WriteInfo("Goodgame pong received");
        }
        private static void WelcomeHandler(GoodgameChannel channel, GoodGameData data)
        {
            Log.WriteInfo("Goodgame protocol version: {0}", data.ProtocolVersion);
            Log.WriteInfo("Goodgame servicer identity: {0}", data.ServerIdentity);

            channel.SendCredentials();
        }
        private static void SuccessAuthHandler(GoodgameChannel channel, GoodGameData data)
        {
            channel.Chat.Status.IsConnected = true;
            if (data.UserId == 0)
            {
                channel.Chat.Status.IsLoggedIn = false;
                channel.Chat.IsAnonymous = true;
            }
            else
            {
                channel.Chat.Status.IsLoggedIn = true;
                channel.Chat.IsAnonymous = false;
            }

            channel.SendChannelJoin();
        }
        private void SendChannelJoin()
        {
            var channelId = (Chat as GoodgameChat).GetChannelId(ChannelName);
            Log.WriteInfo("Goodgame serializing join packet. ChannelId: {0}", channelId);
            var joinPacket = new GoodgamePacket()
            {
                Type = "join",
                Data = new GoodGameData()
                {
                    ChannelId = channelId,
                    IsHidden = false,
                    Mobile = 0,
                },
            };
            if( joinPacket != null && joinPacket.Data != null )
            {
                Log.WriteInfo("Goodgame sending {0}", joinPacket.ToString());
                webSocket.Send(joinPacket.ToString());
            }
        }
        private static void SuccessJoinHandler(GoodgameChannel channel, GoodGameData data)
        {
            UI.Dispatch(() => {
                channel.Chat.Status.IsConnecting = false;
                channel.Chat.Status.IsStarting = false;
                channel.Chat.Status.IsConnected = true;

                if (!channel.Chat.IsAnonymous)
                    channel.Chat.Status.IsLoggedIn = true;            
            });

            channel.timer.Change(0, counterInterval);
            channel.disconnectTimer.Change(disconnectTimeout, Timeout.Infinite);

            if (channel.JoinCallback != null)
                channel.JoinCallback(channel);           

            Log.WriteInfo("Goodgame joined to #{0} id:{1}", data.ChannelName, data.ChannelId);
        }
        private static void ChannelCountersHandler(GoodgameChannel channel, GoodGameData data)
        {
            Log.WriteInfo("Goodgame counters. Clients: {0}, Users:{1}", data.ClientsInChannel, data.UsersInChannel);
        }
        private static void MessageHandler(GoodgameChannel channel, GoodGameData data)
        {
            if (String.IsNullOrWhiteSpace(data.UserName) || String.IsNullOrWhiteSpace(data.Text))
                return;

            channel.ChannelStats.MessagesCount++;
            channel.Chat.UpdateStats();

            if (channel.ReadMessage != null)
                channel.ReadMessage(new ChatMessage()
                {
                    Channel = channel.ChannelName,
                    ChatIconURL = channel.Chat.IconURL,
                    ChatName = channel.Chat.ChatName,
                    FromUserName = data.UserName,
                    HighlyImportant = false,
                    IsSentByMe = false,
                    Text = HttpUtility.HtmlDecode(data.Text),
                });
        }

        public override void Join(Action<IChatChannel> callback, string channel)
        {
            Thread.Sleep(random.Next(200,500));
            if (String.IsNullOrWhiteSpace(channel))
                return;

            ChannelName = "#" + channel.Replace("#", "");

            webSocket = new WebSocketBase();
            webSocket.PingInterval = 0;
            webSocket.Origin = "http://goodgame.ru";
            JoinCallback = callback;

            webSocket.DisconnectHandler = () =>
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                if (LeaveCallback != null)
                    LeaveCallback(this);
            };
            webSocket.ReceiveMessageHandler = ReadRawMessage;
            Connect();
        }
        private void RequestCounters()
        {
            var channelId = (Chat as GoodgameChat).GetChannelId(ChannelName);
            var counterPacket = new GoodgamePacket()
            {
                Type = "getviewers",
                Data = new GoodGameData()
                {
                    Channel = channelId
                },
            };
            if (counterPacket != null && counterPacket.Data != null)
            {
                Log.WriteInfo("Goodgame sending {0}", counterPacket.ToString());
                webSocket.Send(counterPacket.ToString());
            }
        }
        private void SendCredentials()
        {
            uint userId = 0;
            UInt32.TryParse( Chat.Config.GetParameterValue("UserId").ToString(), out userId);
            var authPacket = new GoodgamePacket()
            {
                Type = "auth",
                Data = new GoodGameData()
                {
                    UserId = userId,
                    Token = this.With( x => Chat.Config.GetParameterValue("ChatToken")).With( x=> x.ToString()),
                },
            };

            Log.WriteInfo("Goodgame sending {0}", authPacket.ToString());
            webSocket.Send(authPacket.ToString());
        }
        private void SendPing()
        {
            var pingPacket = new GoodgamePacket()
            {
                Type = "ping",
                Data = new GoodGameData()
                {

                }
            };
            webSocket.Send(pingPacket.ToString());
        }
        private void Connect()
        {
            Uri serverUri;
            if( !Uri.TryCreate( this.With ( x => Chat.Config.GetParameterValue("ServerUri")).With(x => x.ToString()), UriKind.Absolute, out serverUri ) )
            {
                webSocket.Path = String.Format("/chat/{0}/{1}/websocket", Rnd.RandomWebSocketServerNum(0x1e3), Rnd.RandomWebSocketString());
                webSocket.Port = "8081";
                webSocket.Host = "chat.goodgame.ru";
            }
            else
            {
                webSocket.Path = String.Format("{0}/{1}/{2}/websocket", serverUri.AbsolutePath, Rnd.RandomWebSocketServerNum(0x1e3), Rnd.RandomWebSocketString());
                webSocket.Port = serverUri.Port.ToString();
                webSocket.Host = serverUri.Host;
            }
            webSocket.Connect();
        }
        private void ReadRawMessage(string rawMessage)
        {
            Log.WriteInfo("Goodgame raw message {0}", rawMessage);
            disconnectTimer.Change(disconnectTimeout, Timeout.Infinite);
            if( rawMessage.StartsWith("a"))
            {
                var packet = this.With( x => JArray.Parse(rawMessage.Substring(1)))
                    .With( x => x[0])
                    .With( x => JsonConvert.DeserializeObject<GoodgamePacket>( x.Value<string>() ));

                if (packet == null)
                    return;

                if ( packet.Data != null && packetHandlers.ContainsKey(packet.Type))
                    packetHandlers[packet.Type](this, packet.Data);
            }
            else if( rawMessage.StartsWith("o"))
            {
            }
        }
        public override void Leave()
        {
            Log.WriteInfo("Goodgame.ru leaving {0}", ChannelName);
            
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            disconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);

            if( !webSocket.IsClosed )
                webSocket.Disconnect();
        }

        public override void SendMessage(ChatMessage message)
        {
            if (Chat.IsAnonymous || String.IsNullOrWhiteSpace(message.Channel) ||
                String.IsNullOrWhiteSpace(message.FromUserName) ||
                String.IsNullOrWhiteSpace(message.Text))
                return;
            //["{\"type\":\"send_message\",\"data\":{\"channel_id\":2304,\"text\":\"asdf\",\"hideIcon\":false,\"mobile\":0}}"]
            var channelId = (Chat as GoodgameChat).GetChannelId(ChannelName);
            var messagePacket = new GoodgamePacket()
            {
                Type = "send_message",
                Data = new GoodGameData()
                {
                    ChannelId = channelId,
                    Text = message.Text,
                    IsIconHidden = false,
                    Mobile = 0,
                }
            };

            webSocket.Send(messagePacket.ToString());
        }
    }



    #region Goodgame Json
    [DataContract]
    public class GoodgamePacket
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }
        [DataMember(Name = "data")]
        public GoodGameData Data { get; set; }

        public override string ToString()
        {
            return @"[""" + JsonConvert.SerializeObject(this).Replace(@"""", @"\""") + @"""]";
        }

    }
    [DataContract]
    public class GoodGameChannelStatus
    {
        public string stream_id { get; set; }
        public string stream_key { get; set; }
        public string stream_name { get; set; }
        public string stream_status { get; set; }
    }

    [DataContract]
    public class GoodGameData
    {
        [DataMember(Name = "user_id", EmitDefaultValue = false, IsRequired = false)]
        public UInt32 UserId { get; set; }
        [DataMember(Name = "token", EmitDefaultValue = false, IsRequired = false)]
        public string Token { get; set; }
        [DataMember(Name = "channel_id", EmitDefaultValue = false, IsRequired = false)]
        public UInt32 ChannelId { get; set; }
        [DataMember(Name = "channel", EmitDefaultValue = false, IsRequired = false)]
        public UInt32 Channel { get; set; }
        [DataMember(Name = "hidden", EmitDefaultValue = false, IsRequired = false)]
        public bool IsHidden { get; set; }
        [DataMember(Name = "mobile", EmitDefaultValue = false, IsRequired = false)]
        public UInt32 Mobile { get; set; }
        [DataMember(Name = "text", EmitDefaultValue = false, IsRequired = false)]
        public string Text { get; set; }
        [DataMember(Name = "hideIcon", EmitDefaultValue = false, IsRequired = false)]
        public bool IsIconHidden { get; set; }
        [DataMember(Name = "protocolVersion", EmitDefaultValue = false, IsRequired = false)]
        public string ProtocolVersion { get; set; }
        [DataMember(Name = "serverIdent", EmitDefaultValue = false, IsRequired = false)]
        public string ServerIdentity { get; set; }
        [DataMember(Name = "user_name", EmitDefaultValue = false, IsRequired = false)]
        public string UserName { get; set; }
        [DataMember(Name = "channel_name", EmitDefaultValue = false, IsRequired = false)]
        public string ChannelName { get; set; }
        [DataMember(Name = "motd", EmitDefaultValue = false, IsRequired = false)]
        public string Greeting { get; set; }
        [DataMember(Name = "slowmod", EmitDefaultValue = false, IsRequired = false)]
        public UInt32 SlowMode { get; set; }
        [DataMember(Name = "smiles", EmitDefaultValue = false, IsRequired = false)]
        public UInt32 Smiles { get; set; }
        [DataMember(Name = "smilesPeka", EmitDefaultValue = false, IsRequired = false)]
        public UInt32 SmilesPeka { get; set; }
        [DataMember(Name = "clients_in_channel", EmitDefaultValue = false, IsRequired = false)]
        public UInt32 ClientsInChannel { get; set; }
        [DataMember(Name = "users_in_channel", EmitDefaultValue = false, IsRequired = false)]
        public UInt32 UsersInChannel { get; set; }
        [DataMember(Name = "access_rights", EmitDefaultValue = false, IsRequired = false)]
        public UInt32 AccessRights { get; set; }
        [DataMember(Name = "premium", EmitDefaultValue = false, IsRequired = false)]
        public UInt32 Premium { get; set; }
        [DataMember(Name = "is_banned", EmitDefaultValue = false, IsRequired = false)]
        public bool IsBanned { get; set; }
        [DataMember(Name = "banned_time", EmitDefaultValue = false, IsRequired = false)]
        public UInt32 BannedTime { get; set; }
        [DataMember(Name = "reason", EmitDefaultValue = false, IsRequired = false)]
        public string Reason { get; set; }
        [DataMember(Name = "payments", EmitDefaultValue = false, IsRequired = false)]
        public double Payments { get; set; }
        [DataMember(Name = "paidsmiles", EmitDefaultValue = false, IsRequired = false)]
        public object[] PaidSmiles { get; set; }
        [DataMember(Name = "user_rights", EmitDefaultValue = false, IsRequired = false)]
        public UInt32 UserRights { get; set; }
        [DataMember(Name = "message_id", EmitDefaultValue = false, IsRequired = false)]
        public UInt32 MessageId { get; set; }
        [DataMember(Name = "timestamp", EmitDefaultValue = false, IsRequired = false)]
        public UInt32 Timestamp { get; set; }
        [DataMember(Name = "count", EmitDefaultValue = false, IsRequired = false)]
        public UInt32? Count { get; set; }
    }

    #endregion

}
