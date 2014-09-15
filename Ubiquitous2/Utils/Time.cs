using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Utils
{
    public static class Time
    {
        public static long UnixTimestamp()
        {

            DateTime nx = new DateTime(1970, 1, 1);
            TimeSpan ts = DateTime.UtcNow - nx;

            return (long)ts.TotalSeconds;

        }
    }
}
