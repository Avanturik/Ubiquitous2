using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UB.Model.IRC;

namespace UB.Model
{
    public class ChatDataService : IChatDataService
    {
        private List<ChatMessage> messageQueue = new List<ChatMessage>();
        private object messageQueueLock = new object();
        private Timer receiveTimer;

        private Random rnd;
        private Action<ChatMessage[], Exception> readChatCallback;
        public ChatDataService()
        {
            rnd = new Random();
            InitializeChats();
        }

        public void GetRandomMessage(Action<ChatMessage, Exception> callback)
        {
            var lorem = Properties.Settings.Default.LoremIpsum;
            var words = lorem.Split(' ');
            var wordsCount = rnd.Next(5, words.Length);            
            var text = String.Join(" ", Enumerable.Range(0,wordsCount).Select((i,str) => words[i]));

            var message = new ChatMessage(text) {
                FromUserName = "xedoc",
                ChatIconURL = @"/favicon.ico",
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
            //Accumulate messages and update ViewModel periodically
            receiveTimer = new Timer((obj) =>
            {
                if ( messageQueue.Count > 0 && readChatCallback != null)
                {
                    lock (messageQueueLock)
                    {
                        readChatCallback(messageQueue.ToArray(), null);
                        messageQueue.Clear();
                    }
                }
            }, null, 0, 1000);

            var chatManager = new ChatManager();

            chatManager.Chats.ForEach(chat => {
                chat.MessageReceived += chat_MessageReceived;
                chat.Start();
            });
        }

        void chat_MessageReceived(object sender, ChatServiceEventArgs e)
        {
            var message = e.Message;
            var chat = sender as IChat;
            message.ChatIconURL = chat.IconURL;
            AddMessageToQueue(message);
        }
        void AddMessageToQueue( ChatMessage message )
        {
            lock (messageQueueLock)
            {
                message.ChatIconURL = @"/favicon.ico";
                messageQueue.Add(message);
            }
        }

    }
}