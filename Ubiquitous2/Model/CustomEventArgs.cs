using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class ChatServiceEventArgs : EventArgs
    {
        public List<ChatMessage> Messages { get; set; }
    }
    public class StringEventArgs : EventArgs
    {
        public StringEventArgs(String text)
        {
            Text = text;
        }
        public String Text { get; set; }
    }

    public class ExceptionEventArgs : EventArgs
    {
        public ExceptionEventArgs( Exception exception)
        {
            Exception = exception;
        }
        public Exception Exception { get; set; }
    }


}

