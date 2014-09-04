using System;

namespace UB.Model
{
    public class ChatMessage
    {
        public ChatMessage()
        {

        }
        public ChatMessage (String text)
        {
            Id = Guid.NewGuid();
            TimeStamp = DateTime.Now.ToShortTimeString();
            Text = text;
        }
        public String ChatName { get; set; }
        public String Text{ get; set; }
        public Guid Id { get; set; }
        public String TimeStamp { get; set; }
        public String ChatIconURL { get; set; }
        public String FromUserName { get; set; }
        public String Channel { get; set; }

    }
}
