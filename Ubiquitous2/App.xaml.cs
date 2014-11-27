using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using GalaSoft.MvvmLight.Threading;
using UB.Model;
using UB.Properties;
using UB.Utils;
using System.Deployment.Application;
using System.Text.RegularExpressions;

namespace UB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public double ChatBoxWidth { get; set; }
        public double ChatBoxHeight { get; set; }
        public AppConfig AppConfig { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            NativeMethods.SetProcessDPIAware();

            Utils.Net.DemandTCPPermission();

            if (RenderCapability.Tier == 0)
                Timeline.DesiredFrameRateProperty.OverrideMetadata(
                    typeof(Timeline),
                    new FrameworkPropertyMetadata { DefaultValue = 20 });

            Regex.CacheSize = 0;

            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Ubiquitous2");
            WebRequest.DefaultWebProxy = null;
            //RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly; 
        }

    }
}
