using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.LastFM;

namespace UB.Model
{
    public class CurrentTrackDataService : ICurrentTrackDataService
    {
        private MusicTickerLastFm lastFMTicker;
        private bool isInitialized = false;
        public CurrentTrackDataService()
        {
            lastFMTicker = new MusicTickerLastFm();
            lastFMTicker.OnTrackChange += lastFMTicker_OnTrackChange;
        }
        private void initialize()
        {
            lastFMTicker.LoginInfo = new LoginInfoBase()
            {
                UserName = UB.Properties.Ubiquitous.Default.LastFMUserName,
                Password = UB.Properties.Ubiquitous.Default.LastFMPassword
            };
            lastFMTicker.Start();
        }



        void lastFMTicker_OnTrackChange(object sender, MusicTickerEventArgs e)
        {
            if (TrackChangeHandler != null)
                TrackChangeHandler(e.TrackInfo);
        }
        public Action<MusicTrackInfo> TrackChangeHandler
        {
            get;
            set;
        }


        public bool Start()
        {
            initialize();
            return true;
        }

        public bool Stop()
        {
            lastFMTicker.Stop();
            return true;
        }
    }
}
