using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public interface IFollowersProvider
    {
        Action<ChatUser> AddFollower {get;set;}
        Action<ChatUser> RemoveFollower { get; set; }
    }
}
