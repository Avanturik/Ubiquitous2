using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class TwitchChat : IRCChatBase
    {
        public TwitchChat(String userName, String password, String[] channels) : 
            base(new IRC.IRCLoginInfo() { 
                Channels = channels,
                HostName = "irc.twitch.tv",
                UserName = userName, 
                Password = password,
                Port = 6667,
                RealName = userName,
            })
        {

        }        
    }

}
