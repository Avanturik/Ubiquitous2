using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public static class Registry
    {
        private static Random random = new Random();

        //Titles
        private const string chatTitleNormalTwitch = "Twitch.tv";
        private const string chatTitleEventTwitch = "Twitch.tv(event)";
        
        //Icons
        private const string chatIconTwitch = @"/Ubiquitous2;component/Resources/twitch.ico";

        public static List<ChatConfig> DefaultChatSettings
        {
            get
            {
                return new List<ChatConfig>()
                {
                    new ChatConfig()
                    {
                        ChatName = chatTitleNormalTwitch,
                        IconURL = chatIconTwitch,
                        Enabled = false,
                        Parameters = new List<ConfigField>() {
                            new ConfigField() {  Name = "Username", Label = "Username", DataType = "Text", IsVisible = true, Value = "justinfan" + random.Next(1,123456) },
                            new ConfigField() {  Name = "Password", Label = "Password", DataType = "Password", IsVisible = true, Value = "blah" },
                            new ConfigField() {  Name = "Channels", Label = "Channels", DataType = "Text", IsVisible = true, Value = "goodguygarry,nightblue3,herdyn,#starladder1, mushisgosu" },
                            new ConfigField() {  Name = "OAuthToken", Label = "OAuth token", DataType = "Text", IsVisible = false, Value = String.Empty },
                        }
                    },
                    new ChatConfig()
                    {
                        ChatName = chatTitleEventTwitch,
                        IconURL = chatIconTwitch,
                        Enabled = false,
                        Parameters = new List<ConfigField>() {
                            new ConfigField() {  Name = "Username", Label = "Username", DataType = "Text", IsVisible = true, Value = "justinfan" + random.Next(1,123456) },
                            new ConfigField() {  Name = "Password", Label = "Password", DataType = "Password", IsVisible = true, Value = "blah" },
                            new ConfigField() {  Name = "Channels", Label = "Channels", DataType = "Text", IsVisible = true, Value = "riotgames" },
                            new ConfigField() {  Name = "OAuthToken", Label = "OAuth token", DataType = "Text", IsVisible = false, Value = String.Empty },
                        }
                    }
                };
            }
        }

        public static Dictionary<String, Func<ChatConfig, IChat>> ChatFactory = new Dictionary<String, Func<ChatConfig, IChat>>()
        {
            //Normal twitch IRC channel
            {chatTitleNormalTwitch, (config)=>
                                            {
                                                return new TwitchChat(config)
                                                {
                                                    ChatName = chatTitleNormalTwitch,
                                                    IconURL = chatIconTwitch,
                                                };
                                            }},
            //Twitch's Event channel. e.g. #riotgames                                            
            {chatTitleEventTwitch, (config)=>
                                            {
                                                var twitchChatEvent = new TwitchChat(config)
                                                {
                                                    ChatName = chatTitleEventTwitch,
                                                    IconURL = chatIconTwitch,
                                                };
                                                twitchChatEvent.LoginInfo.HostName = "199.9.252.26";
                                                twitchChatEvent.LoginInfo.Port = 80;
                                                return twitchChatEvent;
                                            } },
        };
    }
}
