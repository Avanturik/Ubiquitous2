using System;
using UB.Utils;

namespace UB.Model
{
    public class ChatMessage
    {
        public ChatMessage()
        {
            Initialize();
        }
        public ChatMessage (String text)
        {
            Text = text;
            Initialize();
        }
        private void Initialize()
        {
            Id = Guid.NewGuid();
            TimeStamp = DateTime.Now.ToLongTimeString();
            UnixTimeStamp = Time.UnixTimestamp();
            HighlyImportant = false;
        }
        public String ChatName { get; set; }
        public String Text{ get; set; }
        public Guid Id { get; set; }
        public String TimeStamp { get; set; }
        public String ChatIconURL { get; set; }
        public String FromUserName { get; set; }
        public String Channel { get; set; }
        public bool HighlyImportant { get; set; }
        public bool IsSentByMe { get; set; }
        public long UnixTimeStamp { get; set; }
    }
}
