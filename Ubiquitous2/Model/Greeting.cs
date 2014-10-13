using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class Greeting 
    {
        public Greeting(string title, string message, string chatName)
        {
            Title = title;
            Message = message;
            ChatName = chatName;

        }
        public string Title { get; set; }
        public string Message { get; set; }
        public string ChatName { get; set; }
    }
}
