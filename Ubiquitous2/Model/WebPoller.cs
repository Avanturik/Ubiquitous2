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
            Interval = 30000;
            wc = new WebClientBase();
            Cookies = wc.Cookies;
            IsLongPoll = false;
            IsTimeStamped = true;
            timer = new Timer(poll, this, Timeout.Infinite, Timeout.Infinite);
            wc.ErrorHandler = (error) => {
                Log.WriteError(error);
                if (!IsLongPoll)
                    timer.Change(Interval * 2, Interval * 2);
                else
                    timer.Change(0, Timeout.Infinite);

                gotError = true;
            };
            wc.SuccessHandler = () => {
                if( gotError)
                {
                    if (!IsLongPoll)
                        timer.Change(Interval, Interval);
                    else
                        timer.Change(0, Timeout.Infinite);

                    gotError = false;
                }
            };
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
        private void poll(object sender)
        {
            if (isStopped)
                return;

            lock(lockWebClient)
            {
                var obj = sender as WebPoller;

                if (IsLongPoll)
                    obj.timer.Change(Timeout.Infinite, Timeout.Infinite);

                if (ReadStream != null)
                {
                    ReadStream(wc.DownloadToStream(Uri.OriginalString));
                }
                if( ReadString != null )
                {
                    ReadString(wc.Download(Uri.OriginalString));
                }
                if (IsLongPoll)
                    obj.timer.Change(0, Timeout.Infinite);
            }
        }

        public object LastValue { get; set; }
    }
}
