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
        public static string ChatTitleGamingLive = "Gaminglive.tv";
        public static string ChatTitleHitBox = "Hitbox.tv";
        public static string ChatTitleSteam = "Steam";
        public static string ChatTitleGoodgame = "Goodgame.ru";

        //Service titles
        public static string ServiceTitleMusicTicker = "Music ticker";
        public static string ServiceTitleWebServer = "Web server";
        public static string ServiceTitleImageSaver = "Save chat to image";
        public static string ServiceTitleObsRemote = "OBS control";


        //Default service settings
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
                            new ConfigField("Info1", "Read chat anywhere via browser.", "Info", true, null),
                            new ConfigField("Info2", "Port must not be in use by other apps", "Info", true, null),
                            new ConfigField("Info3", "URL e.g: http://192.168.0.123:8080/","Info", true, null),
                        }
                    },
                    new ServiceConfig()
                    {
                        ServiceName = ServiceTitleImageSaver,
                        IconURL = Icons.PngIcon,
                        Enabled = false,
                        Parameters = new List<ConfigField>() {
                            new ConfigField("Info0", "Save chat to image and use it as Image source in the OBS", "Info", true, null),
                            new ConfigField("Filename", "Chat box", "FileSave", true, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\ubiquitous_chat.png"),
                            new ConfigField("FilenameStatus", "Status window", "FileSave", true, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\ubiquitous_status.png"),
                            new ConfigField("FilenameMusic", "Music ticker", "FileSave", true, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\ubiquitous_music.png"),
                        }
                    },
                    new ServiceConfig()
                    {
                        ServiceName = ServiceTitleObsRemote,
                        IconURL = Icons.OBSIcon,
                        Enabled = false,
                        Parameters = new List<ConfigField>() {
                            new ConfigField("Info0", "Start/stop/switch OBS scenes from Steam overlay/GUI/Web", "Info", true, null),
                            new ConfigField("Host", "OBS host", "Text", true, "localhost"),
                            new ConfigField("Password", "OBSRemote password", "Password", true, "admin"),
                            new ConfigField("Info1", @"Default OBSRemote password is ""admin"". To disable authentication leave it empty", "Info", true, null),
                            new ConfigField("Info2", @"This service depend on OBSRemote plugin: http://obsremote.com", "Info", true, null),
                        }
                    }

                };
            }
        }

        // Default chat settings
        public static List<ChatConfig> DefaultChatSettings
        {
            get
            {
                return new List<ChatConfig>()
                {
                    //Twitch
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
                            new ConfigField("Info1", "Enter justinfan<anydigits> and fill channels to get readonly access", "Info", true, null),
                            new ConfigField("Info2", "Channels is comma separated list. Hashtag is optional. e.g: #xedoc, ipsum, #lorem", "Info", true, null),
                            new ConfigField() {  Name = "AuthTokenCredentials", Label = "Auth token credentials", DataType = "Text", IsVisible = false, Value = String.Empty },
                        }
                    },
                    //Twitch events
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
                            new ConfigField("Info1", "Enter justinfan<anydigits> and fill channels to get readonly access", "Info", true, null),
                            new ConfigField("Info2", "Channels is comma separated list. Hashtag is optional. e.g: #xedoc, ipsum, #lorem", "Info", true, null),
                            new ConfigField() {  Name = "AuthTokenCredentials", Label = "Auth token credentials", DataType = "Text", IsVisible = false, Value = String.Empty },
                        }
                    },
                    //Gaminglive.tv
                    new ChatConfig()
                    {
                        ChatName = ChatTitleGamingLive,
                        IconURL = Icons.GamingLiveIconDark,
                        Enabled = false,
                        Parameters = new List<ConfigField>() {
                            new ConfigField() {  Name = "Username", Label = "Username", DataType = "Text", IsVisible = true, Value = String.Empty },
                            new ConfigField() {  Name = "Password", Label = "Password", DataType = "Password", IsVisible = true, Value = String.Empty },
                            new ConfigField() {  Name = "Channels", Label = "Channels", DataType = "Text", IsVisible = true, Value = "gaminglivetv,fog" },
                            new ConfigField() {  Name = "AuthToken", Label = "AuthToken", DataType = "Text", IsVisible = false, Value = String.Empty },
                            new ConfigField("Info1", "Fill channels and leave username and password empty if you need readonly access.", "Info", true, null),
                            new ConfigField("Info2", "Channels is comma separated list. Hashtag is optional. e.g: #xedoc, ipsum, #lorem", "Info", true, null),
                            new ConfigField() {  Name = "AuthTokenCredentials", Label = "Auth token credentials", DataType = "Text", IsVisible = false, Value = String.Empty },
                        }
                    },
                    //Hitbox.tv
                    new ChatConfig()
                    {
                        ChatName = ChatTitleHitBox,
                        IconURL = Icons.HitboxIcon,
                        Enabled = false,
                        Parameters = new List<ConfigField>() {
                            new ConfigField() {  Name = "Username", Label = "Username", DataType = "Text", IsVisible = true, Value = "UnknownSoldier" },
                            new ConfigField() {  Name = "Password", Label = "Password", DataType = "Password", IsVisible = true, Value = String.Empty },
                            new ConfigField() {  Name = "Channels", Label = "Channels", DataType = "Text", IsVisible = true, Value = "florektv,cr4nki,thebox,dopefish,niewzruszonamasa" },
                            new ConfigField() {  Name = "AuthToken", Label = "AuthToken", DataType = "Text", IsVisible = false, Value = String.Empty },
                            new ConfigField("Info1", "Leave username and password empty and fill channels if you need readonly access", "Info", true, null),
                            new ConfigField("Info2", "Channels is comma separated list. Hashtag is optional. e.g: #xedoc, ipsum, #lorem", "Info", true, null),
                            new ConfigField() {  Name = "AuthTokenCredentials", Label = "Auth token credentials", DataType = "Text", IsVisible = false, Value = String.Empty },
                        }
                    },
                    //Steam
                    new ChatConfig()
                    {
                        ChatName = ChatTitleSteam,
                        IconURL = Icons.SteamIcon,
                        Enabled = false,
                        Parameters = new List<ConfigField>() {
                            new ConfigField("InfoTop", "Chat in fullscreen mode via Steam overlay", "Info", true, null),
                            new ConfigField() {  Name = "Username", Label = "Bot username", DataType = "Text", IsVisible = true, Value = "UnknownSoldier" },
                            new ConfigField() {  Name = "Password", Label = "Bot password", DataType = "Password", IsVisible = true, Value = String.Empty },
                            new ConfigField() {  Name = "Whitelist", Label = "Whiltelist", DataType = "Text", IsVisible = true, Value = String.Empty },
                            new ConfigField("Info0", "Whitelist - comma separated nicknames list. Bot will redirect messages to all his friends if you leave it empty", "Info", true, null),
                            new ConfigField() {  Name = "MessageFormat", Label = "Message format", DataType = "Text", IsVisible = true, Value = "%from @%chatname: %text" },
                            new ConfigField("Info4", "Message format variables: %text - text, %from - from name, %to - to name, %chatname - chat name %time - timestamp", "Info", true, null),
                            new ConfigField() {  Name = "AuthToken", Label = "AuthToken", DataType = "Text", IsVisible = false, Value = String.Empty },
                            new ConfigField("Info1", "Usage: create additional Steam account for bot and add you main account to his friends ", "Info", true, null),
                            new ConfigField("Info3", "If Steam Guard window will popup - enter a code from email", "Info", true, null),
                            new ConfigField() {  Name = "AuthTokenCredentials", Label = "Auth token credentials", DataType = "Text", IsVisible = false, Value = String.Empty },

                        }
                    },
                    //Goodgame.ru
                    new ChatConfig()
                    {
                        ChatName = ChatTitleGoodgame,
                        IconURL = Icons.GoodgameIcon,
                        Enabled = false,
                        Parameters = new List<ConfigField>() {
                            new ConfigField() {  Name = "Username", Label = "Username", DataType = "Text", IsVisible = true, Value = String.Empty },
                            new ConfigField() {  Name = "Password", Label = "Password", DataType = "Password", IsVisible = true, Value = String.Empty },
                            new ConfigField() {  Name = "Channels", Label = "Channels", DataType = "Text", IsVisible = true, Value = "#FlaimStraik, iks_slon,#gopro,twaryna,tapka,kolya_milk,x_kish_x,d1oxde,warrion,CerealKiller" },
                            new ConfigField() {  Name = "AuthToken", Label = "AuthToken", DataType = "Text", IsVisible = false, Value = String.Empty },
                            new ConfigField("Info1", "Leave username and password empty and fill channels if you need readonly access", "Info", true, null),
                            new ConfigField("Info2", "Channels is comma separated list. Hashtag is optional. e.g: #xedoc, ipsum, #lorem", "Info", true, null),
                            new ConfigField() {  Name = "AuthTokenCredentials", Label = "Auth token credentials", DataType = "Text", IsVisible = false, Value = String.Empty },
                        }
                    },

                };
            }        
        }

        public static AppConfig DefaultAppSettings
        {
            get
            {
                return new AppConfig()
                {
                    ThemeName = "Main",
                    EnableTransparency = false,
                    BackgroundOpacity = 0.8,
                    MessageBackgroundOpacity = 0.01,
                    IndividualMessageBackgroundOpacity = 0.8,
                    Parameters = new List<ConfigField>(),
                    MouseTransparency = false,
                };
            }
        }
        // Default chat settings
        public static List<WindowSettings> WindowSettings
        {
            get
            {
                return new List<WindowSettings>()
                {

                };
            }
        }

       
        // Service factory
        public static Dictionary<String, Func<ServiceConfig, IService>> ServiceFactory = new Dictionary<String, Func<ServiceConfig, IService>>()
        {
            //Last.fm - music ticker
            {ServiceTitleMusicTicker, (config)=> { return new LastFMService(config); }},
            //Built-in web server
            {ServiceTitleWebServer, (config)=> { return new WebServerService(config); }},
            //Chat to image saver
            {ServiceTitleImageSaver, (config)=> { return new ChatToImageService(config); }},
            //OBS control via OBSRemote
            {ServiceTitleObsRemote, (config)=> { return new OBSRemoteService(config); }},
            
            
            //TODO: OBS support


        };

        //Chat factory
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
                                            }},
            //GamingLive.tv
            {ChatTitleGamingLive, (config)=>
                                            {
                                                return new GamingLiveChat(config)
                                                {
                                                    ChatName = ChatTitleGamingLive,
                                                    IconURL = Icons.GamingLiveIcon,
                                                };
                                             }},
            //Hitbox.tv
            {ChatTitleHitBox, (config)=>
                                            {
                                                return new HitboxChat(config)
                                                {
                                                    ChatName = ChatTitleHitBox,
                                                    IconURL = Icons.HitboxIcon,
                                                };
                                             }},
            //Steam
            {ChatTitleSteam, (config)=>
                                            {
                                                return new SteamChat(config)
                                                {
                                                    ChatName = ChatTitleSteam,
                                                    IconURL = Icons.SteamIcon,
                                                };
                                             }},

            //Goodgame
            {ChatTitleGoodgame, (config)=>
                                            {
                                                return new GoodgameChat(config)
                                                {
                                                    ChatName = ChatTitleGoodgame,
                                                    IconURL = Icons.GoodgameIcon,
                                                };
                                             }},

        };
    }
}
