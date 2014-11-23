using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    internal interface IChatUserList
    {
        ObservableCollection<ChatUser> ChatUsers { get; set; }
    }
}
