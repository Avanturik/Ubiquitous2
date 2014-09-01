using System;

namespace UB.Model
{
    public interface IChatDataService
    {
        void GetRandomMessage(Action<ChatMessage, Exception> callback);
        void ReadMessages(Action<ChatMessage[], Exception> callback);
    }
}
