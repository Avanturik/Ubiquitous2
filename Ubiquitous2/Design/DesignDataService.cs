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
            ChatChannels = new System.Collections.ObjectModel.ObservableCollection<dynamic>();
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
                ChatIconURL = Icons.MainIcon,
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

        public System.Collections.ObjectModel.ObservableCollection<dynamic> ChatChannels
        {
            get;
            set;
        }


        public void SendMessage(ChatMessage message)
        {
            
        }


        public void Stop()
        {
           
        }


        public void StopAllChats()
        {
            throw new NotImplementedException();
        }



        public WebServerService WebServer
        {
            get;
            set;
        }


        public System.Collections.Generic.List<IChat> Chats
        {
            get { return new System.Collections.Generic.List<IChat>(); }                
        }


        public Action<IChat> ChatStatusHandler
        {
            get;
            set;
        }


        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}