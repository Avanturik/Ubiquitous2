using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UB.Utils;

namespace UB.Model
{
    public class WebPoller
    {
        private WebClientBase wc;
        private bool isStopped = true;
        private object lockWebClient = new object();
        private Timer timer;
        private bool gotError = false;
        public WebPoller()
        {
            timer = new Timer(poll, this, Timeout.Infinite, Timeout.Infinite);
            InitWebClient();
        }
        public void InitWebClient()
        {
            TimeoutMs = 60000;
            Interval = 30000;
            Delay = 0;
            wc = new WebClientBase();
            Cookies = wc.Cookies;
            IsLongPoll = false;
            IsTimeStamped = true;
            IsAnonymous = false;
            wc.Timeout = TimeoutMs;
            wc.ErrorHandler = (error) =>
            {

                Log.WriteError(error);
                if (gotError)
                {
                    timer.Change(60000, 60000);
                    return;
                }

                gotError = true;
            };
            wc.SuccessHandler = () =>
            {
                if( gotError )
                    timer.Change(Interval, Interval);

                gotError = false;
            };
            
        }
        public WebHeaderCollection Headers
        {
            get { return wc.Headers;  }
            set { wc.Headers = value; }
        }
        public void Start()
        {
            if( IsTimeStamped )
            {
                var newUrl = Uri.OriginalString;

                if (!newUrl.Contains('?'))
                {
                    newUrl += "?t=" + Time.UnixTimestamp().ToString();
                }
                else
                {
                    newUrl += "&t=" + Time.UnixTimestamp().ToString();
                }
                Uri = new Uri(newUrl);
            }

            wc.KeepAlive = KeepAlive;
            wc.Timeout = TimeoutMs;
            wc.IsAnonymous = IsAnonymous;

            if (IsLongPoll)
                wc.Timeout = 60000;

            timer.Change(0, Interval);
            isStopped = false;
        }
        public void Stop()
        {
            isStopped = true;
            if( timer != null )
                timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        public string Id { get; set; }
        public int Interval { get; set; }
        public CookieContainer Cookies { get; set; }
        public Action<Stream> ReadStream { get; set; }
        public Action<string> ReadString { get; set; }
        public bool IsLongPoll { get; set; }
        public bool IsTimeStamped { get; set; }
        public Uri Uri { get; set; }
        public Action ErrorHandler { get; set; }
        public bool KeepAlive { get; set; }
        public int TimeoutMs { get; set; }
        public int Delay { get; set; }
        public bool IsAnonymous { get; set; }
        private void poll(object sender)
        {
            if (isStopped)
                return;

            lock(lockWebClient)
            {
                var obj = sender as WebPoller;

                if (obj == null)
                    return;

                if (IsLongPoll)
                    obj.timer.Change(Timeout.Infinite, Timeout.Infinite);

                if (obj.ReadStream != null)
                {
                    using( Stream stream = obj.wc.DownloadToMemoryStream( obj.Uri.OriginalString))
                    {

                        if (obj.gotError)
                            return;

                        obj.ReadStream(stream);
                    }
                }

                if( obj.ReadString != null )
                {
                    obj.ReadString(obj.wc.Download(obj.Uri.OriginalString));
                }
                if (IsLongPoll)
                    obj.timer.Change(obj.Delay, Timeout.Infinite);
            }
        }

        public object LastValue { get; set; }
    }
}
