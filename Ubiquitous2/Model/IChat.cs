using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public interface IChat
    {
        event EventHandler<ChatServiceEventArgs> MessageReceived;
        String ChatName { get; }
        String IconURL { get;  }
        bool Enabled { get; set; }
        bool IsStopping { get; set; }
        bool Start();
        bool Stop();
        bool Restart();
        bool SendMessage(String channel, ChatMessage message);
        List<Action<ChatMessage, IChat>> ContentParsers {get;set;}
        List<Emoticon> Emoticons { get; set; }
        void DownloadEmoticons(String url);
        ChatConfig Config {get;set;}
        void Authorize( Action afterAction);
        
    }
}
