using System;
using System.Linq;
using System.Reflection;
using UB.Model;
using System.IO;

namespace UB.Design
{
    public class DesignDataService : IDataService
    {
        private Random rnd;
        public DesignDataService()
        {
            rnd = new Random();
        }
        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to create design time data

            var item = new DataItem("Welcome to MVVM Light [design]");
            callback(item, null);
        }

        public void GetMessage(Action<ChatMessage, Exception> callback)
        {
            var lorem = Properties.Settings.Default.LoremIpsum;

            var words = lorem.Split(' ');
            var wordsCount = rnd.Next(5, words.Length);
            var text = String.Join(" ", Enumerable.Range(0, wordsCount).Select((i, str) => words[i]));

            var message = new ChatMessage(text) { FromUserName = "xedoc", ImageSource = @"c:\favicon.ico" };

            callback(message, null);
        }
    }
}