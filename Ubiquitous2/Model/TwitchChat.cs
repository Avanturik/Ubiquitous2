using System;
using UB.Model.IRC;
using UB.Utils;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.Web;

namespace UB.Model
{
    public class TwitchChat : IRCChatBase
    {
        private const string ircDomain = "irc.twitch.tv";
        private const int ircPort = 6667;
        private const string emoticonUrl = "http://api.twitch.tv/kraken/chat/emoticons";
        private const string emoticonFallbackUrl = @"Content\twitchemoticons.json";
        private bool isOAuthTokenRenewed = false;
        private bool isAnonymous = false;
        private WebClientBase webClient = new WebClientBase();
        private object iconParseLock = new object();

        public TwitchChat(ChatConfig config) : 
            base(new IRCLoginInfo()
        {
            HostName = ircDomain,
            Port = ircPort,
        })
        {

            Config = config;
            Enabled = config.Enabled;
            ContentParsers.Add(MessageParser.ParseURLs);
            ContentParsers.Add(MessageParser.ParseEmoticons);

            Users = new Dictionary<string, ChatUser>();
            ChatChannels = new List<string>();

            this.NoticeReceived += TwitchChat_NoticeReceived;
            this.ChatUserJoined += TwitchChat_ChatUserJoined;
            this.ChatUserLeft += TwitchChat_ChatUserLeft;
        }

        void TwitchChat_ChatUserLeft(object sender, ChatUserEventArgs e)
        {
            e.ChatUser.ChatName = this.ChatName;

            if (Users.ContainsKey(e.ChatUser.NickName))
                Users.Remove(e.ChatUser.NickName);

            if (e.ChatUser.NickName.Equals(LoginInfo.UserName, StringComparison.InvariantCultureIgnoreCase))
            {
                if (RemoveChannel != null)
                    RemoveChannel(e.ChatUser.Channel, this);
            }
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
                    AddChannel(e.ChatUser.Channel, this);

                Status.IsLoggedIn = true;
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
            InitEmoticons();

            isAnonymous = false;
            Status.IsLoggedIn = false;

            var userName = Config.Parameters.StringValue("Username");
            var password = Config.Parameters.StringValue("Password");

            // Reset failed status if credentials are changed
            if (Status.IsLoginFailed && (userName != LoginInfo.UserName || LoginInfo.Password != password))
            {
                Status.IsLoginFailed = false;
                isOAuthTokenRenewed = false;
            }

            LoginInfo.Channels = Config.Parameters.StringArrayValue("Channels");
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
                            if( oauthToken != null )
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
            lock(iconParseLock )
            {
                var list = new List<Emoticon>();

                if (Emoticons == null)
                    Emoticons = new List<Emoticon>();

                var listRef = new WeakReference(list);

                using (var wc = new WebClientBase())
                {
                    var jsonEmoticons = this.With(x => wc.Download(url))
                        .With(x => JToken.Parse(x))
                        .With(x => x.SelectToken("emoticons"))
                        .With(x => x.ToObject<TwitchJsonEmoticons[]>());

                    var jsonRef = new WeakReference(jsonEmoticons);

                    if (jsonEmoticons == null)
                    {
                        Log.WriteError("Error getting Twitch.tv emoticons!");
                    }
                    else
                    {
                        foreach (TwitchJsonEmoticons icon in jsonEmoticons)
                        {
                            if (icon != null && icon.images != null && icon.regex != null)
                            {
                                string regex = (string)icon.regex;
                                var images = icon.images;
                                var image = images.With(x => images).With(x => x.First());

                                if (image != null && image.width != null && image.height != null && image.url != null)
                                {
                                    var decodedRegex = regex.Replace(@"\&gt\;", ">").Replace(@"\&lt\;", "<").Replace(@"\&amp\;", "&");
                                    list.Add(new Emoticon(decodedRegex, image.url, image.width, image.height));
                                }
                            }

                        }
                    }
                    if (list != null && list.Count > 0)
                    {
                        Emoticons = list.ToList();
                    }

                    list = null;
                    jsonEmoticons = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
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
                var oauthField = new ConfigField()
                {
                    DataType = "Text",
                    IsVisible = false,
                    Label = "OAuth token",
                    Name = "OAuthToken",
                    Value = oauthToken
                };

                var existingOAuthField = Config.Parameters.FirstOrDefault( fld => fld.Name == oauthField.Name);
                if (existingOAuthField != null)
                    existingOAuthField = oauthField;
                else
                    Config.Parameters.Add(oauthField);

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
            webClient.Headers["X-Requested-With"] = "XMLHttpRequest";

            var csrfToken = this.With(x => webClient.Download("http://ru.twitch.tv/user/login_popup"))
                .With( x => Re.GetSubString(x, @"^.*authenticity_token.*?value=""(.*?)"""));

            if (csrfToken == null)
            {
                Log.WriteError("Twitch: Can't get CSRF token. Twitch web layout changed ?");
                return;
            }

            webClient.SetCookie("csrf_token", csrfToken, "twitch.tv");
            webClient.ContentType = ContentType.UrlEncoded;
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
            webClient.Headers["Twitch-Api-Token"] = apiToken;
            webClient.Headers["Accept"] = "application/vnd.twitchtv.v2+json";
            afterAction();
        }

        


    }


    class TwitchJsonEmoticons
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
}
