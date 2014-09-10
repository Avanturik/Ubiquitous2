using System;
using System.Collections.Generic;
using System.Linq;
using UB.Properties;

namespace UB.Model
{
    public class SettingsDataService : ISettingsDataService
    {
        public SettingsDataService()
        {
            InitializeChatSettings();
        }
        public void GetChatSettings(Action<List<ChatConfig>> callback)
        {
            callback(Ubiqiutous.Default.Config.ChatConfigs);
        }
        private void InitializeChatSettings()
        {
            var random = new Random();

            //Twitch.tv default settings
            var twitchDefaultConfig = new ChatConfig() { ChatName = "Twitch.tv", IconURL = @"/favicon.ico", Enabled = false, 
                Parameters = new List<ConfigField>() {
                new ConfigField() {  Name = "Username", Label = "Username", DataType = "Text", IsVisible = true, Value = "justinfan" + random.Next(1,123456) },
                new ConfigField() {  Name = "Password", Label = "Password", DataType = "Password", IsVisible = true, Value = "blah" },
                new ConfigField() {  Name = "Channels", Label = "Channels", DataType = "Text", IsVisible = true, Value = "goodguygarry,nightblue3,herdyn,#starladder1, mushisgosu" },
                new ConfigField() {  Name = "OAuthToken", Label = "OAuth token", DataType = "Text", IsVisible = false, Value = String.Empty },
                }
            };
            var twitchEventDefaultConfig = new ChatConfig()
            {
                ChatName = "Twitch.tv(event)",
                IconURL = @"/favicon.ico",
                Enabled = false,
                Parameters = new List<ConfigField>() {
                new ConfigField() {  Name = "Username", Label = "Username", DataType = "Text", IsVisible = true, Value = "justinfan" + random.Next(1,123456) },
                new ConfigField() {  Name = "Password", Label = "Password", DataType = "Password", IsVisible = true, Value = "blah" },
                new ConfigField() {  Name = "Channels", Label = "Channels", DataType = "Text", IsVisible = true, Value = "riotgames" },
                new ConfigField() {  Name = "OAuthToken", Label = "OAuth token", DataType = "Text", IsVisible = false, Value = String.Empty },
                }
            };


            List<ChatConfig> chatConfigs = new List<ChatConfig>() { twitchDefaultConfig, twitchEventDefaultConfig };

            //First launch ?
            if (Ubiqiutous.Default.Config == null)
                Ubiqiutous.Default.Config = new ConfigSections();

            if( Ubiqiutous.Default.Config.ChatConfigs == null )
            {

                Ubiqiutous.Default.Config.ChatConfigs = chatConfigs.ToList();
                Ubiqiutous.Default.Save();
            }
            else
            {
                foreach (ChatConfig chatConfig in chatConfigs)
                {
                    var savedConfig = Ubiqiutous.Default.Config.ChatConfigs.Where(config => config.ChatName == chatConfig.ChatName).FirstOrDefault();
                    //Chat config is missing
                    if (savedConfig == null)
                    {
                        Ubiqiutous.Default.Config.ChatConfigs.Add(chatConfig);
                    }
                    else
                    {
                        //Remove unused and add new settings
                        //savedConfig.Parameters.RemoveAll(item => !chatConfig.Parameters.Any(s => s.Name == item.Name && s.DataType == item.DataType ));
                        chatConfig.Parameters.ForEach(item =>
                        {
                            if( !savedConfig.Parameters.Any( s => s.Name == item.Name ))
                                savedConfig.Parameters.Add(item);
                        });
                    }
                }

            }

        }

        public void GetRandomChatSetting(Action<ChatConfig> callback)
        {
            callback(
                new ChatConfig()
                {
                    ChatName = "LoremIpsum.tv",
                    Enabled = true,
                    Parameters = new List<ConfigField>() {
                        new ConfigField() { DataType = "Text", IsVisible = true, Label = "User name:", Name = "Username", Value = "loremuser" }
                       }
                });

        }


        public void GetRandomTextField(Action<ConfigField> callback)
        {
            callback(
                 new ConfigField() { DataType = "Text", IsVisible = true, Label = "User name:", Name = "Username", Value = "loremuser" }
            );
        }
    }
}
