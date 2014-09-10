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
            //First launch ?
            if (Ubiqiutous.Default.Config == null)
                Ubiqiutous.Default.Config = new ConfigSections();

            if( Ubiqiutous.Default.Config.ChatConfigs == null )
            {

                Ubiqiutous.Default.Config.ChatConfigs = Registry.DefaultChatSettings.ToList();
                Ubiqiutous.Default.Save();
            }
            else
            {
                foreach (ChatConfig chatConfig in Registry.DefaultChatSettings)
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
                        savedConfig.Parameters.RemoveAll(item => !chatConfig.Parameters.Any(s => s.Name == item.Name && s.DataType == item.DataType ));

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
