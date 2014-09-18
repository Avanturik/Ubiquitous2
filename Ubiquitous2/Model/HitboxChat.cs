using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UB.Utils;

namespace UB.Model
{
    public class HitboxChat : IChat
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

        public HitboxChat(ChatConfig config)
        {
            Config = config;
            ContentParsers = new List<Action<ChatMessage, IChat>>();
            ChatChannels = new List<string>();
            Emoticons = new List<Emoticon>();
            Status = new StatusBase();
            Users = new Dictionary<string, ChatUser>();

            ContentParsers.Add(MessageParser.ParseURLs);
            ContentParsers.Add(MessageParser.ParseSpaceSeparatedEmoticons);

            Enabled = Config.Enabled;
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
            Status.IsStopping = true;
            hitboxChannels.ForEach(chan =>
            {
                StopCounterPoller(chan.ChannelName);
                chan.Leave();
            });
            Status.ResetToDefault();
            return true;
        }

        public bool Restart()
        {
            Stop();
            Start();
            return true;
        }

        public bool SendMessage(ChatMessage message)
        {
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
            var authToken = Config.GetParameterValue("AuthToken") as string;
            if (!LoginWithToken())
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

        public bool LoginWithToken()
        {
            return false;
            var authToken = (string)Config.GetParameterValue("AuthToken");
            if (String.IsNullOrWhiteSpace(authToken))
                return false;

            SetCommonHeaders();
            loginWebClient.Headers["Auth-Token"] = authToken;

            var response = this.With(x => loginWebClient.Download("https://api.gaminglive.tv/auth/me"))
                                .With(x => JToken.Parse(x));

            if (response == null)
                return false;

            var isOk = response.Value<bool>("ok");
            NickName = (string)response.Value<dynamic>("user").login;

            if (isOk && !String.IsNullOrWhiteSpace(NickName))
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
                Config.SetParameterValue("AuthToken", authToken);
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
                if (ContentParsers != null)
                    ContentParsers.ForEach(parser => parser(message, this));

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

            UI.Dispatch(() => Status.ToolTips.RemoveAll(t => t.Header == poller.Id));
            UI.Dispatch(() => Status.ToolTips.Add(new ToolTip(poller.Id, "")));

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
                    UI.Dispatch(() => Status.ViewersCount = viewers);
                }
            };
            poller.Start();

            counterWebPollers.Add(poller);
        }
        void StopCounterPoller(string channelName)
        {
            UI.Dispatch(() => Status.ToolTips.RemoveAll(t => t.Header == channelName));
            var poller = counterWebPollers.FirstOrDefault(p => p.Id == channelName);
            if (poller != null)
            {
                poller.Stop();
                counterWebPollers.Remove(poller);
            }
        }
        private void JoinChannels()
        {
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
                    hitboxChannels.RemoveAll(item => item.ChannelName == hbChannel.ChannelName);
                    ChatChannels.RemoveAll(chan => chan.Equals(hbChannel.ChannelName, StringComparison.InvariantCultureIgnoreCase));
                    if (RemoveChannel != null)
                        RemoveChannel(hitboxChannel.ChannelName, this);
                };

                hitboxChannel.Join((hbChannel) =>
                {
                    Status.IsConnected = true;
                    hitboxChannels.Add(hbChannel);
                    if (hbChannel.ChannelName.Equals("#" + NickName, StringComparison.InvariantCultureIgnoreCase))
                        Status.IsLoggedIn = true;

                    ChatChannels.RemoveAll(chan => chan.Equals(hbChannel.ChannelName, StringComparison.InvariantCultureIgnoreCase));
                    ChatChannels.Add((hbChannel.ChannelName));
                    if (AddChannel != null)
                        AddChannel(hitboxChannel.ChannelName, this);

                    WatchChannelStats(hitboxChannel.ChannelName);

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
            if (isFallbackEmoticons)
                return;
            if (isWebEmoticons)
                return;
            lock (iconParseLock)
            {
                var list = new List<Emoticon>();
                if (Emoticons == null)
                    Emoticons = new List<Emoticon>();

                var jsonEmoticons = this.With(x => Json.DeserializeUrl<JObject>(url));

                if (jsonEmoticons == null)
                {
                    Log.WriteError("Unable to get Twitch.tv emoticons!");
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
                        if( pair.Value.Length > 0 )
                        {
                            list.Add(new Emoticon(null, smileUrl, defaultWidth, defaultHeight)
                            {
                                ExactWord = pair.Key,
                            });

                            if (pair.Value.Length > 1)
                            list.Add(new Emoticon(null, smileUrl, defaultWidth, defaultHeight)
                            {
                                ExactWord = pair.Value[1],
                            });

                            if (pair.Value.Length > 2 && !pair.Value[1].Equals(pair.Value[2]))
                            list.Add(new Emoticon(null, smileUrl, defaultWidth, defaultHeight)
                            {
                                ExactWord = pair.Value[2],
                            });
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

    }

    public class HitboxChannel
    {
        private WebSocketBase webSocket;
        private HitboxChat _chat;
        private Random random = new Random();
        private Timer pingTimer;
        private bool isAnonymous = false;
        public HitboxChannel(HitboxChat chat)
        {
            _chat = chat;
            pingTimer = new Timer((obj) => {
                webSocket.Send("2::");
            }, null, Timeout.Infinite, Timeout.Infinite);
        }
       
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
                Thread.Sleep(16);
            }
            Task.WaitAll(hostTestTasks, 3000);

            if (resultList.Count() <= 0)
            {
                Log.WriteError("All hitbox servers are down.");
                callback(null);
                return;
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

            ChannelName = "#" + channel.Replace("#", "");
            webSocket = new WebSocketBase();
            webSocket.Origin = "http://www.hitbox.tv";
            webSocket.ConnectHandler = () =>
            {
                //pingTimer.Change(webSocket.PingInterval, webSocket.PingInterval);
                SendCredentials(nickName, channel, authToken);

                if (callback != null)
                    callback(this);
            };
            
            webSocket.DisconnectHandler = () =>
            {
                Log.WriteError("Hitbox disconnected");
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
            if( rawMessage.Equals("2::"))
            {
                webSocket.Send("2::");
                return;
            }

            if( !String.IsNullOrWhiteSpace(rawMessage) && rawMessage.Contains("chatMsg"))
            {
                var json = Re.GetSubString(rawMessage, @".*args"":\[""(.*?)""\]}$");
                if( json == null )
                    return;
                dynamic msg = this.With(x => JToken.Parse(json.Replace(@"\""", @"""").Replace(@"\\",@"\")))
                    .With(x => x.Value<dynamic>("params"));

                if( msg == null)
                    return;

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
        public string ChannelName { get; set; }
        public void Leave()
        {
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
