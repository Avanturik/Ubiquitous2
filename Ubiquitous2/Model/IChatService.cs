using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{


    interface IChatService
    {
        event EventHandler<ChatServiceEventArgs> MessageReceived;
        bool Start();
        bool Stop();
    }
}
