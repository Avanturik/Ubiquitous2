using System;
using UB.Model.IRC;
using UB.Utils;

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

    }

}
