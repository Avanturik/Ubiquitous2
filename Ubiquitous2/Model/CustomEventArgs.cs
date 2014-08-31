using System;

namespace UB.Model
{
    public class ChatServiceEventArgs : EventArgs
    {
        public ChatMessage Message { get; set; }
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

