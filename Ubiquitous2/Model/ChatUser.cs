using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class ChatUser
    {
        public ChatUser()
        {
            Badges = new List<UserBadge>();
        }
        public string NickName { get; set; }
        public string Channel { get; set; }
        public string ChatName { get; set; }
        public string GroupName { get; set; }
        public List<UserBadge> Badges { get; set; }
    }
}
