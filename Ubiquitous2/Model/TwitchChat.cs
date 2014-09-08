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

        private WebClientBase webClient = new WebClientBase();

        public TwitchChat(ChatConfig config) : 
            base(new IRCLoginInfo()
        {
            HostName = ircDomain,
            Port = ircPort,
        })
        {
            Emoticons = new List<Emoticon>();

            Config = config;
            Enabled = config.Enabled;
            ContentParsers.Add(MessageParser.ParseURLs);
            ContentParsers.Add(MessageParser.ParseEmoticons);            
            

        }
        public override string IconURL
        {
            get
            {
                return @"/favicon.ico";
            }
        }
        public override string ChatName 
        { 
            get 
            { 
                return "Twitch.tv"; 
            } 
        }

        public override bool Start()
        {
            bool anonymousAccess = false;

            if (Regex.IsMatch(Config.Parameters.StringValue("Username"), @"justinfan\d+", RegexOptions.IgnoreCase))
                anonymousAccess = true;

            if (!anonymousAccess)
                Task.Factory.StartNew(() => Authorize(() => { 
                    
                }));

            base.Start();
            InitEmoticons();


            return true;
        }
        private void InitEmoticons()
        {
            //Fallback icon list
            DownloadEmoticons(AppDomain.CurrentDomain.BaseDirectory + emoticonFallbackUrl);
            //Web icons
            Task.Factory.StartNew(() => DownloadEmoticons(emoticonUrl));
        }
        public override void DownloadEmoticons(string url)
        {

            var list = new List<Emoticon>();

            using (var wc = new WebClientBase())
            {
                var jsonEmoticons = this.With(x => wc.Download(url))
                    .With(x => JToken.Parse(x))
                    .With(x => x.SelectToken("emoticons"))
                    .With(x => x.ToObject<JArray>());

                if (jsonEmoticons == null)
                {
                    Debug.Print("Error getting Twitch.tv emoticons!");
                    list = new List<Emoticon>();
                }
                else
                {
                    foreach (dynamic icon in jsonEmoticons.Children())
                    {
                        if (icon != null && icon.images != null && icon.regex != null)
                        {
                            string regex = (string)icon.regex;
                            JArray images = icon.images as JArray;
                            dynamic image = this.With(x => (JArray)icon.images).With(x => (dynamic)x.First);

                            if (image != null && image.width != null && image.height != null && image.url != null)
                            {
                                var decodedRegex = regex.Replace(@"\&gt\;", ">").Replace(@"\&lt\;", "<").Replace(@"\&amp\;", "&");
                                list.Add(new Emoticon(decodedRegex, (string)image.url, (int)image.width, (int)image.height));
                            }

                        }

                    }
                }
            }
            if( list != null && list.Count > 0 )
                Emoticons = list;
        }
        public override void Authorize( Action afterAction)
        {
            webClient.Headers["X-Requested-With"] = "XMLHttpRequest";

            var csrfToken = this.With(x => webClient.Download("http://ru.twitch.tv/user/login_popup"))
                .With( x => Re.GetSubString(x, @"^.*authenticity_token.*?value=""(.*?)"""));

            if (csrfToken == null)
                throw new Exception("Can't get CSRF token. Twitch web layout changed ?");

            webClient.SetCookie("csrf_token", csrfToken, "twitch.tv");

            webClient.ContentType = ContentType.UrlEncoded;
            webClient.Headers["Accept"] = "text/html, application/xhtml+xml, */*";

            var apiToken = this.With(x => webClient.Upload("https://secure.twitch.tv/user/login", String.Format(
                    "utf8=%E2%9C%93&authenticity_token={0}%3D&redirect_on_login=&embed_form=false&user%5Blogin%5D={1}&user%5Bpassword%5D={2}",
                    csrfToken,
                    Config.Parameters.StringValue("Username"),
                    Config.Parameters.StringValue("Password"))))
                .With( x => webClient.CookieValue("api_token", "http://twitch.tv");

            webClient.Headers["Twitch-Api-Token"] = apiToken;
            webClient.Headers["Accept"] = "application/vnd.twitchtv.v2+json";

            


            //result = webClient.DownloadString(oauthUrl);
            //if (String.IsNullOrEmpty(result))
            //    return false;

            //JObject chatOauthJson = JObject.Parse(result);
            //if (chatOauthJson == null)
            //    return false;

            //String chatOauthKey = chatOauthJson["chat_oauth_token"].ToString();
            //if (String.IsNullOrEmpty(chatOauthKey))
            //    return false;

                    
            //ChatOAuthKey = "oauth:" + chatOauthKey;


        }


    }

}
