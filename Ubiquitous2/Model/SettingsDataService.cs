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
            Initialize();
        }
        public void GetChatSettings(Action<List<ChatConfig>> callback)
        {
            callback(Ubiquitous.Default.Config.ChatConfigs);
        }
        public void GetServiceSettings(Action<List<ServiceConfig>> callback)
        {
            callback(Ubiquitous.Default.Config.ServiceConfigs);
        }
        private void Initialize()
        {
            //First launch ?
            if (Ubiquitous.Default.Config == null)
                Ubiquitous.Default.Config = new ConfigSections();

            InitializeChatSettings();
            InitializeServiceSettings();
        }
        private void InitializeServiceSettings()
        {

            if (Ubiquitous.Default.Config.ServiceConfigs == null)
            {
                Ubiquitous.Default.Config.ServiceConfigs = SettingsRegistry.DefaultServiceSettings.ToList();
                Ubiquitous.Default.Save();
            }
            else
            {
                foreach (ServiceConfig serviceConfig in SettingsRegistry.DefaultServiceSettings)
                {
                    var savedConfig = Ubiquitous.Default.Config.ServiceConfigs.Where(config => config.ServiceName.Equals(serviceConfig.ServiceName)).FirstOrDefault();
                    //Chat config is missing
                    if (savedConfig == null)
                    {
                        Ubiquitous.Default.Config.ServiceConfigs.Add(serviceConfig);
                    }
                    else
                    {
                        //Remove unused and add new settings
                        savedConfig.Parameters.RemoveAll(item => !serviceConfig.Parameters.Any(s => s.Name == item.Name && s.DataType == item.DataType));

                        serviceConfig.Parameters.ForEach(item =>
                        {
                            if (!savedConfig.Parameters.Any(s => s.Name == item.Name))
                                savedConfig.Parameters.Add(item);
                        });
                    }
                }

            }
        }
        private void InitializeChatSettings()
        {
            if( Ubiquitous.Default.Config.ChatConfigs == null )
            {
                Ubiquitous.Default.Config.ChatConfigs = SettingsRegistry.DefaultChatSettings.ToList();
                Ubiquitous.Default.Save();
            }
            else
            {
                foreach (ChatConfig chatConfig in SettingsRegistry.DefaultChatSettings)
                {
                    var savedConfig = Ubiquitous.Default.Config.ChatConfigs.Where(config => config.ChatName == chatConfig.ChatName).FirstOrDefault();
                    //Chat config is missing
                    if (savedConfig == null)
                    {
                        Ubiquitous.Default.Config.ChatConfigs.Add(chatConfig);
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
