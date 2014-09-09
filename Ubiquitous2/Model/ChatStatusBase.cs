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
            IsGotAuthenticationInfo = false;
            IsLoginFailed = false;
            IsLoggedIn = false;
            IsStopping = false;
            IsStarting = false;
            IsConnecting = false;
        }

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
