using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    //TODO: implement user list
    public class HitboxChat : ChatBase, IStreamTopic, IFollowersProvider, IChatUserList
    {
        private WebClientBase loginWebClient = new WebClientBase();
        private object iconParseLock = new object();
        private object lockSearch = new object();
        private WebPoller followerPoller = new WebPoller();
        private HitboxFollowers currentFollowers = new HitboxFollowers();

        public HitboxChat(ChatConfig config) : base(config)
        {
            EmoticonUrl = "https://www.hitbox.tv/api/chat/icons/UnknownSoldier";
            EmoticonFallbackUrl = @"Content\hitboxemoticons.json";
            
            NickName = "UnknownSoldier";

            CreateChannel = () => { return new HitboxChannel(this); };

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
            ChatUsers = new ObservableCollection<ChatUser>();
            Games = new ObservableCollection<Game>();
        }
        #region IChat implementation

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
                Log.WriteInfo("Hitbox authorization exception {0}", e.Message);
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
            var password = Config.GetParameterValue("Password") as string;
            var tokenCredentials = Config.GetParameterValue("AuthTokenCredentials") as string;

            if (tokenCredentials != userName + password)
                return false;

            if( String.IsNullOrEmpty(userName) || userName.Equals("unknownsoldier",StringComparison.InvariantCultureIgnoreCase))
            {
                IsAnonymous = true;
                return true;
            }

            if (String.IsNullOrWhiteSpace(authToken))
                return false;

            NickName = userName;

            var test = this.With(x => Json.DeserializeUrl<dynamic>(String.Format("https://www.hitbox.tv/api/teams/codex?authToken={0}",authToken)));
            
            if (test.teams != null)
            {
                IsAnonymous = false;
                PollFollowers();
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
                IsAnonymous = true;
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
                IsAnonymous = true;
                return false;
            }
            else
            {
                IsAnonymous = false;
                Config.SetParameterValue("AuthToken", authToken);
                Config.SetParameterValue("AuthTokenCredentials", userName + password);

                return LoginWithToken();
            }
        }
        private void SetCommonHeaders()
        {
            loginWebClient.Headers["Accept"] = @"application/json, text/plain, */*";
            loginWebClient.Headers["Content-Type"] = @"application/json;charset=UTF-8";
            loginWebClient.Headers["Accept-Encoding"] = "gzip,deflate,sdch";
        }
        public override void DownloadEmoticons(string url)
        {
            if (IsFallbackEmoticons && IsWebEmoticons )
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
                        if (IsFallbackEmoticons)
                            IsWebEmoticons = true;

                        IsFallbackEmoticons = true;
                    }
                }
            }
        }
        #endregion

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

        #region IFollowerProvider
        public Action<ChatUser> AddFollower
        {
            get;
            set;
        }

        public Action<ChatUser> RemoveFollower
        {
            get;
            set;
        }
        private void PollFollowers()
        {
            if (followerPoller != null)
                followerPoller.Stop();

            if (!Status.IsLoggedIn)
                return;

            var getUrl = @"https://www.hitbox.tv/api/followers/user/{0}?limit=50";
            var userName = Config.GetParameterValue("Username") as string;

            if (userName.Equals("unknownsoldier", StringComparison.InvariantCultureIgnoreCase))
                return;
            
            followerPoller.Id = "followersPoller";
            followerPoller.Interval = 10000;
            followerPoller.Uri = new Uri(String.Format(getUrl, HttpUtility.UrlEncode(userName.ToLower())));
            followerPoller.ReadStream = (stream) =>
            {
                if (stream == null)
                    return;

                using( stream )
                {
                    var followers = Json.DeserializeStream<HitboxFollowers>(stream);
                    if (followers != null && followers.followers != null)
                    {
                        if (currentFollowers.followers == null)
                        {
                            currentFollowers.followers = followers.followers.ToList();
                        }
                        else if (followers.followers.Count > 0)
                        {
                            var newFollowers = followers.followers.Take(25).Except(currentFollowers.followers, new LambdaComparer<HitboxFollower>((x, y) => x.user_name.Equals(y.user_name)));
                            foreach (var follower in newFollowers)
                            {
                                if (AddFollower != null)
                                    AddFollower(new ChatUser()
                                    {
                                        NickName = follower.user_name,
                                        ChatName = ChatName
                                    });
                            }

                            currentFollowers.followers = followers.followers.ToList();
                        }
                    }
                }


            };
            followerPoller.Start();
        }
        #endregion


        public ObservableCollection<ChatUser> ChatUsers
        {
            get;
            set;
        }
    }

    public class HitboxChannel : ChatChannelBase
    {
        private WebSocketBase webSocket;
        private Random random = new Random();
        private WebPoller statsPoller;
        private object pollerLock = new object();
        private object lockRawMessage = new object();
        private Timer timerEveryMinute;
        private object chatUsersLock = new object();
        private ObservableCollection<ChatUser> currentUserList = new ObservableCollection<ChatUser>();

        public HitboxChannel(HitboxChat chat)
        {
            Chat = chat;
            timerEveryMinute = new Timer((obj) => { 
                var channel = obj as HitboxChannel;
                if( channel == null )
                    return;

                SendUserListRequest(channel.ChannelName);
            }, this, Timeout.Infinite, Timeout.Infinite);            
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
        public override void Join(Action<IChatChannel> callback, string channel)
        {

            if( String.IsNullOrWhiteSpace(channel) )
                return;

            var authToken = Chat.Config.GetParameterValue("AuthToken") as string;

            ChannelName = "#" + channel.Replace("#", "");
            webSocket = new WebSocketBase();
            webSocket.PingInterval = 0;
            webSocket.Origin = "http://www.hitbox.tv";
            webSocket.ConnectHandler = () =>
            {
                SendCredentials(Chat.NickName, channel, authToken);

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
            SetupStatsWatcher();
        }
        private void SendUserListRequest(string channel)
        {
            var hitboxPacket = new HitboxWebSocketPacket()
            {
                name = "message",
                args = new List<HitboxArg>()
                {
                    new HitboxArg() {
                        method = "getChannelUserList",
                        @params = new {
                            channel = channel.Replace("#",""),
                        }
                    }
                }
            };
            var jsonString = JsonConvert.SerializeObject(hitboxPacket);
            webSocket.Send("5:::" + jsonString);
        }
        private void SendCredentials(string nickname, string channel, string authToken)
        {
            var loginData = new HitboxWebSocketPacket()
            {
                name = "message",
                args = new List<HitboxArg>()
                {
                    new HitboxArg() {
                        method = "joinChannel",
                        @params = new {
                            channel = channel.Replace("#",""),
                            name =  Chat.NickName,
                            token = Chat.IsAnonymous ? "" : authToken ?? "",
                            isAdmin = !Chat.IsAnonymous && channel.Replace("#","").Equals(nickname,StringComparison.InvariantCultureIgnoreCase) ? true :false,
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
            if( String.IsNullOrWhiteSpace( rawMessage ))
                return;

            lock( lockRawMessage )
            {
                const string jsonArgsRe = @".*args"":\[""(.*?)""\]}$";

                if (rawMessage.Equals("1::"))
                {
                    Chat.Status.IsConnected = true;  
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
                        timerEveryMinute.Change(500, 60000);

                        var role = (string)msg.role;
                        switch( role.ToLower() )
                        {
                            case "guest":
                                if (!Chat.IsAnonymous)
                                {
                                    Chat.Status.IsLoggedIn = false;
                                    if( !Chat.Status.IsLoginFailed )
                                    {
                                        Chat.Status.IsConnected = false;
                                        Chat.Status.IsLoggedIn = false;
                                        Chat.Status.IsLoginFailed = true;
                                        Chat.Status.IsStarting = false;
                                        Chat.Config.SetParameterValue("AuthToken", String.Empty);
                                        Chat.Restart();
                                    }
                                }
                                else
                                {
                                    Chat.Status.IsLoginFailed = false;
                                }

                                break;
                            case "admin":
                                {
                                    Chat.Status.IsLoggedIn = true;
                                    Chat.Status.IsLoginFailed = false;
                                }
                                break;
                            case "anon":
                                {
                                    Chat.Status.IsLoggedIn = true;
                                    Chat.Status.IsLoginFailed = false;
                                }
                                break;
                            default:
                               break;
                        }
                        var authToken = Chat.Config.GetParameterValue("AuthToken") as string;
                        SendCredentials(Chat.NickName, ChannelName, authToken);
                    }
                    else if (rawMessage.Contains(@":\""chatMsg"))
                    {
                        var nickName = (string)msg.name;
                        var text = (string)msg.text;

                        if (String.IsNullOrWhiteSpace(nickName) || String.IsNullOrWhiteSpace(text))
                            return;

                        if (ReadMessage != null)
                            ReadMessage(new ChatMessage()
                            {
                                Channel = ChannelName,
                                ChatIconURL = Chat.IconURL,
                                ChatName = Chat.ChatName,
                                FromUserName = nickName,
                                HighlyImportant = false,
                                IsSentByMe = false,
                                Text = text
                            });
                    }
                    else if (rawMessage.Contains(@":\""userList"))
                    {
                        var data = msg.data;
                        var guestsNumber = this.With(x => data.Guests as JArray).With(x => x.ToObject<string[]>());
                        var admins = this.With(x => data.admin as JArray).With(x => x.ToObject<string[]>());
                        var moderators = this.With(x => data.user as JArray).With(x => x.ToObject<string[]>());
                        var users = this.With(x => data.anon as JArray).With(x => x.ToObject<string[]>());
                        var followers = this.With(x => data.isFollower as JArray).With(x => x.ToObject<string[]>());
                        var subscribers = this.With(x => data.isSubscriber as JArray).With(x => x.ToObject<string[]>());
                        var staff = this.With(x => data.isStaff as JArray).With(x => x.ToObject<string[]>());

                        currentUserList.Clear();
                        foreach( var pair in new Dictionary<string, string[]> { 
                            {"Staff", staff}, 
                            {"Admins",admins}, 
                            {"Moderators", moderators}, 
                            {"Subscribers", subscribers}, 
                            {"Followers", followers},
                            {"Users", users}} )
                        {
                            if (pair.Value == null)
                                continue;

                            foreach( string userNickname in pair.Value )
                            {
                                currentUserList.Add(new ChatUser()
                                {
                                    Channel = ChannelName,
                                    ChatName = Chat.ChatName,
                                    GroupName = pair.Key,
                                    NickName = userNickname,
                                    Badges = null,
                                });
                            }
                        }
                        var oldUserList = (Chat as IChatUserList).ChatUsers;
                    
                        //Delete disconnected users
                        UI.Dispatch(() => {
                            oldUserList.Where(item => item.Channel.Equals(ChannelName) && item.ChatName.Equals(Chat.ChatName))
                                .Except(currentUserList, new LambdaComparer<ChatUser>((x, y) => x.NickName.Equals(y.NickName)))                                
                                .ToList()
                                .ForEach(item => oldUserList.Remove(item));
                        });
                        var newUserList = currentUserList
                            .Where(item => item.Channel.Equals(ChannelName) && item.ChatName.Equals(Chat.ChatName))
                            .Except(oldUserList, new LambdaComparer<ChatUser>((x, y) => x.NickName.Equals(y.NickName)))
                            .ToList();

                        foreach( ChatUser user in newUserList )
                        {
                            UI.Dispatch(() =>
                            {
                                lock (chatUsersLock)
                                    (Chat as IChatUserList).ChatUsers.Add(new ChatUser()
                                    {
                                        Channel = ChannelName,
                                        ChatName = Chat.ChatName,
                                        GroupName = user.GroupName,
                                        NickName = user.NickName,
                                        Badges = null,
                                    });
                            });
                        }
                        newUserList = null;

                    }

                }
            }
            
        }
        
        public override void Leave()
        {
            Log.WriteInfo("Hitbox leaving {0}", ChannelName);
            if( webSocket != null && !webSocket.IsClosed )
                webSocket.Disconnect();

            if( timerEveryMinute != null)
                timerEveryMinute.Change(Timeout.Infinite, Timeout.Infinite);

            if( statsPoller != null)
                statsPoller.Stop();
        }
        
        public override void SendMessage( ChatMessage message )
        {
            if (Chat.IsAnonymous || String.IsNullOrWhiteSpace(message.Channel) ||
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

        public override void SetupStatsWatcher()
        {
            statsPoller = new WebPoller()
            {
                Id = ChannelName,
                Uri = new Uri(String.Format(@"http://api.hitbox.tv/media/live/{0}", ChannelName.Replace("#", ""))),
            };

            statsPoller.ReadStream = (stream) =>
            {
                if (stream == null)
                    return;

                lock (pollerLock)
                {
                    using (stream)
                    {
                        var channelInfo = this.With(x => stream)
                            .With(x => Json.DeserializeStream<HitboxChannelStats>(stream))
                            .With(x => x.livestream)
                            .With(x => x.FirstOrDefault(livestream => livestream.media_name.Equals(ChannelName.Replace("#", ""), StringComparison.InvariantCultureIgnoreCase)));

                        statsPoller.LastValue = channelInfo;
                        if (channelInfo != null)
                        {
                            ChannelStats.ViewersCount = channelInfo.media_views;
                            Chat.UpdateStats();
                        }
                    }
                }
            };
            statsPoller.Start();
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

    public class HitboxFollower
    {
        public string followers { get; set; }
        public string user_name { get; set; }
        public string user_id { get; set; }
        public string user_logo { get; set; }
        public string user_logo_small { get; set; }
        public string follow_id { get; set; }
        public string follower_user_id { get; set; }
        public string follower_notify { get; set; }
        public string date_added { get; set; }
    }

    public class HitboxFollowers
    {
        public dynamic request { get; set; }
        public List<HitboxFollower> followers { get; set; }
        public string max_results { get; set; }
    }

    public class HitboxUserListParams
    {
        public string channel { get; set; }
    }


    #endregion
}
