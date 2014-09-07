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
        public TwitchChat(ChatConfig config) : 
            base(new IRCLoginInfo()
        {
            Channels = config.Parameters.StringArrayValue("Channels"),
            HostName = "irc.twitch.tv",
            UserName = config.Parameters.StringValue("Username"),
            Password = config.Parameters.StringValue("Password"),
            Port = 6667,
            RealName = config.Parameters.StringValue("Username"),
            })
        {
            Enabled = config.Enabled;
            ContentParser = MessageParser.ParseMessage;
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            //Fallback icons
            DownloadEmoticons(baseDir + @"Content\twitchemoticons.json");
            //Web icons
            Task.Factory.StartNew(() => DownloadEmoticons("http://api.twitch.tv/kraken/chat/emoticons") );
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

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


    }

}
