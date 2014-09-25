using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UB.Model
{
    public interface IChatDataService
    {
        
        void GetRandomMessage(Action<ChatMessage, Exception> callback);
        void ReadMessages(Func<ChatMessage[], Exception, bool> callback);
        void SwitchChat( String chatName, bool enabled);
        IChat GetChat( String chatName );
        void StartAllChats();
        void StopAllChats();
        string GetRandomText();
        List<IChat> Chats { get;}
        ObservableCollection<dynamic> ChatChannels { get; set; }
        void SendMessage(ChatMessage message);
        void Stop();
        void Start();
        Action<IChat> ChatStatusHandler { get; set; }
        void AddMessageSenderToIgnoreList(ChatMessage message);
    }

}
