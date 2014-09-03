using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public interface IChat
    {
        String ChatName { get; }
        String IconURL { get;  }
        bool Enabled { get; set; }
        bool IsStopping { get; set; }
        event EventHandler<ChatServiceEventArgs> MessageReceived;
        bool Start();
        bool Stop();
        bool Restart();
        bool SendMessage(String channel, ChatMessage message);
        ChatConfig GetDefaultSettings();
    }
}
