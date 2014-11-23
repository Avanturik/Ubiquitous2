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
    public class CybergameChat : ChatBase, IStreamTopic
    {
        private WebClientBase loginWebClient = new WebClientBase();
        private List<KeyValuePair<string, string>> webGameList = new List<KeyValuePair<string, string>>();
        private List<KeyValuePair<String, String>> profileFormParams;
        private string webChannelId;
        private object iconParseLock = new object();
        private object lockSearch = new object();


        public CybergameChat(ChatConfig config) : base(config)
        {
            EmoticonFallbackUrl = @"Content\cybergame_smiles.html";
            EmoticonUrl = "http://cybergame.tv/cgchat.htm?v=b";

            CreateChannel = () => { return new CybergameChannel(this); };
            ReceiveOwnMessages = true;

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
            lock (lockSearch)
            {
                Games.Clear();
                webGameList.Where(game => game.Value.ToLower()
                    .StartsWith(gameName.ToLower()))
                    .Select(game => new Game() { Id = game.Key, Name = game.Value })
                    .ToList()
                    .ForEach(game => Games.Add(game));

                if (callback != null)
                    UI.Dispatch(() => callback());
            }
        }

        public void GetTopic()
        {
            if (!Status.IsLoggedIn || !Enabled)
                return;

            Task.Factory.StartNew(() =>
            {
                loginWebClient.ContentType = ContentType.UrlEncodedUTF8;
                var content = loginWebClient.Download(String.Format(@"http://cybergame.tv/my_profile_edit/?rand={0}", Time.UnixTimestamp()));
                Info.Topic = Html.GetInnerText(@"//textarea[@name='channel_desc']", content);
                Info.CurrentGame.Name = Html.GetSiblingInnerText(@"//select/option[@selected='selected']", content);
                webGameList = Html.GetOptions(@"//select[@name='channel_game']/option", content);
                webChannelId = Html.GetAttribute(@"//input[@name='channel']", "value", content);
                profileFormParams = Html.FormParams(@"", content);
                Info.CurrentGame.Id = webGameList.Where(v => v.Value.Equals(Info.CurrentGame.Name, StringComparison.InvariantCultureIgnoreCase)).Select(g => g.Key).FirstOrDefault();
                Info.CanBeRead = true;
                Info.CanBeChanged = true;

                if (StreamTopicAcquired != null)
                    UI.Dispatch(() => StreamTopicAcquired());
            });
        }

        public void SetTopic()
        {
            if (profileFormParams == null || !Status.IsLoggedIn)
                return;

            String param = "a=save_profile&channel_game={0}&channel_desc={1}&channel={2}&display_name={3}&channel_name={4}";
            var displayName = profileFormParams.Where(k => k.Key.StartsWith("display_name_", StringComparison.CurrentCultureIgnoreCase)).Select(v => v.Value).FirstOrDefault();
            var channelName = profileFormParams.Where(k => k.Key.StartsWith("channel_name_", StringComparison.CurrentCultureIgnoreCase)).Select(v => v.Value).FirstOrDefault();
            if (String.IsNullOrEmpty(displayName))
                displayName = NickName;

            loginWebClient.ContentType = ContentType.UrlEncodedUTF8;
            loginWebClient.Headers["X-Requested-With"] = "XMLHttpRequest";

            Info.CurrentGame.Id = webGameList.FirstOrDefault(game => game.Value.Equals(Info.CurrentGame.Name, StringComparison.InvariantCultureIgnoreCase)).Key;

            loginWebClient.Upload(String.Format(@"http://cybergame.tv/my_profile_edit/?mode=async&rand={0}", Time.UnixTimestamp()),
                String.Format(param, Info.CurrentGame.Id, HttpUtility.UrlEncode(Info.Topic), webChannelId, HttpUtility.UrlEncode(displayName), HttpUtility.UrlEncode(channelName)));
        }

        public Action StreamTopicAcquired
        {
            get;
            set;

        }
      
        #endregion

        #region IChat
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
                Log.WriteInfo("Cybergame authorization exception {0}", e.Message);
                return false;
            }
            if (!IsAnonymous)
            {
                Status.IsLoggedIn = true;
            }

            return true;
        }

        public bool LoginWithToken()
        {
            var authToken = Config.GetParameterValue("AuthToken") as string;
            var userName = Config.GetParameterValue("Username") as string;
            NickName = userName;

            var password = Config.GetParameterValue("Password") as string;
            var tokenCredentials = Config.GetParameterValue("AuthTokenCredentials") as string;

            if (tokenCredentials != userName + password)
            {
                Config.SetParameterValue("AuthToken", String.Empty);
                return false;
            }

            if (String.IsNullOrEmpty(userName))
            {
                IsAnonymous = true;
                return true;
            }

            if (String.IsNullOrWhiteSpace(authToken))
                return false;

            loginWebClient.SetCookie("kname", userName, "cybergame.tv");
            loginWebClient.SetCookie("khash", userName, "cybergame.tv");

            var test = this.With(x => loginWebClient.Download("http://cybergame.tv"));

            if (test != null && test.Contains("logout.php"))
                return true;

            Config.SetParameterValue("AuthToken", String.Empty);

            return false;
        }
        public bool LoginWithUsername()
        {
            var userName = Config.GetParameterValue("Username") as string;
            NickName = userName;
            var password = Config.GetParameterValue("Password") as string;

            if (String.IsNullOrWhiteSpace(userName) || String.IsNullOrWhiteSpace(password))
            {
                IsAnonymous = true;
                return true;
            }


            var authString = String.Format(@"action=login&username={0}&pass={1}&remember_me=1", userName, password);

            loginWebClient.ContentType = ContentType.UrlEncoded;

            var authToken = this.With(x => loginWebClient.Upload("http://cybergame.tv/login.php", authString))
                            .With(x => Re.GetSubString(x, @"khash[^""]+""([^""]+)"""));

            if (authToken == null)
            {
                Log.WriteError("Login to cybergame.tv failed. Joining anonymously");
                IsAnonymous = true;
                return false;
            }
            else
            {
                IsAnonymous = false;
                Config.SetParameterValue("AuthToken", authToken);
                Config.SetParameterValue("AuthTokenCredentials", userName + password);

                return true;
            }
        }
        public override void DownloadEmoticons(string url)
        {
            if (IsFallbackEmoticons && IsWebEmoticons)
                return;

            lock (iconParseLock)
            {
                var list = new List<Emoticon>();
                if (Emoticons == null)
                    Emoticons = new List<Emoticon>();

                var emoticonsMatches = this.With(x => loginWebClient.Download(url))
                    .With(x => Regex.Matches(x, @"""(.*?)"":""(smiles/.*?)"""));

                if (emoticonsMatches == null)
                    return;

                if (emoticonsMatches.Count <= 0)
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
                        if (IsFallbackEmoticons)
                            IsWebEmoticons = true;

                        IsFallbackEmoticons = true;
                    }
                }
            }
        }
        
        #endregion
    }
    public class CybergameChannel : ChatChannelBase
    {
        private WebSocketBase webSocket;
        private object pollerLock = new object();
        private WebPoller statsPoller;
        private Random random = new Random();
        private Dictionary<string, Action<CybergameChannel, CybergameData>>
            packetHandlers = new Dictionary<string, Action<CybergameChannel, CybergameData>>() {
                        {"changeWindow", SuccessfulConnect},
                        {"setUI", SuccessfulLogin},
                        {"chatMessage", ChatMessageReceive},
                        {"listUsers", ListUsers},
            };
        public CybergameChannel(IChat chat)
        {
            Chat = chat;
        }       
        private static void SuccessfulConnect(CybergameChannel channel, CybergameData data)
        {
            channel.Chat.Status.IsConnected = true;
            if (channel.JoinCallback != null)
                channel.JoinCallback(channel);
        }

        private static void SuccessfulLogin(CybergameChannel channel, CybergameData data)
        {
            channel.Chat.Status.IsLoggedIn = true;
        }

        private static void ChatMessageReceive(CybergameChannel channel, CybergameData data)
        {
            if (String.IsNullOrWhiteSpace(data.From) || String.IsNullOrWhiteSpace(data.Text))
                return;

            channel.ChannelStats.MessagesCount++;
            channel.Chat.UpdateStats();

            if (channel.ReadMessage != null)
                channel.ReadMessage(new ChatMessage()
                {
                    Channel = channel.ChannelName,
                    ChatIconURL = channel.Chat.IconURL,
                    ChatName = channel.Chat.ChatName,
                    FromUserName = data.From,
                    HighlyImportant = false,
                    IsSentByMe = false,
                    Text = HttpUtility.HtmlDecode(data.Text),
                });
        }

        private static void ListUsers(CybergameChannel channel, CybergameData data)
        {

        }

        private void ReadRawMessage(string rawMessage)
        {
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
            var token = Chat.Config.GetParameterValue("AuthToken").ToString();
            var authPacket = new CybergamePacket()
            {
                Command = "login",
                Message = new CybergameData()
                {
                    Login = Chat.NickName ?? "",
                    Password = token ?? "",
                    Channel = ChannelName,
                },
            };

            Log.WriteInfo("Cybergame sending {0}", authPacket.ToString());
            webSocket.Send(authPacket.ToString());
        }

        public override void Leave()
        {
            Log.WriteInfo("Cybergame leaving {0}", ChannelName);
            if( webSocket != null && !webSocket.IsClosed )
                webSocket.Disconnect();
        }
       
        public override void SendMessage(ChatMessage message)
        {
            if (Chat.IsAnonymous || String.IsNullOrWhiteSpace(message.Channel) ||
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

        public override void Join(Action<IChatChannel> callback, string channel)
        {
            ChannelName = "#" + channel.Replace("#", "");

            SetupStatsWatcher();

            if (String.IsNullOrWhiteSpace(channel))
                return;
       
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
            JoinCallback = callback;

            webSocket.ReceiveMessageHandler = ReadRawMessage;
            webSocket.Path = String.Format("/{0}/{1}/websocket", Rnd.RandomWebSocketServerNum(0x1e3), Rnd.RandomWebSocketString());
            webSocket.Port = "9090";
            webSocket.Host = "cybergame.tv";            
            webSocket.Connect();
        }

        public override void SetupStatsWatcher()
        {
            statsPoller = new WebPoller()
            {
                Id = ChannelName,
                Uri = new Uri(String.Format(@"http://api.cybergame.tv/p/statusv2/?channel={0}", ChannelName.Replace("#", ""))),
            };

            statsPoller.ReadString = (stream) =>
            {
                lock (pollerLock)
                {
                    if (stream == null)
                        return;

                    var channelInfo = JsonConvert.DeserializeObject<CybergameChannelStatus>(stream);
                    statsPoller.LastValue = channelInfo;
                    int viewers = 0;
                    if( channelInfo != null  && int.TryParse( channelInfo.spectators, out viewers))
                    {
                        ChannelStats.ViewersCount = viewers;
                        Chat.UpdateStats();
                    }
                }
            };
            statsPoller.Start();
        }
    }

    #region Json classes
    [DataContract]
    public class CybergamePacket
    {
        [DataMember(Name = "command")]
        public string Command { get; set; }
        [DataMember(Name = "message")]
        public CybergameData Message { get; set; }

        public override string ToString()
        {
            return @"[""" + @"{\""command\"":\""" + Command + @"\"",\""message\"":\""" + Message.ToString() + @"\""}""]";
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

    #endregion
}
