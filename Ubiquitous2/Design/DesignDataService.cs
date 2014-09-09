using System;
using System.Linq;
using UB.Model;

namespace UB.Design
{
    public class DesignDataService : IChatDataService
    {
        private Random rnd;
        public DesignDataService()
        {
            rnd = new Random();
        }

        public void GetRandomMessage(Action<ChatMessage, Exception> callback)
        {
            var lorem = Properties.Settings.Default.LoremIpsum;

            var words = lorem.Split(' ');
            var wordsCount = rnd.Next(5, words.Length);
            var text = String.Join(" ", Enumerable.Range(0, wordsCount).Select((i, str) => words[i]));

            var message = new ChatMessage(text) {
                FromUserName = "xedoc",
                ChatIconURL = @"c:\favicon.ico",
                Channel = "#loremipsum"
            };

            callback(message, null);
        }

        public string GetRandomText()
        {
            var lorem = Properties.Settings.Default.LoremIpsum;

            var words = lorem.Split(' ');
            var wordsCount = rnd.Next(5, words.Length);
            var text = String.Join(" ", Enumerable.Range(0, wordsCount).Select((i, str) => words[i]));

            return "Lorem ipsum " + text;
        }

        public void ReadMessages(Action<ChatMessage[],Exception> callback)
        {

        }


        public void SwitchChat(string chatName, bool enabled)
        {
            throw new NotImplementedException();
        }

        public IChat GetChat(string chatName)
        {
            throw new NotImplementedException();
        }

        public void StartAllChats()
        {
            throw new NotImplementedException();
        }


        public void AddChannels(Action<string, IChat> callback)
        {

        }

        public void RemoveChannels(Action<string, IChat> callback)
        {

        }
    }
}