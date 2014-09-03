using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public override ChatConfig GetDefaultSettings()
        {
            return new ChatConfig()
            {
                ChatName = this.ChatName,
                Enabled = false,
                IconURL = this.IconURL,
                Parameters = new List<ConfigField>() {
                    new ConfigField() { DataType = "Text", IsVisible = true, Label = "User name:", Name = "Username" },
                    new ConfigField() { DataType = "Password", IsVisible = true, Label = "Password:", Name = "Password" },
                    new ConfigField() { DataType = "Text", IsVisible = true, Label = "Channels:", Name = "Channels" }
                }
            };
        }
    }

}
