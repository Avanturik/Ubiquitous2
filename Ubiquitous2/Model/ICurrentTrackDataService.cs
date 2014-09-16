using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public interface ICurrentTrackDataService
    {
        Action<MusicTrackInfo> TrackChangeHandler { get; set; }
        bool Start();
        bool Stop();
    }
}
