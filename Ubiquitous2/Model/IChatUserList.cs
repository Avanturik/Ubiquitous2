using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Utils;

namespace UB.Model
{
    internal interface IChatUserList
    {
        SmartCollection<ChatUser> ChatUsers { get; set; }
    }
}
