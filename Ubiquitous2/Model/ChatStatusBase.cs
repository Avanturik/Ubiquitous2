using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace UB.Model
{
    public class ChatStatusBase : IChatStatus
    {
        public ChatStatusBase()
        {
            ResetToDefault();
        }
        public void ResetToDefault()
        {
            IsGotAuthenticationInfo = false;
            IsLoginFailed = false;
            IsLoggedIn = false;
            IsConnected = false;
            IsJoined = false;
            IsStopping = false;
            IsStarting = false;
            IsConnecting = false;
        }
        public bool IsConnected { get; set; }
        public bool IsJoined { get; set; }
        public bool IsGotAuthenticationInfo
        {
            get;
            set;
        }

        public bool IsLoginFailed
        {
            get;
            set;
        }

        public bool IsLoggedIn
        {
            get;
            set;
        }


        public bool IsStopping
        {
            get;
            set;
        }

        public bool IsStarting
        {
            get;
            set;
        }


        public bool IsConnecting
        {
            get;
            set;
        }
    }
}
