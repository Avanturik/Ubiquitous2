using System;
using UB.Utils;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;
using System.Web;
using dotIRC;
using System.Threading;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.Runtime.Serialization;


namespace UB.Model
{
    public class TwitchChat : ChatBase, IStreamTopic, IFollowersProvider, IChatUserList
    {
        private object lockSearch = new object();
        private object iconParseLock = new object();
        private WebClientBase webClient = new WebClientBase();
        private static List<Emoticon> sharedEmoticons = new List<Emoticon>();
        private TwitchFollowers currentFollowers = new TwitchFollowers();
        private WebPoller followerPoller = new WebPoller();
        private Random random = new Random();

        public TwitchChat(ChatConfig config)
            : base(config)
        {
            EmoticonUrl = "http://api.twitch.tv/kraken/chat/emoticons";
            EmoticonFallbackUrl = @"Content\twitchemoticons.json";

            ReceiveOwnMessages = true;

            NickName = "justinfan" + random.Next(1000000, 9999999).ToString();

            CreateChannel = () => { return new TwitchChannel(this); };

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

            ChatUsers = new ObservableCollection<ChatUser>();
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
                Log.WriteInfo("Twitch authorization exception {0}", e.Message);
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
            var authToken = Config.GetParameterValue("OAuthToken") as string;
            var userName = Config.GetParameterValue("Username") as string;
            var password = Config.GetParameterValue("Password") as string;
            var tokenCredentials = Config.GetParameterValue("AuthTokenCredentials") as string;

            if (tokenCredentials != userName + password)
                return false;

            if (String.IsNullOrEmpty(userName) || Regex.IsMatch(userName, @"justinfan\d+", RegexOptions.IgnoreCase))
            {
                IsAnonymous = true;
                return true;
            }

            if (String.IsNullOrWhiteSpace(authToken))
                return false;

            NickName = userName;

            var test = this.With(x => Json.DeserializeUrl<dynamic>(String.Format("https://api.twitch.tv/kraken?oauth_token={0}", authToken)));

            if (test.token != null && (bool)test.token.valid )
            {
                IsAnonymous = false;
                PollFollowers();
                return true;
            }
            Config.SetParameterValue("OAuthToken",  null);
            Config.SetParameterValue("ApiToken", null);
            Config.SetParameterValue("AuthTokenCredentials", null);


            return false;
        }
        public bool LoginWithUsername()
        {
            var userName = Config.GetParameterValue("Username") as string;
            var password = Config.GetParameterValue("Password") as string;

            if (String.IsNullOrWhiteSpace(userName) || String.IsNullOrWhiteSpace(password) || Regex.IsMatch(userName, @"justinfan\d+", RegexOptions.IgnoreCase))
            {
                IsAnonymous = true;
                return true;
            }

            NickName = userName;
         
            webClient.SetCookie("api_token", null, "twitch.tv");
            webClient.SetCookie("csrf_token", null, "twitch.tv");

            var csrfToken = this.With(x => webClient.Download("http://www.twitch.tv/login"))
                    .With(x => Re.GetSubString(x, @"^.*authenticity_token.*?value=""(.*?)"""));
            
            
            if (csrfToken == null)
            {
                Log.WriteError("Twitch: Can't get CSRF token. Twitch web layout changed ?");
                return false;
            }
            string csrf_cookie = csrfToken;
            if (csrf_cookie.Substring(csrf_cookie.Length - 1).Equals("="))
                csrf_cookie = csrf_cookie.Substring(0, csrf_cookie.Length - 1) + "%3D";

            webClient.SetCookie("csrf_token", csrf_cookie, "twitch.tv");
            webClient.ContentType = ContentType.UrlEncoded;
            webClient.Headers["X-Requested-With"] = "XMLHttpRequest";
            webClient.Headers["X-CSRF-Token"] = csrfToken;
            webClient.Headers["Accept"] = "text/html, application/xhtml+xml, */*";

            var apiToken = this.With(x => webClient.Upload("https://secure.twitch.tv/user/login", String.Format(
                    "utf8=%E2%9C%93&authenticity_token={0}%3D&redirect_on_login=&embed_form=false&user%5Blogin%5D={1}&user%5Bpassword%5D={2}",
                    csrfToken,
                    userName,
                    password)))
                .With(x => webClient.CookieValue("api_token", "http://twitch.tv"));

            if (String.IsNullOrWhiteSpace(apiToken))
            {
                Log.WriteError("Twitch: Can't get API token");
                return false;
            }
            webClient.Headers["Twitch-Api-Token"] = apiToken;
            webClient.Headers["X-CSRF-Token"] = csrfToken;
            webClient.Headers["Accept"] = "*/*";

            if (apiToken == null)
            {
                Log.WriteError("Login to twitch.tv failed. Joining anonymously");
                IsAnonymous = true;
                return false;
            }
            else
            {
                var oauthToken = this.With(x => webClient.Download("http://api.twitch.tv/api/me?on_site=1"))
                                .With(x => JToken.Parse(x))
                                .With(x => x.Value<string>("chat_oauth_token"));

                if( String.IsNullOrWhiteSpace( oauthToken ))
                {
                    Log.WriteError("Login to twitch.tv failed. Joining anonymously");
                    IsAnonymous = true;
                    return false;
                }

                IsAnonymous = false;
                Config.SetParameterValue("OAuthToken", oauthToken);
                Config.SetParameterValue("ApiToken", apiToken);
                Config.SetParameterValue("AuthTokenCredentials", userName + password);

                return LoginWithToken();
            }        
        }

        public override bool SendMessage(ChatMessage message)
        {        
            message.UserBadges = new List<UserBadge>() {
                new UserBadge() { Title = "broadcaster", Url = "http://chat-badges.s3.amazonaws.com/broadcaster.png"}
            };

            return base.SendMessage(message);
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

                var jsonEmoticons = this.With(x => Json.DeserializeUrl<TwitchJsonEmoticons>(url))
                    .With(x => x.emoticons);

                if (jsonEmoticons == null)
                {
                    Log.WriteError("Unable to get Twitch.tv emoticons!");
                }
                else
                {
                    foreach (TwitchJsonEmoticon icon in jsonEmoticons)
                    {
                        if (icon != null && icon.images != null && !String.IsNullOrWhiteSpace(icon.regex))
                        {
                            var image = icon.images.With(x => icon.images).With(x => x.First());
                            if (image != null && !String.IsNullOrWhiteSpace(image.url))
                            {
                                list.Add(new Emoticon(icon.regex.Replace(@"\&gt\;", ">").Replace(@"\&lt\;", "<").Replace(@"\&amp\;", "&"),
                                                        image.url,
                                                        image.width,
                                                        image.height));
                            }
                        }
                    }
                    if (list.Count > 0)
                    {

                        sharedEmoticons = list.ToList();
                        Emoticons = sharedEmoticons;
                        if (IsFallbackEmoticons)
                            IsWebEmoticons = true;

                        IsFallbackEmoticons = true;
                    }
                }
            }
        }

        private string TwitchPost(string url, string parameters)
        {
            var csrfToken = webClient.CookieValue("csrf_token", "http://twitch.tv");

            if (String.IsNullOrWhiteSpace(csrfToken) &&  !LoginWithUsername())
                return null;

            //webClient.Headers["X-CSRF-Token"] = webClient.CookieValue("csrf_token", "http://twitch.tv");
            webClient.ContentType = ContentType.UrlEncodedUTF8;
            return webClient.Upload(url, parameters);
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
        public void QueryGameList(string gameName, Action callback)
        {
            lock (lockSearch)
            {
                Log.WriteInfo("Searching twitch game {0}", gameName);
                Games.Clear();
                Games.Add(new Game() { Name = "Loading..." });

                if (callback != null)
                    UI.Dispatch(() => callback());

                var jsonGames = this.With(x => webClient.Download(String.Format("http://www.twitch.tv/discovery/search?term={0}", HttpUtility.UrlEncode(gameName))))
                    .With(x => JToken.Parse(x))
                    .With(x => x.ToArray<dynamic>());

                if (jsonGames == null)
                {
                    Log.WriteInfo("Twitch search returned empty result", gameName);
                    return;
                }

                Games.Clear();
                foreach (var obj in jsonGames)
                {
                    Games.Add(new Game()
                    {
                        Id = obj.id,
                        Name = obj.name,
                    });
                }
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
                webClient.ContentType = ContentType.UrlEncodedUTF8;
                var json = this.With(x => webClient.Download(String.Format("http://api.twitch.tv/api/channels/{0}/ember?on_site=1", 
                    HttpUtility.UrlEncode(NickName))))
                    .With(x => !String.IsNullOrWhiteSpace(x) ? JToken.Parse(x) : null);

                if (json == null || Info == null)
                    return;

                Info.Topic = json["status"].ToObject<string>();
                Info.CurrentGame.Name = json["game"].ToObject<string>();
                Info.Language = json["broadcaster_language"].ToObject<string>();
                if (String.IsNullOrWhiteSpace(Info.Language))
                    Info.Language = "other";

                Info.CanBeRead = true;

                if (StreamTopicAcquired != null)
                    UI.Dispatch(() => StreamTopicAcquired());
            });
        }
        public void SetTopic()
        {

            if (!Status.IsLoggedIn)
                return;

            Task.Factory.StartNew(() =>
            {
                var url = String.Format(@"http://www.twitch.tv/{0}/update", NickName);
                var parameters = String.Format(@"status={0}&game={1}&broadcaster_language={2}", HttpUtility.UrlEncode(Info.Topic), HttpUtility.UrlEncode(Info.CurrentGame.Name), HttpUtility.UrlEncode(Info.Language));
                var result = TwitchPost(url, parameters);

                // Did you just login on Web and your session cookie is invalid now ? Okay, let's authenticate again...
                if (!result.Contains(Info.Topic))
                {
                    if (LoginWithUsername())
                        result = TwitchPost(url, parameters);
                    else
                        Log.WriteError("unable to set topic for Twitch.tv");
                }
            });
        }
        public string SearchQuery
        {
            get;
            set;
        }
        public Action StreamTopicAcquired
        {
            get;
            set;
        }
        #endregion IStreamTopic

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

            if (IsAnonymous)
                return;

            followerPoller.Id = "followersPoller";
            followerPoller.Interval = 10000;
            followerPoller.Uri = new Uri(String.Format(@"http://api.twitch.tv/kraken/channels/{0}/follows?limit=50&offset=0&on_site=1", 
                NickName));

            followerPoller.ReadStream = (stream) =>
            {
                if (stream == null)
                    return;

                using (stream)
                {
                    var followers = Json.DeserializeStream<TwitchFollowers>(stream);
                    if (followers != null && followers.follows != null)
                    {
                        if (currentFollowers.follows == null)
                        {
                            currentFollowers.follows = followers.follows.ToList();
                        }
                        else if (followers.follows.Count > 0)
                        {
                            var newFollowers = followers.follows.Take(25).Except(currentFollowers.follows, new LambdaComparer<TwitchFollow>((x, y) => x.user.display_name.Equals(y.user.display_name)));
                            foreach (var follower in newFollowers)
                            {
                                Log.WriteInfo("New Twitch follower: {0}", follower.user.display_name);
                                if (AddFollower != null)
                                    AddFollower(new ChatUser()
                                    {
                                        NickName = follower.user.display_name,
                                        ChatName = ChatName
                                    });
                            }

                            currentFollowers.follows = followers.follows.ToList();
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

    public class TwitchChannel : ChatChannelBase
    {
        private IrcClient ircClient = new IrcClient();
        private object chatUsersLock = new object();
        private Random random = new Random();
        private Timer pingTimer, disconnectTimer;
        private const int pingInterval = 30000;
        private Dictionary<string, List<UserBadge>> userBadges = new Dictionary<string, List<UserBadge>>();
        private NameValueCollection channelBadges = new NameValueCollection();
        private Dictionary<string, Action<TwitchChannel, IrcRawMessageEventArgs>>
                packetHandlers = new Dictionary<string, Action<TwitchChannel, IrcRawMessageEventArgs>>() {
                            {"001", ConnectHandler},
                            {"353", UserListHandler},
                            {"MODE", ModeHandler},
                            {"PRIVMSG", PrivateMessageHandler},
                            {"JOIN", JoinHandler},
                            {"PART", LeaveHandler},
                            {"NOTICE", NoticeHandler},
                        };
        private WebPoller statsPoller;
        private object pollerLock = new object();

        public TwitchChannel(TwitchChat chat)
        {
            Chat = chat;            
        }
        public override void Join(Action<IChatChannel> callback, string channel)
        {
            var safeConnectDelay = Chat.ChatChannels.Count * 100;
            Thread.Sleep(safeConnectDelay);
            ChannelName = "#" + channel.Replace("#", "");

            pingTimer = new Timer((sender) => {
                TryIrc( () => ircClient.Ping() );
            }, this, Timeout.Infinite, Timeout.Infinite);

            disconnectTimer = new Timer((sender) => {
                TryIrc( () => Leave());
            }, this, Timeout.Infinite, Timeout.Infinite);

            SetupStatsWatcher();

            using (WebClientBase webClient = new WebClientBase())
            {
                var badgesJson = this.With(x => webClient.Download(String.Format(@"https://api.twitch.tv/kraken/chat/{0}/badges", ChannelName.Replace("#", ""))))
                    .With(x => JsonConvert.DeserializeObject<TwitchBadges>(x));

                if (badgesJson == null)
                    return;

                channelBadges["admin"] = this.With(x => badgesJson.admin).With(x => x.image);
                channelBadges["broadcaster"] = this.With(x => badgesJson.broadcaster).With(x => x.image);
                channelBadges["mod"] = this.With(x => badgesJson.mod).With(x => x.image);
                channelBadges["staff"] = this.With(x => badgesJson.staff).With(x => x.image);
                channelBadges["turbo"] = this.With(x => badgesJson.turbo).With(x => x.image);
                channelBadges["subscriber"] = this.With(x => badgesJson.subscriber).With(x => x.image);
            }
            SetUserBadge(ChannelName.ToLower().Replace("#",""), "broadcaster");

            JoinCallback = callback;
            var nickname = Chat.IsAnonymous ? "justinfan" + random.Next(100000, 999999) : Chat.NickName;
            var registrationInfo = new IrcUserRegistrationInfo() {
                UserName = nickname,
                NickName = nickname,
                RealName = nickname,
                Password = Chat.IsAnonymous ? "blah" : "oauth:" + Chat.Config.GetParameterValue("OAuthToken") as string,
            };

            var host = Chat.Config.GetParameterValue("Host") as string;
            var portText = Chat.Config.GetParameterValue("Port") as string;
            int port = 6667;

            int.TryParse(portText, out port);
            
            TryIrc( () => ircClient.Initialize());
            ircClient.Disconnected += ircClient_Disconnected;
            ircClient.RawMessageReceived += ircClient_RawMessageReceived;
          
            if (Regex.IsMatch(host, @"\d+\.\d+\.\d+\.\d+"))
            {
                TryIrc( () => ircClient.Connect (host, port, false, registrationInfo ));
            }
            else
            {
                Utils.Net.TestTCPPort(host, port, (hostList, error) =>
                {
                    if (hostList == null || hostList.AddressList.Count() <= 0)
                    {
                        Log.WriteError("All servers are down. Domain:" + host);
                        return;
                    }
                    TryIrc( () => ircClient.Connect(hostList.AddressList[random.Next(0, hostList.AddressList.Count())], port, false, registrationInfo));
                });
            }
        }

        void ircClient_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
        {
            var command = e.Message.Command;
            if (packetHandlers.ContainsKey(command))
                packetHandlers[command](this, e);
        }

        void ircClient_Disconnected(object sender, EventArgs e)
        {
            Log.WriteInfo("Twitch disconnected from {0}", ChannelName);
            Leave();
        }
        public override void Leave()
        {
            if( ircClient != null)
            {
                ircClient.Disconnected -= ircClient_Disconnected;
                ircClient.RawMessageReceived -= ircClient_RawMessageReceived;
            }

            if( disconnectTimer != null)
                disconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
            
            if( pingTimer != null)
                pingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            
            if( statsPoller != null )
                statsPoller.Stop();

            Log.WriteInfo("Twitch leaving {0}", ChannelName);

            TryIrc(() => ircClient.Quit("bye!"));
            TryIrc(() => ircClient.Dispose());

            if (LeaveCallback != null)
                LeaveCallback(this);
        }
        public override void SendMessage( ChatMessage message )
        {
            TryIrc( () => ircClient.LocalUser.SendMessage( ChannelName, message.Text));
        }
        public override void SetupStatsWatcher()
        {
            statsPoller = new WebPoller()
            {
                Id = ChannelName,
                Uri = new Uri(String.Format(@"http://api.twitch.tv/kraken/streams/{0}?on_site=1", ChannelName.Replace("#", ""))),
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
                            .With(x => Json.DeserializeStream<dynamic>(stream));


                        statsPoller.LastValue = channelInfo;
                        if (channelInfo != null && channelInfo.stream != null )
                        {
                            ChannelStats.ViewersCount = channelInfo.stream.viewers;
                            Chat.UpdateStats();
                        }
                    }
                }
            };
            statsPoller.Start();
        }
        private void SetUserBadge(string userName, string userType)
        {
            if (!userBadges.ContainsKey(userName))
                userBadges.Add(userName, new List<UserBadge>());
            
            if (userBadges[userName].Any(x => x.Title.Equals(userType)))
                return;

            userBadges[userName].Add( 
                new UserBadge() {
                    Url = channelBadges[userType.ToLower()],
                    Title = userType
                });
            Log.WriteInfo("Special user:{0} type:{1}", userName, userType);
        }
        private static void PrivateMessageHandler( TwitchChannel channel, IrcRawMessageEventArgs args )
        {
            var parameters = args.Message.Parameters;
            if (parameters.Count < 2)
                return;

            if( parameters[1].StartsWith("specialuser", StringComparison.InvariantCultureIgnoreCase))
            {
                var userBadgeParams = parameters[1].Split(' ');
                if( userBadgeParams.Length == 3 )
                    channel.SetUserBadge(userBadgeParams[1], userBadgeParams[2]);
            }

            if (!parameters[0].Equals(channel.ChannelName, StringComparison.InvariantCultureIgnoreCase))
                return;

            channel.ChannelStats.MessagesCount++;
            channel.Chat.UpdateStats();

            if (channel.ReadMessage != null)
                channel.ReadMessage(new ChatMessage()
                {
                    Channel = channel.ChannelName,
                    ChatIconURL = channel.Chat.IconURL,
                    ChatName = channel.Chat.ChatName,
                    FromUserName = args.Message.Source.Name,
                    HighlyImportant = false,
                    IsSentByMe = false,
                    Text = parameters[1],
                    UserBadges = channel.userBadges.ContainsKey(args.Message.Source.Name) ? channel.userBadges[args.Message.Source.Name] : null
                });
        }
        private static void ConnectHandler(TwitchChannel channel, IrcRawMessageEventArgs args)
        {
            if( channel.ircClient.Channels.Count <= 0 )
            {
                channel.Chat.Status.IsConnected = true;
                channel.Chat.Status.IsConnecting = false;
                channel.TryIrc(() => channel.ircClient.Channels.Join(channel.ChannelName));
                channel.TryIrc(() => channel.ircClient.SendRawMessage("TWITCHCLIENT 1"));
            }
            if( channel.Chat.IsAnonymous )
            {
                channel.pingTimer.Change(pingInterval, pingInterval);

                if (channel.JoinCallback != null)
                    channel.JoinCallback(channel);
            }
        }
        private static void ModeHandler(TwitchChannel channel, IrcRawMessageEventArgs args)
        {
            var parameters = args.Message.Parameters;
            if (parameters.Count < 3)
                return;
            if (!parameters[1].Equals("+o"))
                return;

            var username = parameters[2];
            if( !username.Equals( channel.Chat.NickName, StringComparison.InvariantCultureIgnoreCase))
                channel.SetUserBadge(parameters[2], "mod");
        }
        private static void UserListHandler(TwitchChannel channel, IrcRawMessageEventArgs args)
        {
            var parameters = args.Message.Parameters;
            if (parameters.Count < 3)
                return;
            
            string userGroup = "Users";

            var users = parameters[3].Split(' ');
            if (users.Count() <= 0)
                return;

            foreach( var userNickname in users)
            {

                lock (channel.chatUsersLock)
                {
                    UI.Dispatch(() =>
                    {
                        lock (channel.chatUsersLock)
                            if (!(channel.Chat as IChatUserList).ChatUsers.Any(u => u != null && u.NickName.Equals(userNickname)))
                                (channel.Chat as IChatUserList).ChatUsers.Add(new ChatUser()
                                {
                                    Channel = channel.ChannelName,
                                    ChatName = channel.Chat.ChatName,
                                    GroupName = userGroup,
                                    NickName = userNickname,
                                    Badges = null,
                                });
                    });
                }
            }
        }
        private static void JoinHandler(TwitchChannel channel, IrcRawMessageEventArgs args)
        {
            string userNickname = args.Message.Source.Name;
            lock (channel.chatUsersLock)
            {

                if( !(channel.Chat as IChatUserList).ChatUsers.ToList().Any(u => u.NickName.Equals(userNickname)))
                {
                    List<UserBadge> badges;
                    string userGroup = "Users";
                    channel.userBadges.TryGetValue(userNickname, out badges);

                        UI.Dispatch(() =>
                        {
                            lock (channel.chatUsersLock)
                                if (!(channel.Chat as IChatUserList).ChatUsers.Any(u => u != null && u.NickName.Equals(userNickname)))
                                    (channel.Chat as IChatUserList).ChatUsers.Add(new ChatUser()
                                    {
                                        Channel = channel.ChannelName,
                                        ChatName = channel.Chat.ChatName,
                                        GroupName = userGroup,
                                        NickName = userNickname,
                                        Badges = badges,
                                    });
                        });

                }
            }
            Log.WriteInfo("Twitch user joined: {0}", userNickname);
            if (userNickname.Equals(channel.Chat.NickName, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!channel.Chat.IsAnonymous)
                    channel.Chat.Status.IsLoggedIn = true;

                channel.pingTimer.Change(pingInterval, pingInterval);

                if (channel.JoinCallback != null)
                    channel.JoinCallback(channel);
            }
        }
        private static void LeaveHandler(TwitchChannel channel, IrcRawMessageEventArgs args)
        {            
            string userNickname = args.Message.Source.Name;
            UI.Dispatch(() => {
                (channel.Chat as IChatUserList).ChatUsers
                    .RemoveAll(user => user.NickName.Equals(userNickname, StringComparison.InvariantCultureIgnoreCase));            
            });
            Log.WriteInfo("Twitch user left: {0}", args.Message.Source);
        }
        private static void NoticeHandler( TwitchChannel channel, IrcRawMessageEventArgs args )
        {

            Log.WriteInfo("Twitch notice: {0}", args.RawContent);
            if (args.RawContent.Contains("Login unsuccessful"))
            {
                Thread.Sleep(2000);
                channel.Leave();
                //channel.Join((ch) => { channel.JoinCallback(ch); }, channel.ChannelName);
            }
        }

        private void TryIrc( Action action )
        {
            try
            {

                if (action != null)
                    action();
            }
            catch( Exception e )
            {
                Log.WriteError("Twitch IRC exception {0}", e.Message);
            }
        }
    }

    #region Twitch badges json
    public class TwitchBadgeAdmin
    {
        [DataMember(IsRequired = false)]
        public string alpha { get; set; }
        [DataMember(IsRequired = false)]
        public string image { get; set; }
        [DataMember(IsRequired = false)]
        public string svg { get; set; }
    }

    public class TwitchBadgeBroadcaster
    {
        [DataMember(IsRequired = false)]
        public string alpha { get; set; }
        [DataMember(IsRequired = false)]
        public string image { get; set; }
        [DataMember(IsRequired = false)]
        public string svg { get; set; }
    }

    public class TwitchBadgeMod
    {
        [DataMember(IsRequired = false)]
        public string alpha { get; set; }
        [DataMember(IsRequired = false)]
        public string image { get; set; }
        [DataMember(IsRequired = false)]
        public string svg { get; set; }
    }

    public class TwitchBadgeStaff
    {
        [DataMember(IsRequired = false)]
        public string alpha { get; set; }
        [DataMember(IsRequired = false)]
        public string image { get; set; }
        [DataMember(IsRequired = false)]
        public string svg { get; set; }
    }

    public class TwitchBadgeTurbo
    {
        [DataMember(IsRequired = false)]
        public string alpha { get; set; }
        [DataMember(IsRequired = false)]
        public string image { get; set; }
        [DataMember(IsRequired = false)]
        public string svg { get; set; }
    }

    public class TwitcBadgeSubscriber
    {
        [DataMember(IsRequired = false)]
        public string image { get; set; }
    }

    public class TwitchBadges
    {
        public TwitchBadgeAdmin admin { get; set; }
        public TwitchBadgeBroadcaster broadcaster { get; set; }
        public TwitchBadgeMod mod { get; set; }
        public TwitchBadgeStaff staff { get; set; }
        public TwitchBadgeTurbo turbo { get; set; }
        public TwitcBadgeSubscriber subscriber { get; set; }
        public TwitchLinks2 _links { get; set; }
    }
    #endregion

    #region Twich emoticon json
    class TwitchJsonEmoticons
    {
        public object _links { get; set; }
        public TwitchJsonEmoticon[] emoticons { get; set; }
    }
    class TwitchJsonEmoticon
    {
        public string regex { get; set; }
        public TwitchJsonImage[] images { get; set; }
    }

    class TwitchJsonImage
    {
        public int width { get; set; }
        public int height { get; set; }
        public string url { get; set; }
        public string emoticon_set { get; set; }
    }
    #endregion

    #region Twitch channel status json

    public class TwitchLinks
    {
        public string self { get; set; }
        public string channel { get; set; }
    }

    public class TwitchLinks2
    {
        public string self { get; set; }
    }

    public class TwitchPreview
    {
        public string small { get; set; }
        public string medium { get; set; }
        public string large { get; set; }
        public string template { get; set; }
    }

    public class TwitchLinks3
    {
        public string self { get; set; }
        public string follows { get; set; }
        public string commercial { get; set; }
        public string stream_key { get; set; }
        public string chat { get; set; }
        public string features { get; set; }
        public string subscriptions { get; set; }
        public string editors { get; set; }
        public string videos { get; set; }
        public string teams { get; set; }
    }

    public class TwitchChannelDescription
    {
        public TwitchLinks3 _links { get; set; }
        public object background { get; set; }
        public object banner { get; set; }
        public string display_name { get; set; }
        public string game { get; set; }
        public string logo { get; set; }
        public bool mature { get; set; }
        public string status { get; set; }
        public string url { get; set; }
        public string video_banner { get; set; }
        public int _id { get; set; }
        public string name { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public object abuse_reported { get; set; }
        public int delay { get; set; }
        public int followers { get; set; }
        public string profile_banner { get; set; }
        public string profile_banner_background_color { get; set; }
        public int views { get; set; }
        public string language { get; set; }
    }

    public class TwitchStream
    {
        public long _id { get; set; }
        public string game { get; set; }
        public int viewers { get; set; }
        public string created_at { get; set; }
        public TwitchLinks2 _links { get; set; }
        public TwitchPreview preview { get; set; }
        public TwitchChannelDescription channel { get; set; }
    }

    public class TwitchChannelInfo
    {
        public TwitchLinks _links { get; set; }
        public TwitchStream stream { get; set; }
    }
    public class TwitchFollow
    {
        public string created_at { get; set; }
        public dynamic _links { get; set; }
        public TwitchUser user { get; set; }
    }
    public class TwitchFollowers
    {
        public List<TwitchFollow> follows { get; set; }
        public int _total { get; set; }
        public dynamic _links { get; set; }
    }
    public class TwitchUser
    {
        public int _id { get; set; }
        public string name { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public dynamic _links { get; set; }
        public string display_name { get; set; }
        public string logo { get; set; }
        public string bio { get; set; }
        public string type { get; set; }
    }

    #endregion
}
