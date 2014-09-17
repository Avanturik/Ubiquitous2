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
        string NickName { get; set; }
        
        bool Enabled { get; set; }
        bool Start();
        bool Stop();
        bool Restart();
        bool SendMessage(ChatMessage message);

        Dictionary<string, ChatUser> Users { get; set; }
        List<string> ChatChannels { get; set; }
        Action<string, IChat> AddChannel { get; set; }
        Action<string, IChat> RemoveChannel { get; set; }

        List<Action<ChatMessage, IChat>> ContentParsers {get;set;}
        List<Emoticon> Emoticons { get; set; }
        void DownloadEmoticons(String url);
        ChatConfig Config {get;set;}
        StatusBase Status { get; set; }        
    }
}
