using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using GalaSoft.MvvmLight.Threading;
using UB.Model;
using UB.Properties;

namespace UB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Utils.Net.DemandTCPPermission();

            DispatcherHelper.Initialize();
            if (RenderCapability.Tier == 0)
                Timeline.DesiredFrameRateProperty.OverrideMetadata(
                    typeof(Timeline),
                    new FrameworkPropertyMetadata { DefaultValue = 10 });

            //ConfigSections test = Ubiqiutous.Default.Config;
            //Debug.Print("{0}",test.ChatConfigs.Count);
            //Ubiqiutous.Default.Config = new ConfigSections();
            //Ubiqiutous.Default.Config.ChatConfigs = new System.Collections.Generic.List<ChatConfig>();
            //Ubiqiutous.Default.Config.ChatConfigs.Add(new ChatConfig()
            //{
            //    ChatName = "Twitch.tv",
            //    Enabled = true,
            //    IconURL = @"/favicon.ico",
            //    Parameters = new System.Collections.Generic.List<ConfigField>()
            //{
            //    new ConfigField() { DataType = "Text", IsVisible = true, Label = "User name:", Name = "Username", Value = "default"}
            //}
            //});
            //Ubiqiutous.Default.Save();
            //MakePortable(Settings.Default);
            
        }
        private static void MakePortable(ApplicationSettingsBase settings)
        {
            //var portableSettingsProvider =
            //    new PortableSettingsProvider("Ubiquitous.Default.settings");
            //settings.Providers.Add(portableSettingsProvider);            
            //foreach (SettingsProperty prop in settings.Properties)
            //    prop.Provider = portableSettingsProvider;
            
            //settings.Reload();
            //settings.Save();
        }
    }
}
