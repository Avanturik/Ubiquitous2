using System;
using UB.Model.IRC;
using UB.Utils;
using System.Web.UI;
using System.IO;
using UB.Utils;
using System.Text.RegularExpressions;

namespace UB.Model
{
    public class TwitchChat : IRCChatBase
    {
        public TwitchChat(ChatConfig config) : 
            base(new IRCLoginInfo() { 
                Channels = config.Parameters.StringArrayValue("Channels"),
                HostName = "irc.twitch.tv",
                UserName = config.Parameters.StringValue("Username"),
                Password = config.Parameters.StringValue("Password"),
                Port = 6667,
                RealName = config.Parameters.StringValue("Username"),
            })
        {
            Enabled = config.Enabled;
            ContentParser = contentParser;
        }
        public override string IconURL
        {
            get
            {
                return @"/favicon.ico";
            }
        }
        public override String ChatName 
        { 
            get 
            { 
                return "Twitch.tv"; 
            } 
        }

        void contentParser(ChatMessage message)
        {
            //Parse links
            message.Text = Html.ConvertUrlsToLinks(message.Text);

            //Parse emoticons
            if (message.Text.Contains(":)"))
            {                
                message.Text = message.Text.Replace(":)", 
                    Html.CreateImageTag("http://static-cdn.jtvnw.net/jtv_user_pictures/chansub-global-emoticon-ebf60cd72f7aa600-24x18.png",24,18));
            }

        }

    }

}
