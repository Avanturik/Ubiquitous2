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
        private object lockWebClient = new object();
        private Timer timer;
        private bool gotError = false;
        public WebPoller()
        {
            Interval = 30000;
            wc = new WebClientBase();
            Cookies = wc.Cookies;
            timer = new Timer(poll, this, Timeout.Infinite, Timeout.Infinite);
            wc.ErrorHandler = (error) => {
                Log.WriteError(error);
                timer.Change(Interval * 2, Interval * 2);
                gotError = true;
            };
            wc.SuccessHandler = () => {
                if( gotError)
                {
                    timer.Change(Interval, Interval );
                    gotError = false;
                }
            };
        }
        public void Start()
        {
            var newUrl = Uri.OriginalString;
            if( !newUrl.Contains('?'))
            {
                newUrl += "?t=" + Time.UnixTimestamp().ToString();
            }
            else
            {
                newUrl += "&t=" + Time.UnixTimestamp().ToString();
            }
            Uri = new Uri(newUrl);

            timer.Change(0, Interval);
        }
        public void Stop()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        public string Id { get; set; }
        public int Interval { get; set; }
        public CookieContainer Cookies { get; set; }
        public Action<Stream> ReadStream { get; set; }
        public Action<string> ReadString { get; set; }
        public Uri Uri { get; set; }
        
        private void poll(object sender)
        {
            lock(lockWebClient)
            {
                if( ReadStream != null )
                {
                    ReadStream(wc.DownloadToStream(Uri.OriginalString));
                }
                if( ReadString != null )
                {
                    ReadString(wc.Download(Uri.OriginalString));
                }
            }
        }

        public object LastValue { get; set; }
    }
}
