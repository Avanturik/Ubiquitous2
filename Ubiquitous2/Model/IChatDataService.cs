using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UB.Model
{
    public interface IChatDataService
    {
        void GetRandomMessage(Action<ChatMessage, Exception> callback);
        void ReadMessages(Action<ChatMessage[], Exception> callback);
        void SwitchChat( String chatName, bool enabled);
        IChat GetChat( String chatName );
        void StartAllChats();
        string GetRandomText();
        ObservableCollection<dynamic> ChatChannels { get; set; }
    }
}
