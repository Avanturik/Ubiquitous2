using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using UB.LastFM.Services;
using System.Diagnostics;
using UB.Utils;
using UB.Model;
namespace UB.LastFM
{
    public class MusicTickerLastFm : IMusicTicker
    {
        public event EventHandler<EventArgs> OnLogin;
        public event EventHandler<EventArgs> OnLoginFailed;
        public event EventHandler<MusicTickerEventArgs> OnTrackChange;

        private Timer pollTimer;
        private const string API_KEY = "601555201ca3988d08079bc5a7a23a59";
        private const string API_SECRET = "6c1fc90676d9845bd473750fc6c7503c";
        private const int POLL_INTERVAL = 2000;
        private Session _session;
        private User _lfmUser;
        private object pollLock = new object();
        private Track _currentTrack;
        public MusicTickerLastFm()
        {
            Status = new StatusBase();
            pollTimer = new Timer(new TimerCallback(pollTimer_Tick), null, Timeout.Infinite, Timeout.Infinite);

        }
        private void pollTimer_Tick(object o)
        {
            lock (pollLock)
            {
                pollTimer.Change(Timeout.Infinite, Timeout.Infinite);
                poll();
                pollTimer.Change(POLL_INTERVAL, Timeout.Infinite);
            }
            
        }
        public StatusBase Status
        {
            get;
            set;
        }
        public bool Start()
        {
            if (Status.IsStarting)
                return false;

            Status.IsStarting = true;
            Status.IsConnecting = true;
            if( !authenticate() )
            {
                Log.WriteError("Couldn't authenticate on Last.fm. Check credentials!");
                Status.ResetToDefault();
                Status.IsLoginFailed = true;
                return false;
            }
            pollTimer.Change(0, Timeout.Infinite);
            Status.IsStarting = false;
            Status.IsLoggedIn = true;
            Status.IsConnected = true;
            return true;

        }
        public bool Stop()
        {
            pollTimer.Change(Timeout.Infinite, Timeout.Infinite);
            Status.ResetToDefault();
            return true;
        }
        private void poll()
        {
            if (!_session.Authenticated)
                return;

            var track = this.With(x => _session)
                .With(x => _lfmUser)
                .With(x => _lfmUser.GetNowPlaying());

            if (track != null && ( _currentTrack == null || (_currentTrack.GetID() != track.GetID())))
            {
                var album = track.GetAlbum();

                var artist = track.Artist;
                string imageUrl = null;

                if (album != null)
                    imageUrl = album.GetImageURL(AlbumImageSize.Large);
                else if (artist != null)
                    imageUrl = artist.GetImageURL(ImageSize.Large);

                if (OnTrackChange != null)
                    OnTrackChange(this, new MusicTickerEventArgs() { TrackInfo = new MusicTrackInfo(){
                            Album = (album == null ? "" : album.Title),
                            Artist = (artist == null ? "" : artist.Name),
                            ImageURL = imageUrl,
                            Title = track.Title 
                        }
                    });
            }
            _currentTrack = track;

        }
        public LoginInfoBase LoginInfo
        {
            get;
            set;
        }
        private bool authenticate()
        {
            
            if (_session != null && _session.Authenticated)
                return true;

            if( String.IsNullOrWhiteSpace(LoginInfo.UserName) ||
                String.IsNullOrWhiteSpace(LoginInfo.Password))
            {
                Log.WriteError("LastFM couldn't be queried with empty username/password");
            }

            
            try
            {
                _session = new Session(API_KEY, API_SECRET);

                string md5password = Utilities.MD5(LoginInfo.Password);

                _session.Authenticate(LoginInfo.UserName, md5password);
                if (_session.Authenticated)
                {
                    if (OnLogin != null)
                        OnLogin(this, EventArgs.Empty);
                    _lfmUser = new User(LoginInfo.UserName, _session);
                    return true;
                }
                else
                {
                    if (OnLoginFailed != null)
                        OnLoginFailed(this, EventArgs.Empty);
                    return false;
                }
            }
            catch {
                Log.WriteError("Last.FM authentication error");
                return false;
            }
        }
        
         
    }


}
