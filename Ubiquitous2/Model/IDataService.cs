using System;

namespace UB.Model
{
    public interface IDataService
    {
        void GetMessage(Action<ChatMessage, Exception> callback);
        void ReadMessages(Action<ChatMessage[], Exception> callback);
    }
}
