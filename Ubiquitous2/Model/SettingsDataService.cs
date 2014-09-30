using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using UB.Properties;
using UB.Utils;

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
            InitializeWindowSettings();
            InitializeAppSettings();
            InitializeStreamInfos();
        }
        private void InitializeStreamInfos()
        {
            if( Ubiquitous.Default.Config.StreamInfoPresets == null )
            {
                Ubiquitous.Default.Config.StreamInfoPresets = new List<StreamInfoPreset>();
            }
        }
        private void InitializeAppSettings()
        {
            if( Ubiquitous.Default.Config.AppConfig == null )
            {
                Ubiquitous.Default.Config.AppConfig = SettingsRegistry.DefaultAppSettings;               
            }
            if (String.IsNullOrWhiteSpace(Ubiquitous.Default.Config.AppConfig.ThemeName))
                Ubiquitous.Default.Config.AppConfig.ThemeName = "Main";

            Theme.SwitchTheme(Ubiquitous.Default.Config.AppConfig.ThemeName);
            (Application.Current as App).AppConfig = Ubiquitous.Default.Config.AppConfig;
        }
        private void InitializeWindowSettings()
        {
            if (Ubiquitous.Default.Config.WindowSettings == null)
            {
                Ubiquitous.Default.Config.WindowSettings = SettingsRegistry.WindowSettings.ToList();
                Ubiquitous.Default.Save();
            }
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
                        var backupParameters = savedConfig.Parameters.ToList();
                        savedConfig.Parameters.Clear();
                        foreach( var parameter in serviceConfig.Parameters.ToList() )
                        {
                            if( backupParameters.Any( p => p.Name.Equals(parameter.Name,StringComparison.InvariantCulture)))
                            {
                                parameter.Value = backupParameters.FirstOrDefault(p => p.Name.Equals(parameter.Name, StringComparison.InvariantCulture)).Value;
                            }
                            savedConfig.Parameters.Add(parameter);
                        }
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
                        var backupParameters = savedConfig.Parameters.ToList();
                        savedConfig.Parameters.Clear();
                        foreach (var parameter in chatConfig.Parameters.ToList())
                        {
                            if (backupParameters.Any(p => p.Name.Equals(parameter.Name, StringComparison.InvariantCulture)))
                            {
                                parameter.Value = backupParameters.FirstOrDefault(p => p.Name.Equals(parameter.Name, StringComparison.InvariantCulture)).Value;
                            }
                            savedConfig.Parameters.Add(parameter);
                        }
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


        public void GetAppSettings(Action<AppConfig> callback)
        {
            callback(Ubiquitous.Default.Config.AppConfig);
        }
    }
}
