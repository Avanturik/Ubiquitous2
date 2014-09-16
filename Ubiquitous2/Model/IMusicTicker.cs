using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public interface IMusicTicker
    {
        event EventHandler<EventArgs> OnLogin;
        event EventHandler<EventArgs> OnLoginFailed;
        event EventHandler<MusicTickerEventArgs> OnTrackChange;
        LoginInfoBase LoginInfo { get; set; }
        bool Start();
        bool Stop();

    }
}
