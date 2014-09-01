using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    interface IChat
    {
        bool IsStopping { get; set; }
        event EventHandler<ChatServiceEventArgs> MessageReceived;
        bool Start();
        bool Stop();
        bool Restart();
        bool SendMessage(String channel, ChatMessage message);
    }
}
