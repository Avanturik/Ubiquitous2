using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using UB.Model.Services;
using System.Diagnostics;
using UB.Utils;
using UB.Model;
using System.Linq;

namespace UB.Model
{
    public class LastFMService : IService
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
        public LastFMService()
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
            lock( startStopLock )
            {
                if( Config == null )
                {
                    Log.WriteError("LastFM can't start without config!");
                    return false;
                }
                if (Status.IsStarting)
                    return false;

                Status.IsStarting = true;
                Status.IsConnecting = true;
                if (!authenticate())
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

        }
        public bool Stop()
        {
            lock(startStopLock)
            {
                pollTimer.Change(Timeout.Infinite, Timeout.Infinite);
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

                musicTrackInfo = new MusicTrackInfo(){
                            Album = (album == null ? "" : album.Title),
                            Artist = (artist == null ? "" : artist.Name),
                            ImageURL = imageUrl,
                            Title = track.Title 
                        };
            }
            _currentTrack = track;

        }
        
        private MusicTrackInfo musicTrackInfo { get; set; }

        private bool authenticate()
        {
            
            if (_session != null && _session.Authenticated)
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
            callback(musicTrackInfo);
        }
    }


}
