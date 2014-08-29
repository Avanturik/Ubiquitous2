using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UB.Model.IRC;

namespace UB.Model
{
    public class DataService : IDataService
    {
        private Random rnd;
        public DataService()
        {
            rnd = new Random();
        }
        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new DataItem("Welcome to MVVM Light");
            callback(item, null);
        }

        public void GetMessage(Action<ChatMessage, Exception> callback)
        {
            var lorem = Properties.Settings.Default.LoremIpsum;
            var words = lorem.Split(' ');
            var wordsCount = rnd.Next(5, words.Length);            
            var text = String.Join(" ", Enumerable.Range(0,wordsCount).Select((i,str) => words[i]));

            var message = new ChatMessage(text) { FromUserName = "xedoc", ImageSource = @"/favicon.ico" };
            callback(message, null);
        }

    }
}