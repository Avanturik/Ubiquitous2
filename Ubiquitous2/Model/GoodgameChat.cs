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
using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UB.Utils;

namespace UB.Model
{
    public class GoodgameChat : IChat, IStreamTopic
    {
        //API: https://docs.google.com/document/d/1vVtEhiqHhyW7wYor3zLh6PNu0_v_3LTboTQBPaRR1SY/edit?pli=1

        public event EventHandler<ChatServiceEventArgs> MessageReceived;
        private const string emoticonUrl = @"http://goodgame.ru/css/compiled/chat.css";
        //private const string emoticonUrl = @"http://goodgame.ru/css/compiled/smiles.css";
        private const string emoticonImageUrl = @"http://goodgame.ru/images/generated/smiles.png";
        private const string emoticonFallbackUrl = @"Content\goodgame_smiles.css";
        private string ownChannel;
        private string ownChannelId;
        private List<GoodgameChannel> goodgameChannels = new List<GoodgameChannel>();
        private List<WebPoller> counterWebPollers = new List<WebPoller>();
        private object iconParseLock = new object();
        private object channelsLock = new object();
        private object toolTipLock = new object();
        private object counterLock = new object();
        private object pollerLock = new object();
        private bool isFallbackEmoticons = false;
        private bool isWebEmoticons = false;
        private bool isAnonymous = true;
        private string chatToken = null;
        private WebClientBase webClient = new WebClientBase();
        public GoodgameChat(ChatConfig config)
        {
            Config = config;
            Enabled = Config.Enabled;

            ContentParsers = new List<Action<ChatMessage, IChat>>();
            ChatChannels = new List<string>();
            Emoticons = new List<Emoticon>();
            Status = new StatusBase();
            Status.ResetToDefault();
            Users = new Dictionary<string, ChatUser>();


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
        public UInt32 UserId = 0;

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

        public bool HideViewersCounter
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

            Log.WriteInfo("Starting Goodgame.ru chat");
            Status.ResetToDefault();
            Status.IsStarting = true;
            Status.IsConnecting = true;
            if (Login())
            {
                Task.Factory.StartNew(() => JoinChannels());
            }

            isFallbackEmoticons = false;
            isWebEmoticons = false;
            Task.Factory.StartNew( ()=> InitEmoticons());

            return true;
        }
        private bool Login()
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
            if (!isAnonymous)
            {
                Status.IsLoggedIn = true;
                GetTopic();
            }

            return true;
        }
        private string GoodgameGet(string url)
        {
            var content = webClient.Download(url);
            if( content != null && content.Length < 1000 && content.Contains("location.href="))
            {
                var cookieName = Re.GetSubString( content, @"\.cookie=\""(.*?)=");
                var cookieValue = Re.GetSubString( content, @"\.cookie=\"".*?=(.*?)""");
                var newHref = Re.GetSubString( content, @"location\.href=""(.*?)""");
                if( !String.IsNullOrWhiteSpace(cookieName) && !String.IsNullOrWhiteSpace(cookieValue) && !String.IsNullOrWhiteSpace(newHref))
                {
                    webClient.Encoding = System.Text.Encoding.UTF8;
                    webClient.SetCookie( cookieName, cookieValue, "goodgame.ru");
                    content = webClient.Download( newHref );
                }
            }
            return content;
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
                isAnonymous = true;
                return true;
            }

            if (String.IsNullOrWhiteSpace(authToken))
                return false;

            NickName = userName;

            webClient.SetCookie("PHPSESSID", authToken, "goodgame.ru");

            var content = GoodgameGet("http://goodgame.ru/chat/");
            
            if (String.IsNullOrWhiteSpace(content))
                return false;

            chatToken = Re.GetSubString(content, @"token.*?'(.*?)'");
            UInt32.TryParse(Re.GetSubString(content, @"userId.*?'(\d+)"), out UserId);
            if( UserId == 0 )
            {
                Config.SetParameterValue("AuthToken", String.Empty);
                isAnonymous = true;
                return false;
            }
            else
            {
                isAnonymous = false;
                Info.CanBeRead = true;
                Info.CanBeChanged = true;
                Config.SetParameterValue("AuthToken", authToken);
                Config.SetParameterValue("AuthTokenCredentials", userName + password);
            }

            return true;
        }

        private bool LoginWithUsername()
        {
            var userName = Config.GetParameterValue("Username") as string;
            var password = Config.GetParameterValue("Password") as string;

            if (String.IsNullOrWhiteSpace(userName) || String.IsNullOrWhiteSpace(password))
            {
                isAnonymous = true;
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
                isAnonymous = true;
                return false;
            }
            else
            {
                Config.SetParameterValue("AuthToken", authToken);
                Config.SetParameterValue("AuthTokenCredentials", userName + password);
                return LoginWithToken();
            }
        }

        public bool Stop()
        {
            if (!Enabled)
                Status.ResetToDefault();

            if (Status.IsStopping)
                return false;

            Log.WriteInfo("Stopping Goodgame chat");

            Status.IsStopping = true;
            Status.IsStarting = false;

            lock (channelsLock)
            {
                goodgameChannels.ForEach(chan =>
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
            return true;
        }

        public bool SendMessage(ChatMessage message)
        {
            if (isAnonymous)
                return false;

            var goodgameChannel = goodgameChannels.FirstOrDefault(channel => channel.ChannelName.Equals(message.Channel, StringComparison.InvariantCultureIgnoreCase));
            if (goodgameChannel != null)
            {
                if (String.IsNullOrWhiteSpace(message.FromUserName))
                {
                    message.FromUserName = NickName;
                }
                Task.Factory.StartNew(() => goodgameChannel.SendMessage(message));
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

        public Func<string, object> RequestData
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
        private UInt32 GetChannelId( string channelName )
        {
            UInt32 channelId = 0;
            var content = GoodgameGet(String.Format(@"http://goodgame.ru/api/getupcomingbroadcast?id={0}&fmt=json", channelName.Replace("#","")));
            var textChannelId = Re.GetSubString(content, @"stream_id.*?(\d+)");
            if( !String.IsNullOrWhiteSpace(textChannelId) )
            {
                UInt32.TryParse(textChannelId, out channelId);
            }
            return channelId;
        }
        private void JoinChannels()
        {

            if (Status.IsStopping)
                return;

            var serverUri = GetServerUri();

            var channels = Config.Parameters.StringArrayValue("Channels").Select(chan => "#" + chan.ToLower().Replace("#", "")).ToArray();

            if (!String.IsNullOrWhiteSpace( NickName) )
            {
                if (!channels.Contains("#" + NickName.ToLower()))
                    channels = channels.Union(new String[] { NickName.ToLower() }).ToArray();
            }

            foreach (var channel in channels)
            {

                var goodgameChannel = new GoodgameChannel(this);
                goodgameChannel.ServerUri = serverUri;
                goodgameChannel.ReadMessage = ReadMessage;
                goodgameChannel.LeaveCallback = (ggChannel) =>
                {
                    StopCounterPoller(ggChannel.ChannelName);
                    lock (channelsLock)
                        goodgameChannels.RemoveAll(item => item.ChannelName == ggChannel.ChannelName);

                    ChatChannels.RemoveAll(chan => chan == null);
                    ChatChannels.RemoveAll(chan => chan.Equals(ggChannel.ChannelName, StringComparison.InvariantCultureIgnoreCase));
                    if (RemoveChannel != null)
                        RemoveChannel(ggChannel.ChannelName, this);

                    if (!Status.IsStarting && !Status.IsStopping)
                    {
                        Restart();
                        return;
                    }
                };
                if (!goodgameChannels.Any(c => c.ChannelName == channel))
                {
                    goodgameChannel.ChannelId = GetChannelId(channel);
                    goodgameChannel.Join((ggChannel) =>
                    {
                        if (Status.IsStopping)
                            return;

                        UI.Dispatch(() => {
                            HideViewersCounter = false;
                            Status.IsConnected = true;
                            if (!ggChannel.IsAnonymous)
                            {
                                isAnonymous = false;
                                Status.IsLoggedIn = true;
                            }
                        });
                        lock (channelsLock)
                            goodgameChannels.Add(ggChannel);


                        if (RemoveChannel != null)
                            RemoveChannel(ggChannel.ChannelName, this);

                        ChatChannels.RemoveAll(chan => chan.Equals(ggChannel.ChannelName, StringComparison.InvariantCultureIgnoreCase));
                        ChatChannels.Add((ggChannel.ChannelName));
                        if (AddChannel != null)
                            AddChannel(ggChannel.ChannelName, this);

                        WatchChannelStats(ggChannel.ChannelName, ggChannel.ChannelId.ToString());

                    }, NickName, channel, chatToken);
                }
            }
        }
        void StopCounterPoller(string channelName)
        {
            UI.Dispatch(() =>
            {
                lock (toolTipLock)
                    Status.ToolTips.RemoveAll(t => t.Header == channelName);
            });
            var poller = counterWebPollers.FirstOrDefault(p => p.Id == channelName);
            if (poller != null)
            {
                poller.Stop();
                counterWebPollers.Remove(poller);
            }
        }
        public void WatchChannelStats(string channel, string channelId)
        {            
            var poller = new WebPoller()
            {
                Id = channel,
                //TODO: get streamid and watch plain text counter
                Uri = new Uri(String.Format(@"http://ftp.goodgame.ru/counter/{0}.txt?rnd=0.06468730559572577", channelId )),
            };

            UI.Dispatch(() =>
            {
                lock (toolTipLock)
                    Status.ToolTips.RemoveAll(t => t.Header == poller.Id);
            });
            UI.Dispatch(() =>
            {
                lock (toolTipLock)
                    Status.ToolTips.Add(new ToolTip(poller.Id, ""));
            });

            poller.ReadString = (text) =>
            {
                lock (counterLock)
                {
                    poller.LastValue = text;
                    var viewers = 0;
                    foreach (var webPoller in counterWebPollers.ToList())
                    {
                        Int32 viewersCount = 0;
                        var viewersCountText = webPoller.LastValue as string;
                        Int32.TryParse(viewersCountText, out viewersCount);

                        lock (toolTipLock)
                        {
                            var tooltip = Status.ToolTips.FirstOrDefault(t => t.Header.Equals(webPoller.Id));
                            if (tooltip == null)
                                return;

                            viewers += viewersCount;
                            tooltip.Text = String.IsNullOrWhiteSpace(viewersCountText)?"0":viewersCountText;
                            tooltip.Number = viewersCount;
                        }

                    }
                    UI.Dispatch(() => Status.ViewersCount = viewers);
                }
            };
            poller.Start();

            lock (pollerLock)
            {
                counterWebPollers.RemoveAll(p => p.Id == poller.Id);
                counterWebPollers.Add(poller);
            }
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
                        if (original != message.Text)
                            Log.WriteInfo("After parsing with {0}: {1}", number, message.Text);
                        number++;
                    });
                }

                MessageReceived(this, new ChatServiceEventArgs() { Message = message });

            }
        }
        public void DownloadEmoticons(string url)
        {

            lock (iconParseLock)
            {
                if (isFallbackEmoticons && isWebEmoticons)
                    return;

                var list = new List<Emoticon>();
                if (Emoticons == null)
                    Emoticons = new List<Emoticon>();


                var content = GoodgameGet(url);

                MatchCollection matches = Regex.Matches(content, @"}[^\.]*\.smile-([^-|\s]*)\s*{(.*?)}", RegexOptions.IgnoreCase);

                if (matches.Count <= 0 )
                {
                    Log.WriteError("Unable to get Goodgame.ru emoticons!");
                }
                else
                {
                    string originalUrl = null;
                    foreach (Match match in matches)
                    {
                        if( match.Groups.Count >= 2)
                        {
                            var smileName = match.Groups[1].Value;
                            var cssClassDefinition = match.Groups[2].Value;

                            var background = Css.GetBackground(cssClassDefinition);

                            if( background != null && !String.IsNullOrWhiteSpace(background.url) && background.width > 0 && background.height > 0)
                            {
                                originalUrl = String.Format("http://goodgame.ru/{0}", background.url.Replace("../../", ""));
                                var modifiedUrl = String.Format(@"/ubiquitous/cache?ubx={0}&uby={1}&ubw={2}&ubh={3}&uburl={4}", 
                                    background.x, background.y, background.width, background.height, HttpUtility.UrlEncode(originalUrl));

                                list.Add( new Emoticon(String.Format(":{0}:", smileName),
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
                        if( !String.IsNullOrWhiteSpace(originalUrl) && Uri.TryCreate( originalUrl, UriKind.Absolute, out uri ))
                        {
                            var ddosCookieGet = GoodgameGet("http://goodgame.ru");
                            if( ddosCookieGet != null )
                            {
                                var imageDataService = SimpleIoc.Default.GetInstance<IImageDataSource>();
                                UI.Dispatch (() => imageDataService.AddImage( uri, webClient.DownloadToMemoryStream(originalUrl)));
                            }
                            else
                            {
                                //get local image
                            }
                        }
                        Emoticons = list;
                        if (isFallbackEmoticons)
                            isWebEmoticons = true;

                        isFallbackEmoticons = true;
                    }
                }
            }
        }

        private void InitEmoticons()
        {
            //Fallback icon list
            DownloadEmoticons(AppDomain.CurrentDomain.BaseDirectory + emoticonFallbackUrl);
            //Web icons
            Task.Factory.StartNew(() => DownloadEmoticons(emoticonUrl));
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
                .ForEach( g => Games.Add(g));

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

                if( !String.IsNullOrWhiteSpace(ownChannel) )
                {
                    
                    content = GoodgameGet(String.Format(@"http://goodgame.ru/channel/{0}",ownChannel));
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
    }


    public class GoodgameChannel
    {
        private WebSocketBase webSocket;
        private GoodgameChat _chat;
        private Random random = new Random();
        private WebClientBase webClient = new WebClientBase();
        private UInt32 userId = 0;
        private Action<GoodgameChannel> joinCallback;
        private Dictionary<string, Action<GoodgameChannel, GoodGameData>>
            packetHandlers = new Dictionary<string, Action<GoodgameChannel, GoodGameData>>() {
                {"welcome", WelcomeHandler},
                {"success_auth", SuccessAuthHandler},
                {"success_join", SuccessJoinHandler},
                {"channel_counters", ChannelCountersHandler},
                {"message", MessageHandler},
            };

        public GoodgameChannel(GoodgameChat chat)
        {
            _chat = chat;
            userId = _chat.UserId;
            Status = new StatusBase();
            IsAnonymous = true;
        }
        public StatusBase Status { get; set; }
        
        public string NickName { get; set; }
        public string AuthToken { get; set; }
        public UInt32 ChannelId { get; set; }

        public bool IsAnonymous { get; set; }
        public Uri ServerUri { get; set; }
        private static void WelcomeHandler(GoodgameChannel channel, GoodGameData data)
        {
            Log.WriteInfo("Goodgame protocol version: {0}", data.ProtocolVersion);
            Log.WriteInfo("Goodgame servicer identity: {0}", data.ServerIdentity);

            channel.SendCredentials();
        }
        private static void SuccessAuthHandler(GoodgameChannel channel, GoodGameData data)
        {
            channel.Status.IsConnected = true;
            if (data.UserId == 0)
            {
                channel.IsAnonymous = true;
            }
            else
            {
                channel.Status.IsLoggedIn = true;
                channel.IsAnonymous = false;
            }

            channel.SendChannelJoin();
        }
        private void SendChannelJoin()
        {

            Log.WriteInfo("Goodgame serializing join packet. ChannelId: {0}", ChannelId);
            var joinPacket = new GoodgamePacket()
            {
                Type = "join",
                Data = new GoodGameData()
                {
                    ChannelId = ChannelId,
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
                channel.Status.IsConnecting = false;
                channel.Status.IsStarting = false;
                channel._chat.Status.IsConnected = true;
                channel._chat.Status.IsConnecting = false;
                channel._chat.Status.IsConnected = true;

                if (!channel.IsAnonymous)
                    channel.Status.IsLoggedIn = true;            
            });


            if (channel.joinCallback != null)
                channel.joinCallback(channel);

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

            if (channel.ReadMessage != null)
                channel.ReadMessage(new ChatMessage()
                {
                    Channel = channel.ChannelName,
                    ChatIconURL = channel._chat.IconURL,
                    ChatName = channel._chat.ChatName,
                    FromUserName = data.UserName,
                    HighlyImportant = false,
                    IsSentByMe = false,
                    Text = HttpUtility.HtmlDecode(data.Text),
                });
        }

        public void Join(Action<GoodgameChannel> callback, string nickName, string channel, string authToken)
        {

            if (String.IsNullOrWhiteSpace(channel))
                return;

            NickName = nickName;
            AuthToken = authToken;
            ChannelName = "#" + channel.Replace("#", "");

            webSocket = new WebSocketBase();
            webSocket.PingInterval = 0;
            webSocket.Origin = "http://goodgame.ru";
            joinCallback = callback;
            //webSocket.ConnectHandler = () =>
            //{

            //    if (callback != null)
            //        callback(this);
            //};

            webSocket.DisconnectHandler = () =>
            {
                Log.WriteError("Goodgame disconnected {0}", ChannelName);
                if (LeaveCallback != null)
                    LeaveCallback(this);
            };
            webSocket.ReceiveMessageHandler = ReadRawMessage;
            Connect();
        }

        private void SendCredentials()
        {
            var authPacket = new GoodgamePacket()
            {
                Type = "auth",
                Data = new GoodGameData()
                {
                    UserId = userId,
                    Token = AuthToken
                },
            };

            Log.WriteInfo("Goodgame sending {0}", authPacket.ToString());
            webSocket.Send(authPacket.ToString());
        }
        private void Connect()
        {
            Status.ResetToDefault();
            if( ServerUri == null )
            {
                webSocket.Path = String.Format("/chat/{0}/{1}/websocket", Rnd.RandomWebSocketServerNum(0x1e3), Rnd.RandomWebSocketString());
                webSocket.Port = "8081";
                webSocket.Host = "chat.goodgame.ru";
            }
            else
            {
                webSocket.Path = String.Format("{0}/{1}/{2}/websocket", ServerUri.AbsolutePath, Rnd.RandomWebSocketServerNum(0x1e3), Rnd.RandomWebSocketString());
                webSocket.Port = ServerUri.Port.ToString();
                webSocket.Host = ServerUri.Host;
            }
            webSocket.Connect();
        }
        private void ReadRawMessage(string rawMessage)
        {
            //Log.WriteInfo("Goodgame raw message received: {0}", rawMessage);
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
        public string ChannelName { get; set; }
        public void Leave()
        {
            Log.WriteInfo("Goodgame.ru leaving {0}", ChannelName);
            
            if( !webSocket.IsClosed )
                webSocket.Disconnect();
        }

        public Action<GoodgameChannel> LeaveCallback { get; set; }
        public Action<ChatMessage> ReadMessage { get; set; }
        public void SendMessage(ChatMessage message)
        {
            if (IsAnonymous || String.IsNullOrWhiteSpace(message.Channel) ||
                String.IsNullOrWhiteSpace(message.FromUserName) ||
                String.IsNullOrWhiteSpace(message.Text))
                return;

            var messagePacket = new GoodgamePacket()
            {
                Type = "send_message",
                Data = new GoodGameData()
                {
                    ChannelId = ChannelId,
                    Text = message.Text,
                    IsIconHidden = false,
                    Mobile = 0,
                }
            };

            webSocket.Send(messagePacket.ToString());
        }
    }


    [DataContract]
    public class GoodgamePacket
    {
        [DataMember(Name = "type")]
        public string Type { get; set;  }
        [DataMember(Name = "data")]
        public GoodGameData Data { get; set; }
        
        public override string ToString()
        {
            return @"[""" + JsonConvert.SerializeObject(this).Replace(@"""",@"\""") + @"""]";
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
    }

}
