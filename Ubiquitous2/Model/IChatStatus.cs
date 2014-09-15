using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    interface IChatStatus
    {
        bool IsGotAuthenticationInfo { get; set; }
        bool IsLoginFailed { get; set; }
        bool IsLoggedIn { get; set; }
        bool IsStopping { get; set;  }
        bool IsStarting { get; set; }
        bool IsConnecting { get; set; }
        bool IsConnected { get; set; }
        bool IsJoined { get; set; }
        List<ToolTip> ToolTips { get; set; }
        int ViewersCount { get; set; }
    }
}
