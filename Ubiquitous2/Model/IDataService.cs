using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace UB.Model
{
    public interface IDataService
    {
        void GetData(Action<DataItem, Exception> callback);
        void GetMessage(Action<ChatMessage, Exception> callback);
    }
}
