using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using UB.LastFM.Services;
using System.Diagnostics;
using UB.Utils;
using UB.Model;
using UB.LastFM;
using System.Linq;

namespace UB.Model
{
    public class LastFMService : NotifyPropertyChangeBase, IService
    {

        private Timer pollTimer;
        private const string API_KEY = "601555201ca3988d08079bc5a7a23a59";
        private const string API_SECRET = "6c1fc90676d9845bd473750fc6c7503c";
        private const int POLL_INTERVAL = 2000;
        private Session _session;
        private User _lfmUser;
        private object pollLock = new object();
        private object startStopLock = new object();
        private Track _currentTrack;

        public LastFMService(ServiceConfig config)
        {
            Config = config;
            Status = new StatusBase();
            pollTimer = new Timer(new TimerCallback(pollTimer_Tick), null, Timeout.Infinite, Timeout.Infinite);
            MusicTrackInfo = new MusicTrackInfo();

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
            lock( startStopLock )
            {
                if( Config == null )
                {

                    Log.WriteError("LastFM can't start without config!");
                    return false;
                }
                if (Status.IsStarting 
                    && !Status.IsLoggedIn 
                    && !Status.IsLoginFailed)
                    return false;

                MusicTrackInfo.Album = "Connecting to last.fm...";
                MusicTrackInfo.Artist = "";
                MusicTrackInfo.Title = "";
                MusicTrackInfo.ImageURL = null;

                Status.IsStarting = true;
                Status.IsConnecting = true;
                if (!authenticate())
                {
                    MusicTrackInfo.Album = "Authentication failure!";
                    Log.WriteError("Couldn't authenticate on Last.fm. Check credentials!");
                    Status.ResetToDefault();
                    Status.IsLoginFailed = true;
                    if (AfterStart != null)
                        AfterStart();
                    return false;
                }
                pollTimer.Change(0, Timeout.Infinite);
                Status.IsStarting = false;
                Status.IsLoggedIn = true;
                Status.IsConnected = true;
                if (AfterStart != null)
                    AfterStart();
                return true;
            }

        }
        public bool Stop()
        {
            lock(startStopLock)
            {
                Status.IsLoggedIn = false;
                _currentTrack = null;
                if( pollTimer != null )
                    pollTimer.Change(Timeout.Infinite, Timeout.Infinite);
                if( Status != null)
                    Status.ResetToDefault();
                return true;
            }
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

                MusicTrackInfo.Album = (album == null ? "" : album.Title);
                MusicTrackInfo.Artist = (artist == null ? "" : artist.Name);
                MusicTrackInfo.ImageURL = imageUrl;
                MusicTrackInfo.Title = track.Title; 

            }
            else if( track == null )
            {
                MusicTrackInfo.Album = "Unknown album";
                MusicTrackInfo.Artist = "Unknown artist";
                MusicTrackInfo.Title = "Unknown title";
            }
            _currentTrack = track;

        }
        
        private bool authenticate()
        {
            
            if (_session != null && Status.IsLoggedIn )
                return true;

            if (Config == null)
            {
                Log.WriteError("LastFM config is empty. Unable to authenticate");
                return false;
            }

            var userName = this.With( x => Config.Parameters.FirstOrDefault(parameter => parameter.Name.Equals("Username", StringComparison.InvariantCultureIgnoreCase)) )
                .With( x => (string)x.Value);
            var password = this.With( x => Config.Parameters.FirstOrDefault(parameter => parameter.Name.Equals("Password", StringComparison.InvariantCultureIgnoreCase)) )
                .With( x => (string)x.Value);
    
            if( String.IsNullOrWhiteSpace(userName) ||
                String.IsNullOrWhiteSpace(password))
            {
                Log.WriteError("LastFM couldn't be queried with empty username/password");
            }

            
            try
            {
                _session = new Session(API_KEY, API_SECRET);

                string md5password = Utilities.MD5(password);

                _session.Authenticate(userName, md5password);
                if (_session.Authenticated)
                {
                    Status.IsLoggedIn = true;
                    _lfmUser = new User(userName, _session);
                    return true;
                }
                else
                {
                    Status.IsLoginFailed = true;
                    return false;
                }
            }
            catch {
                Log.WriteError("Last.FM authentication error");
                return false;
            }
        }

        public void Restart()
        {
            Status.ResetToDefault();

            Stop();

            Start();
        }

        public ServiceConfig Config
        {
            get;
            set;
        }

        public void GetData(Action<object> callback)
        {
            if( !Status.IsLoggedIn)
            {
                MusicTrackInfo.Album = "Connecting to last.fm...";
            }

            callback(MusicTrackInfo);
        }


        public Action AfterStart
        {
            get;
            set;
        }

        /// <summary>
        /// The <see cref="MusicTrackInfo" /> property's name.
        /// </summary>
        public const string MusicTrackInfoPropertyName = "MusicTrackInfo";

        private MusicTrackInfo _musicTrackInfo = null;

        /// <summary>
        /// Sets and gets the MusicTrackInfo property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public MusicTrackInfo MusicTrackInfo
        {
            get
            {
                return _musicTrackInfo;
            }

            set
            {
                if (_musicTrackInfo == value)
                {
                    return;
                }

                RaisePropertyChanging(MusicTrackInfoPropertyName);
                _musicTrackInfo = value;
                RaisePropertyChanged(MusicTrackInfoPropertyName);
            }
        }


    }


}
