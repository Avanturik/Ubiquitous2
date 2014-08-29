using System;
using System.Linq;
using UB.Model;

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
            var lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

            var words = lorem.Split(' ');
            var wordsCount = rnd.Next(5, words.Length);
            var text = String.Join(" ", Enumerable.Range(0, wordsCount).Select((i, str) => words[i]));

            var message = new ChatMessage(text) { FromUserName = "xedoc", ImageSource = @"c:\favicon.ico" };
            callback(message, null);
        }
    }
}