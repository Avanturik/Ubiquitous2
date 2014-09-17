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
        public static string ChatTitleNormalTwitch = "Twitch.tv";
        public static string ChatTitleEventTwitch = "Twitch.tv(event)";

        //Service titles
        public static string ServiceTitleMusicTicker = "Music ticker";
        public static string ServiceTitleWebServer = "Web server";

        public static List<ServiceConfig> DefaultServiceSettings
        {
            get
            {
                return new List<ServiceConfig>()
                {
                    new ServiceConfig()
                    {
                        ServiceName = ServiceTitleMusicTicker,
                        IconURL = Icons.LastFMIcon,
                        Enabled = false,
                        Parameters = new List<ConfigField>() {
                            new ConfigField("Username", "Username", "Text", true, ""),
                            new ConfigField("Password", "Password", "Password", true, ""),
                            new ConfigField("Info", "Enter your last.fm credentials and enable scrobbling in music player", "Info", true, null),
                        }
                    },
                    new ServiceConfig()
                    {
                        ServiceName = ServiceTitleWebServer,
                        IconURL = Icons.WebServerIcon,
                        Enabled = true,
                        Parameters = new List<ConfigField>() {
                            new ConfigField("Port", "TCP port", "Text", true, "8080"),
                            new ConfigField("Info", "Read chat anywhere via browser.", "Info", true, null),
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
                        ChatName = ChatTitleNormalTwitch,
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
                        ChatName = ChatTitleEventTwitch,
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
            {ServiceTitleMusicTicker, (config)=>
                                            {
                                                return new LastFMService()
                                                {
                                                    Config = config,
                                                };
                                            }},
            {ServiceTitleWebServer, (config)=>
                                            {
                                                return new WebServerService(config);
                                            }},

        };
        public static Dictionary<String, Func<ChatConfig, IChat>> ChatFactory = new Dictionary<String, Func<ChatConfig, IChat>>()
        {
            //Normal twitch IRC channel
            {ChatTitleNormalTwitch, (config)=>
                                            {
                                                return new TwitchChat(config)
                                                {
                                                    ChatName = ChatTitleNormalTwitch,
                                                    IconURL = Icons.TwitchIcon,
                                                };
                                            }},
            //Twitch's Event channel. e.g. #riotgames                                            
            {ChatTitleEventTwitch, (config)=>
                                            {
                                                var twitchChatEvent = new TwitchChat(config)
                                                {
                                                    ChatName = ChatTitleEventTwitch,
                                                    IconURL = Icons.TwitchEventIcon,
                                                };
                                                twitchChatEvent.LoginInfo.HostName = "199.9.252.26";
                                                twitchChatEvent.LoginInfo.Port = 80;
                                                return twitchChatEvent;
                                            } },
        };
    }
}
