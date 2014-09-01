using System;

namespace UB.Model.IRC
{
    public class IRCLoginInfo : LoginInfoBase
    {
        public IRCLoginInfo()
        {
            Channels = new String[] {};
        }
        public String[] Channels { get; set; }        
        public int Port { get; set;  }
        public String RealName { get; set; }
        public String HostName { get; set; }
    }
}
