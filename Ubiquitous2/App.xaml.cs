using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using GalaSoft.MvvmLight.Threading;

namespace UB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
            if (RenderCapability.Tier == 0)
                Timeline.DesiredFrameRateProperty.OverrideMetadata(
                    typeof(Timeline),
                    new FrameworkPropertyMetadata { DefaultValue = 10 });
        }
    }
}
