using System;
using UB.Model.IRC;
using UB.Utils;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;
using System.Web;

namespace UB.Model
{
    public class TwitchChat : IRCChatBase, IStreamTopic
    {
        private const string ircDomain = "irc.twitch.tv";
        private const int ircPort = 6667;
        private const string emoticonUrl = "http://api.twitch.tv/kraken/chat/emoticons";
        private const string emoticonFallbackUrl = @"Content\twitchemoticons.json";
        private bool isOAuthTokenRenewed = false;
        private bool isAnonymous = false;
        private object lockSearch = new object();
        private WebClientBase webClient = new WebClientBase();
        private static List<Emoticon> sharedEmoticons = new List<Emoticon>();
        private object iconParseLock = new object();
        private static bool isFallbackEmoticons = false;
        private static bool isWebEmoticons = false;
        private object counterLock = new object();

        private List<WebPoller> counterWebPollers = new List<WebPoller>();

        public TwitchChat(ChatConfig config) : 
            base(new IRCLoginInfo() {
            HostName = ircDomain,
            Port = ircPort,
        })
        {            
            Config = config;
            Enabled = config.Enabled;
            ContentParsers.Add(MessageParser.ParseURLs);
            ContentParsers.Add(MessageParser.ParseEmoticons);

            Info = new StreamInfo() { 
                HasDescription = false,
                HasGame = true,
                HasTopic = true,
                ChatName = Config.ChatName,
            };


            Games = new ObservableCollection<Game>();

            Users = new Dictionary<string, ChatUser>();
            ChatChannels = new List<string>();

            this.NoticeReceived += TwitchChat_NoticeReceived;
            this.ChatUserJoined += TwitchChat_ChatUserJoined;
            this.ChatUserLeft += TwitchChat_ChatUserLeft;
            
            webClient.KeepAlive = true;
        }

        void TwitchChat_ChatUserLeft(object sender, ChatUserEventArgs e)
        {
            e.ChatUser.ChatName = this.ChatName;

            if (Users.ContainsKey(e.ChatUser.NickName))
                Users.Remove(e.ChatUser.NickName);

            if (e.ChatUser.NickName.Equals(LoginInfo.UserName, StringComparison.InvariantCultureIgnoreCase))
            {
                if (RemoveChannel != null)
                {
                    RemoveChannel(e.ChatUser.Channel, this);
                }
                stopCounterPoller(e.ChatUser.Channel);
            }
        }
        void stopCounterPoller( string channelName )
        {
            UI.Dispatch(() => Status.ToolTips.RemoveAll(t => t.Header == channelName));
            var poller = counterWebPollers.FirstOrDefault(p => p.Id == channelName);
            poller.Stop();
            counterWebPollers.Remove(poller);
        }
        void TwitchChat_ChatUserJoined(object sender, ChatUserEventArgs e)
        {
            e.ChatUser.ChatName = this.ChatName;

            if( Users.ContainsKey( e.ChatUser.NickName ))
            {
                Users[e.ChatUser.NickName] = e.ChatUser;
            }
            else
            {
                Users.Add(e.ChatUser.NickName, e.ChatUser);
            }
            if (e.ChatUser.NickName.Equals(LoginInfo.UserName, StringComparison.InvariantCultureIgnoreCase))
            {
                if (AddChannel != null)
                {
                    AddChannel(e.ChatUser.Channel, this);
                }

                var poller = new WebPoller()
                {
                    Id = e.ChatUser.Channel,
                    Uri = new Uri(String.Format(@"http://api.twitch.tv/kraken/streams/{0}?on_site=1", e.ChatUser.Channel.Replace("#", ""))),
                };

                UI.Dispatch(() => Status.ToolTips.RemoveAll(t => t.Header == poller.Id));
                UI.Dispatch(() => Status.ToolTips.Add( new ToolTip(poller.Id, "")));

                poller.ReadStream = (stream) =>
                {
                    lock( counterLock )
                    {
                        var channelInfo = Json.DeserializeStream<TwitchChannelInfo>(stream);
                        poller.LastValue = channelInfo;
                        var viewers = 0;
                        foreach (var webPoller in counterWebPollers.ToList())
                        {
                            var streamInfo = this.With(x => (TwitchChannelInfo)webPoller.LastValue)
                                .With(x => x.stream);

                            var tooltip = Status.ToolTips.FirstOrDefault(t => t.Header == webPoller.Id);
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
                        UI.Dispatch( () => Status.ViewersCount = viewers);
                    }
                    

                };
                poller.Start();

                counterWebPollers.Add(poller);

                if( !isAnonymous )
                {
                    Status.IsLoggedIn = true;
                    Info.CanBeChanged = true;
                }

                Status.IsStarting = false;
                Status.IsConnecting = false;
                Status.IsLoginFailed = false;
            }
        }
        void TwitchChat_NoticeReceived(object sender, StringEventArgs e)
        {
            if (e.Text.Contains("Login unsuccessful"))
            {
                Status.IsLoginFailed = true;
                Status.IsGotAuthenticationInfo = false;
                Status.IsLoggedIn = false;
            }
        }
        public override bool Start()
        {
            isAnonymous = false;
            Status.IsLoggedIn = false;
            Status.IsConnected = false;
            
            isFallbackEmoticons = false;
            isWebEmoticons = false;
            InitEmoticons();

            var userName = Config.Parameters.StringValue("Username");
            var password = Config.Parameters.StringValue("Password");

            // Reset failed status if credentials are changed
            if (Status.IsLoginFailed && (userName != LoginInfo.UserName || LoginInfo.Password != password))
            {
                Status.IsLoginFailed = false;
                isOAuthTokenRenewed = false;
            }

            LoginInfo.Channels = Config.Parameters.StringArrayValue("Channels").Select(chan => chan.ToLower()).ToArray();
            LoginInfo.UserName = userName;
            LoginInfo.Password = password;
            LoginInfo.RealName = userName;
            
            if (Regex.IsMatch(Config.Parameters.StringValue("Username"), @"justinfan\d+", RegexOptions.IgnoreCase))
            {
                isAnonymous = true;
            }
            else
            {
                if (!LoginInfo.Channels.Any(ch => ch.Equals(LoginInfo.UserName, StringComparison.InvariantCultureIgnoreCase)))
                    LoginInfo.Channels = LoginInfo.Channels.Union(new String[] { LoginInfo.UserName.ToLower() }).ToArray();

            }
            for (int i = 0; i < LoginInfo.Channels.Length; i++)
            {
                LoginInfo.Channels[i] = "#" + LoginInfo.Channels[i].Replace("#", "");
            }

            ChatChannels = LoginInfo.Channels.ToList();
            NickName = LoginInfo.UserName;

            if( !isAnonymous && !(Status.IsLoginFailed && isOAuthTokenRenewed) )
            {
                // Login anonymously if password is empty
                if (String.IsNullOrWhiteSpace(LoginInfo.Password))
                {
                    StartAnonymously();
                }
                else // Login with OAuth token
                {
                    var oauthToken = Config.Parameters.StringValue("OAuthToken");
                    var oauthTokenCredentials = Config.Parameters.StringValue("AuthTokenCredentials");                  

                    if ( LoginInfo.UserName + LoginInfo.Password != oauthTokenCredentials)
                        oauthToken = null;

                    if (!String.IsNullOrWhiteSpace(oauthToken) && !Status.IsLoginFailed)
                    {
                        StartWithToken(oauthToken);
                    }
                    else
                    {
                        Task.Factory.StartNew(() => Authenticate(() =>
                        {
                            Status.IsGotAuthenticationInfo = true;
                            oauthToken = ReadOAuthToken();
                            if( !String.IsNullOrWhiteSpace(oauthToken) )
                            {
                                isOAuthTokenRenewed = true;
                                StartWithToken(oauthToken);
                            }
                            else
                            {
                                Log.WriteError("Unable to get Twitch OAuth token. Joining anonymously...");
                                StartAnonymously();
                            }
                        }));
                    }
                }

            }
            else if( isAnonymous )
            {
                base.Start();
            }
            else
            {
                Log.WriteError("Twitch Login failed. Joining anonymously...");
                StartAnonymously();
            }
            return true;
        }
        private void StartWithToken(string oauthToken)
        {
            LoginInfo.Password = "oauth:" + oauthToken;
            GetTopic();
            base.Start();
        }
        private void StartAnonymously()
        {
            LoginInfo.UserName = "justinfan" + Random.Next(1000000, 9999999).ToString();
            isAnonymous = true;
            base.Start();
        }
        private void InitEmoticons()
        {
            //Fallback icon list
            DownloadEmoticons(AppDomain.CurrentDomain.BaseDirectory + emoticonFallbackUrl);
            //Web icons
            Task.Factory.StartNew(() => DownloadEmoticons(emoticonUrl));
        }
        public override bool Stop()
        {
            foreach( var poller in counterWebPollers.ToList() )
            {
                poller.Stop();
            }
            counterWebPollers.Clear();

            if (RemoveChannel != null)
            {
                foreach (var channel in LoginInfo.Channels)
                {
                    RemoveChannel(channel, this);
                }
            }
          
            return base.Stop();
        }
        public override void DownloadEmoticons(string url)
        {
            if (isFallbackEmoticons && isWebEmoticons )
                return;

            lock(iconParseLock )
            {
                var list = new List<Emoticon>();
                if (Emoticons == null)
                    Emoticons = new List<Emoticon>();

                var jsonEmoticons = this.With( x => Json.DeserializeUrl<TwitchJsonEmoticons>(url))
                    .With( x => x.emoticons);

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
                                list.Add(new Emoticon(  icon.regex.Replace(@"\&gt\;", ">").Replace(@"\&lt\;", "<").Replace(@"\&amp\;", "&"), 
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
                        if (isFallbackEmoticons)
                            isWebEmoticons = true;

                        isFallbackEmoticons = true;
                    }
                }
            }
        }
        private string ReadOAuthToken()
        {
            var oauthToken = this.With(x => webClient.Download("http://api.twitch.tv/api/me?on_site=1"))
                .With(x => JToken.Parse(x))
                .With(x => x.Value<string>("chat_oauth_token"));

            if ( oauthToken != null )
            {
                Config.SetParameterValue("OAuthToken", oauthToken);
                Config.SetParameterValue("AuthTokenCredentials", LoginInfo.UserName + LoginInfo.Password);
            }

            return oauthToken;
        }
        public override bool SendMessage(ChatMessage message)
        {
            RaiseMessageReceive( message.Text, message.Channel, LoginInfo.UserName, important:true, isSentByMe:true );
            return base.SendMessage(message);
        }
        public void Authenticate( Action afterAction)        
        {
            webClient.Headers.Clear();
            webClient.SetCookie("api_token", null, "twitch.tv");
            webClient.SetCookie("csrf_token", null, "twitch.tv");

            var csrfToken = GetCSRFToken();


            if (csrfToken == null)
            {
                Log.WriteError("Twitch: Can't get CSRF token. Twitch web layout changed ?");
                return;
            }

            webClient.SetCookie("csrf_token", csrfToken, "twitch.tv");
            webClient.ContentType = ContentType.UrlEncoded;
            webClient.Headers["X-Requested-With"] = "XMLHttpRequest";
            webClient.Headers["X-CSRF-Token"] = csrfToken;
            webClient.Headers["Accept"] = "text/html, application/xhtml+xml, */*";

            var apiToken = this.With(x => webClient.Upload("https://secure.twitch.tv/user/login", String.Format(
                    "utf8=%E2%9C%93&authenticity_token={0}%3D&redirect_on_login=&embed_form=false&user%5Blogin%5D={1}&user%5Bpassword%5D={2}",
                    csrfToken,
                    Config.Parameters.StringValue("Username"),
                    Config.Parameters.StringValue("Password"))))
                .With( x => webClient.CookieValue("api_token", "http://twitch.tv"));

            if( String.IsNullOrWhiteSpace(apiToken))
            {
                Log.WriteError("Twitch: Can't get API token");
                return;
            }
            //webClient.Headers["Twitch-Api-Token"] = apiToken;
            webClient.Headers["X-CSRF-Token"] = csrfToken;
            webClient.Headers["Accept"] = "*/*";
            
            if( afterAction != null )
                afterAction();
        }

        private string GetCSRFToken()
        {
            return this.With(x => webClient.Download("http://ru.twitch.tv/user/login_popup"))
                    .With(x => Re.GetSubString(x, @"^.*authenticity_token.*?value=""(.*?)"""));
            
        }
        private string TwitchPost(string url, string parameters)
        {
            var csrfToken = webClient.CookieValue("csrf_token", "http://twitch.tv");

            if (String.IsNullOrWhiteSpace(csrfToken)) 
                Authenticate(() => {});

            webClient.Headers["X-CSRF-Token"] = webClient.CookieValue("csrf_token", "http://twitch.tv");
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
            lock( lockSearch)
            {
                Log.WriteInfo("Searching twitch game {0}", gameName);
                Games.Clear();
                Games.Add(new Game() { Name = "Loading..." });
                
                if( callback != null)
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
            if (!Enabled || String.IsNullOrWhiteSpace(LoginInfo.UserName) )
                return;

            Task.Factory.StartNew(() => {
                webClient.ContentType = ContentType.UrlEncodedUTF8;
                var json = this.With(x => webClient.Download(String.Format("http://api.twitch.tv/api/channels/{0}/ember?on_site=1", HttpUtility.UrlEncode(LoginInfo.UserName.ToLower()))))
                    .With(x => !String.IsNullOrWhiteSpace(x)?JToken.Parse(x):null);

                if (json == null || Info == null)
                    return;
                
                Info.Topic = json["status"].ToObject<string>();
                Info.CurrentGame.Name = json["game"].ToObject<string>();
                Info.CanBeRead = true;

                if (StreamTopicAcquired != null)
                    UI.Dispatch( () => StreamTopicAcquired());
            });
        }
        public void SetTopic()
        {

            if (!Status.IsLoggedIn)
                return;

            Task.Factory.StartNew(() => {
                var url = String.Format(@"http://www.twitch.tv/{0}/update", LoginInfo.UserName.ToLower());
                var parameters = String.Format(@"status={0}&game={1}", HttpUtility.UrlEncode(Info.Topic), HttpUtility.UrlEncode(Info.CurrentGame.Name));
                var result = TwitchPost( url,parameters);
                
                // Did you just login on Web and your session cookie is invalid now ? Okay, let's authenticate again...
                if (!result.Contains(Info.Topic))
                {
                    Authenticate(delegate { });
                    result = TwitchPost( url,parameters );
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


    }


    #region Twich emoticon json
    class TwitchJsonEmoticons
    {
        public object _links { get; set; }
        public TwitchJsonEmoticon[] emoticons { get; set; }
    }
    class TwitchJsonEmoticon
    {
        public string regex { get; set; }
        public TwitchJsonImage[] images {get;set;}
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

    public class TwitchChannel
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
        public TwitchChannel channel { get; set; }
    }

    public class TwitchChannelInfo
    {
        public TwitchLinks _links { get; set; }
        public TwitchStream stream { get; set; }    
    }
#endregion

}
