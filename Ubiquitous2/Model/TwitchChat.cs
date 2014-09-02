using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Model.IRC;

namespace UB.Model
{
    public class TwitchChat : IRCChatBase
    {
        public TwitchChat(String userName, String password, String[] channels) : 
            base(new IRCLoginInfo() { 
                Channels = channels,
                HostName = "irc.twitch.tv",
                UserName = userName, 
                Password = password,
                Port = 6667,
                RealName = userName,
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
