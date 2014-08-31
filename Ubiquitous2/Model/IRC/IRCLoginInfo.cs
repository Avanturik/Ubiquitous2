using System;

namespace UB.Model.IRC
{
    public class IRCLoginInfo : LoginInfoBase
    {
        public String Channel { get; set; }        
        public int Port { get; set;  }
        public String RealName { get; set; }
        public String HostName { get; set; }
    }
}
