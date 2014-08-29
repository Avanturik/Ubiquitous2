using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public String Text{ get; set; }
        public Guid Id { get; set; }
        public String TimeStamp { get; set; }
        public String ImageSource { get; set; }
        public String FromUserName { get; set; }

    }
}
