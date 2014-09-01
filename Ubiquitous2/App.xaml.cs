using System.Configuration;
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
