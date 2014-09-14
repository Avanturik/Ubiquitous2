﻿using System.Configuration;
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
        private WebServer webServer;
        protected override void OnStartup(StartupEventArgs e)
        {
            Utils.Net.DemandTCPPermission();

            if (RenderCapability.Tier == 0)
                Timeline.DesiredFrameRateProperty.OverrideMetadata(
                    typeof(Timeline),
                    new FrameworkPropertyMetadata { DefaultValue = 20 });

            if( Ubiqiutous.Default.WebServerPort >0 && Ubiqiutous.Default.WebServerPort <= 65535)
                webServer = new WebServer(Ubiqiutous.Default.WebServerPort);
        }

    }
}
