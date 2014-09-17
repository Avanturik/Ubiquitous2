using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public static class SettingsRegistry
    {
        private static Random random = new Random();

        //Chat titles
        private const string chatTitleNormalTwitch = "Twitch.tv";
        private const string chatTitleEventTwitch = "Twitch.tv(event)";

        //Service titles
        private const string serviceTitleMusicTicker = "Music ticker";

        public static List<ServiceConfig> DefaultServiceSettings
        {
            get
            {
                return new List<ServiceConfig>()
                {
                    new ServiceConfig()
                    {
                        ServiceName = serviceTitleMusicTicker,
                        IconURL = Icons.LastFMIcon,
                        Enabled = false,
                        Parameters = new List<ConfigField>() {
                            new ConfigField("Username", "Username", "Text", true, ""),
                            new ConfigField("Password", "Password", "Password", true, "")
                        }
                    }
                };
            }
        }
        public static List<ChatConfig> DefaultChatSettings
        {
            get
            {
                return new List<ChatConfig>()
                {
                    new ChatConfig()
                    {
                        ChatName = chatTitleNormalTwitch,
                        IconURL = Icons.TwitchIcon,
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
                        IconURL = Icons.TwitchEventIcon,
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
        public static Dictionary<String, Func<ServiceConfig, IService>> ServiceFactory = new Dictionary<String, Func<ServiceConfig, IService>>()
        {
            //Normal twitch IRC channel
            {serviceTitleMusicTicker, (config)=>
                                            {
                                                return new LastFMService()
                                                {
                                                    Config = config,
                                                };
                                            }},
        };
        public static Dictionary<String, Func<ChatConfig, IChat>> ChatFactory = new Dictionary<String, Func<ChatConfig, IChat>>()
        {
            //Normal twitch IRC channel
            {chatTitleNormalTwitch, (config)=>
                                            {
                                                return new TwitchChat(config)
                                                {
                                                    ChatName = chatTitleNormalTwitch,
                                                    IconURL = Icons.TwitchIcon,
                                                };
                                            }},
            //Twitch's Event channel. e.g. #riotgames                                            
            {chatTitleEventTwitch, (config)=>
                                            {
                                                var twitchChatEvent = new TwitchChat(config)
                                                {
                                                    ChatName = chatTitleEventTwitch,
                                                    IconURL = Icons.TwitchEventIcon,
                                                };
                                                twitchChatEvent.LoginInfo.HostName = "199.9.252.26";
                                                twitchChatEvent.LoginInfo.Port = 80;
                                                return twitchChatEvent;
                                            } },
        };
    }
}
