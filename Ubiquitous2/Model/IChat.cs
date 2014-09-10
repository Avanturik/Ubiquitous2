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
        String ChatName { get; set; }
        String IconURL { get; set; }
        bool Enabled { get; set; }
        bool Start();
        bool Stop();
        bool Restart();
        bool SendMessage(ChatMessage message);
        Action<string, IChat> AddChannel { get; set; }
        Action<string, IChat> RemoveChannel { get; set; }
        List<Action<ChatMessage, IChat>> ContentParsers {get;set;}
        List<Emoticon> Emoticons { get; set; }
        void DownloadEmoticons(String url);
        ChatConfig Config {get;set;}
        ChatStatusBase Status { get; set; }
        Dictionary<string, ChatUser> Users { get; set; }
    }
}
