using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UB.Utils;

namespace UB.Model
{
    public class CybergameChat : IChat, IStreamTopic
    {
        public event EventHandler<ChatServiceEventArgs> MessageReceived;
        private List<WebPoller> counterWebPollers = new List<WebPoller>();
        private List<CybergameChannel> cybergameChannels = new List<CybergameChannel>();
        private WebClientBase loginWebClient = new WebClientBase();
        private string emoticonFallbackUrl = @"Content\cybergame_smiles.html";
        private string emoticonUrl = "http://cybergame.tv/cgchat.htm?v=b";
        private object pollerLock = new object();
        private object channelsLock = new object();
        private object counterLock = new object();
        private object toolTipLock = new object();
        private object iconParseLock = new object();
        private bool isWebEmoticons = false;
        private bool isFallbackEmoticons = false;
        private bool isAnonymous = true;
        public CybergameChat(ChatConfig config)
        {
            Config = config;
            ContentParsers = new List<Action<ChatMessage, IChat>>();
            ChatChannels = new List<string>();
            Emoticons = new List<Emoticon>();
            Status = new StatusBase();
            Users = new Dictionary<string, ChatUser>();

            Info = new StreamInfo()
            {
                HasDescription = false,
                HasGame = true,
                HasTopic = true,
                ChatName = Config.ChatName,
            };
            
            ContentParsers.Add(MessageParser.ParseURLs);
            ContentParsers.Add(MessageParser.ParseEmoticons);
            
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

            Log.WriteInfo("Starting Cybergame.tv chat");
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

            Log.WriteInfo("Stopping Cybergame.tv chat");

            Status.IsStopping = true;
            Status.IsStarting = false;

            lock (channelsLock)
            {
                cybergameChannels.ForEach(chan =>
                {
                    StopCounterPoller(chan.ChannelName);
                    chan.Leave();
                    if (RemoveChannel != null)
                        RemoveChannel(chan.ChannelName, this);
                });
            }
            ChatChannels.Clear();
            return true;
        }



        public bool Restart()
        {
            if (Status.IsStopping || Status.IsStarting)
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

            var cybergameChannel = cybergameChannels.FirstOrDefault(channel => channel.ChannelName.Equals(message.Channel, StringComparison.InvariantCultureIgnoreCase));
            if (cybergameChannel != null)
            {
                if (String.IsNullOrWhiteSpace(message.FromUserName))
                {
                    message.FromUserName = NickName;
                }
                Task.Factory.StartNew(() => cybergameChannel.SendMessage(message));
                ReadMessage(message);
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
           
        }

        public void GetTopic()
        {
           
        }

        public void SetTopic()
        {
            
        }

        public Action StreamTopicAcquired
        {
            get;
            set;

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
                Log.WriteInfo("Cybergame authorization exception {0}", e.Message);
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
            {
                Config.SetParameterValue("AuthToken", String.Empty);
                return false;
            }

            if (String.IsNullOrEmpty(userName))
            {
                isAnonymous = true;
                return true;
            }

            if (String.IsNullOrWhiteSpace(authToken))
                return false;

            NickName = userName;

            loginWebClient.SetCookie("kname", userName, "cybergame.tv");
            loginWebClient.SetCookie("khash", userName, "cybergame.tv");

            var test = this.With(x => loginWebClient.Download("http://cybergame.tv"));
            
            if( test != null && test.Contains("logout.php") )
                return true;

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

            var authString = String.Format(@"action=login&username={0}&pass={1}&remember_me=1", userName, password);

            loginWebClient.ContentType = ContentType.UrlEncoded;

            var authToken = this.With(x => loginWebClient.Upload("http://cybergame.tv/login.php", authString))
                            .With(x => Re.GetSubString(x, @"khash[^""]+""([^""]+)"""));

            if (authToken == null)
            {
                Log.WriteError("Login to cybergame.tv failed. Joining anonymously");
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
        private void JoinChannels()
        {
            if (Status.IsStopping)
                return;

            var channels = Config.Parameters.StringArrayValue("Channels").Select(chan => "#" + chan.ToLower().Replace("#", "")).ToArray();

            if ( !String.IsNullOrWhiteSpace( NickName ) )
            {
                if (!channels.Contains("#" + NickName.ToLower()))
                    channels = channels.Union(new String[] { NickName.ToLower() }).ToArray();
            }

            foreach (var channel in channels)
            {
                var cybergameChannel = new CybergameChannel(this);
                cybergameChannel.ReadMessage = ReadMessage;
                cybergameChannel.LeaveCallback = (cybChannel) =>
                {
                    StopCounterPoller(cybChannel.ChannelName);
                    lock (channelsLock)
                        cybergameChannels.RemoveAll(item => item.ChannelName == cybChannel.ChannelName);

                    ChatChannels.RemoveAll(chan => chan == null);
                    ChatChannels.RemoveAll(chan => chan.Equals(cybChannel.ChannelName, StringComparison.InvariantCultureIgnoreCase));
                    if (RemoveChannel != null)
                        RemoveChannel(cybChannel.ChannelName, this);

                    if (!Status.IsStarting && !Status.IsStopping)
                    {
                        Restart();
                        return;
                    }
                };
                if (!cybergameChannels.Any(c => c.ChannelName == channel))
                    cybergameChannel.Join((cybChannel) =>
                    {
                        if (Status.IsStopping)
                            return;

                        Status.IsConnected = true;
                        lock (channelsLock)
                            cybergameChannels.Add(cybChannel);


                        if (RemoveChannel != null)
                            RemoveChannel(cybChannel.ChannelName, this);

                        ChatChannels.RemoveAll(chan => !String.IsNullOrWhiteSpace(chan) && chan.Equals(cybChannel.ChannelName, StringComparison.InvariantCultureIgnoreCase));
                        ChatChannels.Add((cybChannel.ChannelName));
                        if (AddChannel != null)
                            AddChannel(cybChannel.ChannelName, this);

                        WatchChannelStats(cybChannel.ChannelName);

                    }, NickName, channel, (String)Config.GetParameterValue("AuthToken"));
            }
        }

        public void WatchChannelStats(string channel)
        {
            
            var poller = new WebPoller()
            {
                Id = channel,
                Uri = new Uri(String.Format(@"http://api.cybergame.tv/p/statusv2/?channel={0}", channel.Replace("#", ""))),
            };
            Log.WriteInfo(poller.Uri.OriginalString);
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

            poller.ReadString = (stream) =>
            {
                lock (counterLock)
                {
                    var channelInfo = JsonConvert.DeserializeObject<CybergameChannelStatus>(stream);
                    poller.LastValue = channelInfo;
                    var viewers = 0;
                    foreach (var webPoller in counterWebPollers.ToList())
                    {
                        var streamInfo = (CybergameChannelStatus)webPoller.LastValue;
                        int streamInfoViewers = 0;


                        lock (toolTipLock)
                        {
                            var tooltip = Status.ToolTips.FirstOrDefault(t => t.Header.Equals(webPoller.Id));
                            if (tooltip == null)
                                return;

                            if (streamInfo != null && int.TryParse(streamInfo.spectators, out streamInfoViewers))
                            {
                                viewers += streamInfoViewers;
                                tooltip.Text = streamInfo.spectators;
                                tooltip.Number = streamInfoViewers;
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

            lock (pollerLock)
            {
                counterWebPollers.RemoveAll(p => p.Id == poller.Id);
                counterWebPollers.Add(poller);
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
            if (isFallbackEmoticons && isWebEmoticons)
                return;

            lock (iconParseLock)
            {
                var list = new List<Emoticon>();
                if (Emoticons == null)
                    Emoticons = new List<Emoticon>();
                
                var test = loginWebClient.Download(url);
                if (test != null)
                    Log.WriteInfo("");
                var emoticonsMatches = this.With( x => loginWebClient.Download("http://cybergame.tv/cgchat.htm?v=b"))
                    .With( x => Regex.Matches(x,@"""(.*?)"":""(smiles/.*?)"""));

                if( emoticonsMatches.Count <= 0 )
                {
                    Log.WriteError("Unable to get Cybergame.tv emoticons!");
                    return;
                }
                else
                {
                    foreach (Match match in emoticonsMatches)
                    {
                        if (match.Groups.Count <= 2)
                            continue;

                        var smileUrl = "http://cybergame.tv/" + match.Groups[2].Value;
                        var regex = Regex.Escape(match.Groups[1].Value);

                        if (String.IsNullOrWhiteSpace(regex) || String.IsNullOrWhiteSpace(smileUrl))
                            continue;

                        list.Add(new Emoticon(regex, smileUrl, 0, 0));
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
    }
    public class CybergameChannel
    {
        private WebSocketBase webSocket;
        private CybergameChat _chat;
        private Random random = new Random();
        private bool isAnonymous = false;
        private Dictionary<string, Action<CybergameChannel, CybergameData>>
            packetHandlers = new Dictionary<string, Action<CybergameChannel, CybergameData>>() {
                        {"changeWindow", SuccessfulConnect},
                        {"setUI", SuccessfulLogin},
                        {"chatMessage", ChatMessageReceive},
                        {"listUsers", ListUsers},
            };
        public CybergameChannel(CybergameChat chat)
        {
            _chat = chat;
            Status = new StatusBase();
        }
        public StatusBase Status { get; set; }
        public string NickName { get; set; }
        public string AuthToken { get; set; }

        private static void SuccessfulConnect(CybergameChannel channel, CybergameData data)
        {
            channel._chat.Status.IsConnected = true;
            if (channel.joinCallback != null)
                channel.joinCallback(channel);
        }

        private static void SuccessfulLogin(CybergameChannel channel, CybergameData data)
        {
            channel._chat.Status.IsLoggedIn = true;
        }

        private static void ChatMessageReceive(CybergameChannel channel, CybergameData data)
        {
            if (String.IsNullOrWhiteSpace(data.From) || String.IsNullOrWhiteSpace(data.Text))
                return;

            if (channel.ReadMessage != null)
                channel.ReadMessage(new ChatMessage()
                {
                    Channel = channel.ChannelName,
                    ChatIconURL = channel._chat.IconURL,
                    ChatName = channel._chat.ChatName,
                    FromUserName = data.From,
                    HighlyImportant = false,
                    IsSentByMe = false,
                    Text = HttpUtility.HtmlDecode(data.Text),
                });
        }

        private static void ListUsers(CybergameChannel channel, CybergameData data)
        {

        }

        public void Join(Action<CybergameChannel> callback, string nickName, string channel, string authToken)
        {

            if (String.IsNullOrWhiteSpace(channel))
                return;
            NickName = nickName;
            AuthToken = authToken;
            ChannelName = "#" + channel.Replace("#", "");
            webSocket = new WebSocketBase();
            webSocket.Origin = "http://www.cybergame.tv";
            webSocket.ConnectHandler = () =>
            {
            };

            webSocket.DisconnectHandler = () =>
            {
                Log.WriteError("Cybergame.tv disconnected {0}", ChannelName);
                if (LeaveCallback != null)
                    LeaveCallback(this);
            };
            joinCallback = callback;

            webSocket.ReceiveMessageHandler = ReadRawMessage;
            webSocket.Path = String.Format("/{0}/{1}/websocket", Rnd.RandomWebSocketServerNum(0x1e3), Rnd.RandomWebSocketString());
            webSocket.Port = "9090";
            webSocket.Host = "cybergame.tv";
            Status.ResetToDefault();
            webSocket.Connect();
        }

        private void ReadRawMessage(string rawMessage)
        {
            Log.WriteInfo("Cybergame raw message: {0}", rawMessage);
            if( rawMessage.Equals("o") )
            {
                SendCredentials();
                return;
            }

            if (rawMessage.StartsWith("a"))
            {
                var packet = this.With(x => JArray.Parse(rawMessage.Substring(1)))
                    .With(x => x[0])
                    .With(x => x.Value<string>().Replace(@"\","").Replace(@":""{",":{").Replace(@"}""}","}}"))
                    .With(x => JsonConvert.DeserializeObject<CybergamePacket>(x));

                if (packet == null)
                    return;

                if (packet.Message != null && packetHandlers.ContainsKey(packet.Command))
                    packetHandlers[packet.Command](this, packet.Message);
            }
        }

        private void SendCredentials()
        {
            var authPacket = new CybergamePacket()
            {
                Command = "login",
                Message = new CybergameData()
                {
                    Login = NickName ?? "",
                    Password = AuthToken ?? "",
                    Channel = ChannelName,
                },
            };

            Log.WriteInfo("Cybergame sending {0}", authPacket.ToString());
            webSocket.Send(authPacket.ToString());
        }

        public string ChannelName { get; set; }
        public void Leave()
        {
            Log.WriteInfo("Cybergame leaving {0}", ChannelName);
            webSocket.Disconnect();
        }

        private Action<CybergameChannel> joinCallback;
        public Action<CybergameChannel> LeaveCallback { get; set; }
        public Action<ChatMessage> ReadMessage { get; set; }
        public void SendMessage(ChatMessage message)
        {
            if (isAnonymous || String.IsNullOrWhiteSpace(message.Channel) ||
                String.IsNullOrWhiteSpace(message.FromUserName) ||
                String.IsNullOrWhiteSpace(message.Text))
                return;

            var messagePacket = new CybergamePacket()
            {
                Command = "sendChatMessage",
                Message = new CybergameData()
                {
                    Message = message.Text,
                }
            };

            webSocket.Send(messagePacket.ToString());
        }
    }

    [DataContract]
    public class CybergamePacket
    {
        [DataMember(Name = "command")]
        public string Command { get; set; }
        [DataMember(Name = "message")]
        public CybergameData Message { get; set; }

        public override string ToString()
        {
            return @"[""" +  @"{\""command\"":\""" + Command + @"\"",\""message\"":\""" +  Message.ToString()  + @"\""}""]";
        }

    }

    public class CybergameChannelStatus
    {
        [DataMember(Name = "online")]
        public string online { get; set; }
        [DataMember(Name = "spectators")]
        public string spectators { get; set; }
        [DataMember(Name = "followers")]
        public string followers { get; set; }
        [DataMember(Name = "donates")]
        public List<object> donates { get; set; }
    }

    [DataContract]
    public class CybergameData
    {
        [DataMember(Name = "window", EmitDefaultValue = false, IsRequired = false)]
        public string Window { get; set; }
        [DataMember(Name = "write", EmitDefaultValue = false, IsRequired = false)]
        public bool Write { get; set; }
        [DataMember(Name = "when", EmitDefaultValue = false, IsRequired = false)]
        public string When { get; set; }
        [DataMember(Name = "from", EmitDefaultValue = false, IsRequired = false)]
        public string From { get; set; }
        [DataMember(Name = "text", EmitDefaultValue = false, IsRequired = false)]
        public string Text { get; set; }
        [DataMember(Name = "nicklist", EmitDefaultValue = false, IsRequired = false)]
        public List<string> NickList { get; set; }
        [DataMember(Name = "banlist", EmitDefaultValue = false, IsRequired = false)]
        public List<object> BanList { get; set; }
        [DataMember(Name = "oplist", EmitDefaultValue = false, IsRequired = false)]
        public List<object> OpList { get; set; }
        [DataMember(Name = "parsing", EmitDefaultValue = false, IsRequired = false)]
        public bool Parsing { get; set; }
        [DataMember(Name = "lastupd", EmitDefaultValue = false, IsRequired = false)]
        public UInt64 LastUpdate { get; set; }
        [DataMember(Name = "message", EmitDefaultValue = false, IsRequired = false)]
        public string Message { get; set; }
        [DataMember(Name = "login", EmitDefaultValue = false, IsRequired = false)]
        public string Login { get; set; }
        [DataMember(Name = "password", EmitDefaultValue = false, IsRequired = false)]
        public string Password { get; set; }
        [DataMember(Name = "channel", EmitDefaultValue = false, IsRequired = false)]
        public string Channel { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this).Replace(@"""", @"\\\""");
        }
    }
}
