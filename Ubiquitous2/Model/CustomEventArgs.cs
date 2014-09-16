using System;
using System.Drawing;

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
        public ExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }
        public Exception Exception { get; set; }
    }
    public class ChatUserEventArgs : EventArgs
    {
        public ChatUserEventArgs(ChatUser user)
        {
            ChatUser = user;
        }
        public ChatUser ChatUser { get; set; }
    }

    public class MusicTickerEventArgs : EventArgs
    {
        public MusicTrackInfo TrackInfo { get; set; }
    }
}

