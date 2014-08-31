using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UB.Model.IRC;

namespace UB.Model
{
    public class DataService : IDataService
    {
        private List<ChatMessage> messageQueue = new List<ChatMessage>();
        private object messageQueueLock = new object();
        private Timer receiveTimer;

        private Random rnd;
        private Action<ChatMessage[], Exception> readChatCallback;
        public DataService()
        {
            rnd = new Random();
            InitializeChats();
        }

        public void GetMessage(Action<ChatMessage, Exception> callback)
        {
            var lorem = Properties.Settings.Default.LoremIpsum;
            var words = lorem.Split(' ');
            var wordsCount = rnd.Next(5, words.Length);            
            var text = String.Join(" ", Enumerable.Range(0,wordsCount).Select((i,str) => words[i]));

            var message = new ChatMessage(text) {
                FromUserName = "xedoc",
                ImageSource = @"/favicon.ico",
                Channel = "#loremipsum"
            };
            callback(message, null);
        }

        public void ReadMessages( Action<ChatMessage[], Exception> callback)
        {
            readChatCallback = callback;
        }

        public void InitializeChats()
        {
            //Accumulate messages and update UI periodically
            receiveTimer = new Timer((obj) =>
            {
                if ( messageQueue.Count > 0)
                {
                    lock (messageQueueLock)
                    {
                        readChatCallback(messageQueue.ToArray(), null);
                        messageQueue.Clear();
                    }
                }
            }, null, 0, 1000);

            //Twitch
            var userchannel = "goodguygarry";
            
            var irc = new IRCChatBase(new IRCLoginInfo()
            {
                Channel = userchannel,
                Port = 6667,
                UserName = "justinfan" + rnd.Next(10000000).ToString(),
                HostName = "irc.twitch.tv",
            });
            irc.MessageReceived += irc_MessageReceived;
            irc.Start();
        }

        void irc_MessageReceived(object sender, ChatServiceEventArgs e)
        {
            lock(messageQueueLock)
            {
                var m = e.Message;
                m.ImageSource = @"/favicon.ico";
                messageQueue.Add(m);
            }
        }

    }
}