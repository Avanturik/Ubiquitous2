using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using UB.Utils;

namespace UB.Model
{
    public class GoodgameChat : IChat, IStreamTopic
    {
        public event EventHandler<ChatServiceEventArgs> MessageReceived;
        private const string emoticonUrl = @"http://goodgame.ru/css/compiled/chat.css";
        //private const string emoticonUrl = @"http://goodgame.ru/css/compiled/smiles.css";
        private const string emoticonImageUrl = @"http://goodgame.ru/images/generated/smiles.png";
        private const string emoticonFallbackUrl = @"Content\goodgame_smiles.css";
        private List<GoodgameChannel> goodgameChannels = new List<GoodgameChannel>();
        private List<WebPoller> counterWebPollers = new List<WebPoller>();
        private object iconParseLock = new object();
        private object channelsLock = new object();
        private object toolTipLock = new object();
        private object counterLock = new object();
        private object pollerLock = new object();
        private bool isFallbackEmoticons = false;
        private bool isWebEmoticons = false;
        private WebClientBase webClient = new WebClientBase();
        public GoodgameChat(ChatConfig config)
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

            Log.WriteInfo("Starting Goodgame.ru chat");
            Status.ResetToDefault();
            Status.IsStarting = true;

            if (Login())
            {
                Status.IsConnecting = true;
                Task.Factory.StartNew(() => JoinChannels());
            }

            isFallbackEmoticons = false;
            isWebEmoticons = false;
            InitEmoticons();

            return true;
        }
        private bool Login()
        {
            return true;
        }
        public bool Stop()
        {
            return true;
        }

        public bool Restart()
        {
            return true;
        }

        public bool SendMessage(ChatMessage message)
        {
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

        private void JoinChannels()
        {

            if (Status.IsStopping)
                return;

            var channels = Config.Parameters.StringArrayValue("Channels").Select(chan => "#" + chan.ToLower().Replace("#", "")).ToArray();

            if (!String.IsNullOrWhiteSpace( NickName) )
            {
                if (!channels.Contains("#" + NickName.ToLower()))
                    channels = channels.Union(new String[] { NickName.ToLower() }).ToArray();
            }

            foreach (var channel in channels)
            {

                var goodgameChannel = new GoodgameChannel(this);
                goodgameChannel.ReadMessage = ReadMessage;
                goodgameChannel.LeaveCallback = (ggChannel) =>
                {
                    StopCounterPoller(ggChannel.ChannelName);
                    lock (channelsLock)
                        goodgameChannels.RemoveAll(item => item.ChannelName == ggChannel.ChannelName);

                    ChatChannels.RemoveAll(chan => chan == null);
                    ChatChannels.RemoveAll(chan => chan.Equals(ggChannel.ChannelName, StringComparison.InvariantCultureIgnoreCase));
                    if (RemoveChannel != null)
                        RemoveChannel(goodgameChannel.ChannelName, this);

                    if (!Status.IsStarting && !Status.IsStopping)
                    {
                        Restart();
                        return;
                    }
                };
                if (!goodgameChannels.Any(c => c.ChannelName == channel))
                    goodgameChannel.Join((hbChannel) =>
                    {
                        if (Status.IsStopping)
                            return;

                        Status.IsConnected = true;
                        lock (channelsLock)
                            goodgameChannels.Add(hbChannel);

                        ChatChannels.RemoveAll(chan => chan.Equals(hbChannel.ChannelName, StringComparison.InvariantCultureIgnoreCase));
                        ChatChannels.Add((hbChannel.ChannelName));
                        if (AddChannel != null)
                            AddChannel(goodgameChannel.ChannelName, this);

                        WatchChannelStats(goodgameChannel.ChannelName);

                    }, NickName, channel, (String)Config.GetParameterValue("AuthToken"));
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
        public void WatchChannelStats(string channel)
        {            
            return;

            var poller = new WebPoller()
            {
                Id = channel,
                //TODO: get streamid and watch plain text counter
                Uri = new Uri(String.Format(@"http://ftp.goodgame.ru/counter/{0}.txt?rnd=0.06468730559572577", channel.Replace("#", ""))),
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
                            tooltip.Text = viewersCountText;
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
            if (isFallbackEmoticons)
                return;
            if (isWebEmoticons)
                return;

            lock (iconParseLock)
            {
                var list = new List<Emoticon>();
                if (Emoticons == null)
                    Emoticons = new List<Emoticon>();


                var content = webClient.Download(url);

                MatchCollection matches = Regex.Matches(content, @"\.smiles\.([^{|\s|\n|\t]+)\s*\{\s*([^}]+)\s*}", RegexOptions.IgnoreCase);

                if (matches.Count <= 0 )
                {
                    Log.WriteError("Unable to get Goodgame.ru emoticons!");
                }
                else
                {
                    foreach (Match match in matches)
                    {
                        if( match.Groups.Count >= 2)
                        {
                            var smileName = match.Groups[1].Value;
                            var cssClassDefinition = match.Groups[2].Value;

                            var background = Css.GetBackground(cssClassDefinition);

                            if( background != null && !String.IsNullOrWhiteSpace(url) && background.width > 0 && background.height > 0)
                            {
                                var originalUrl = String.Format("http://goodgame.ru/{0}", background.url.Replace("../../", ""));
                                var modifiedUrl = Url.AppendParameter(originalUrl, "ubx", background.x);
                                modifiedUrl = Url.AppendParameter(modifiedUrl, "uby", background.y);

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
    }


    public class GoodgameChannel
    {
        private WebSocketBase webSocket;
        private GoodgameChat _chat;
        private Random random = new Random();
        private bool isAnonymous = false;

        public GoodgameChannel(GoodgameChat chat)
        {
            _chat = chat;
            Status = new StatusBase();
        }
        public StatusBase Status { get; set; }
        
        public string NickName { get; set; }
        public string AuthToken { get; set; }
        
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
            webSocket.ConnectHandler = () =>
            {
                SendCredentials(NickName, channel, authToken);

                if (callback != null)
                    callback(this);
            };

            webSocket.DisconnectHandler = () =>
            {
                Log.WriteError("Goodgame disconnected {0}", ChannelName);
                if (LeaveCallback != null)
                    LeaveCallback(this);
            };
            webSocket.ReceiveMessageHandler = ReadRawMessage;
            Connect();
        }

        private void SendCredentials(string nickname, string channel, string authToken)
        {
            isAnonymous = String.IsNullOrWhiteSpace(authToken) || String.IsNullOrWhiteSpace(nickname);
            //webSocket.Send(...);
        }
        private void Connect()
        {
            Status.ResetToDefault();

            webSocket.Path = String.Format("/chat/{0}/{1}/websocket", Rnd.RandomWebSocketServerNum(0x1e3), Rnd.RandomWebSocketString());
            webSocket.Port = "8080";
            webSocket.Host = "chat.goodgame.ru";
            webSocket.Connect();
        }
        private void ReadRawMessage(string rawMessage)
        {
            //TODO: parse gg raw messages
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
            if (isAnonymous || String.IsNullOrWhiteSpace(message.Channel) ||
                String.IsNullOrWhiteSpace(message.FromUserName) ||
                String.IsNullOrWhiteSpace(message.Text))
                return;

            //webSocket.Send(asdf);
        }
    }

}
