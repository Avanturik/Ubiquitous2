using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ServiceLocation;
using UB.Model.IRC;

namespace UB.Model
{
    public class ChatDataService : IChatDataService, IDisposable
    {
        private List<ChatMessage> messageQueue = new List<ChatMessage>();
        private object messageQueueLock = new object();
        private SettingsDataService settingsDataService;
        private Random random;
        private Action<ChatMessage[], Exception> readChatCallback;
        private List<ChatConfig> chatConfigs;
        private List<IChat> chats;

        //Disposable
        private Timer receiveTimer;

        public ChatDataService()
        {
            settingsDataService = ServiceLocator.Current.GetInstance<SettingsDataService>();
            random = new Random();
            ChatChannels = new ObservableCollection<dynamic>();

            Task.Factory.StartNew(() => StartAllChats());
        }

        public List<IChat> Chats
        {
            get
            {
                if( chats == null )
                {
                    chats = new List<IChat>();
                    settingsDataService.GetChatSettings((configs) =>
                    {
                        chatConfigs = configs;
                    });
                    chats = chatConfigs.Select(config => Registry.ChatFactory[config.ChatName](config)).ToList();
                }
                return chats;
            }

        }
        public void GetRandomMessage(Action<ChatMessage, Exception> callback)
        {
            var lorem = Properties.Settings.Default.LoremIpsum;
            var words = lorem.Split(' ');
            var wordsCount = random.Next(5, words.Length);            
            var text = String.Join(" ", Enumerable.Range(0,wordsCount).Select((i,str) => words[i]));

            var message = new ChatMessage(text) {
                FromUserName = "xedoc",
                ChatIconURL = @"/Ubiquitous2;component/Resources/ubiquitous smile.ico",
                Channel = "#loremipsum"
            };
            callback(message, null);
        }
        

        public void ReadMessages( Action<ChatMessage[], Exception> callback)
        {
            readChatCallback = callback;
        }

        public void SwitchChat( String chatName, bool enabled)
        {
            var chatConfig = chatConfigs.FirstOrDefault(c => c.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase));

            if( chatConfig == null )
                return;

            var chat = GetChat(chatName);

            if (chat == null)
                return; 

            if (enabled)
                chat.Start();
            else
                chat.Stop();
        }

        public IChat GetChat( String chatName )
        {
            return Chats.FirstOrDefault(chat => chat.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase));
        }
        
        public void StartAllChats()
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
            }, null, 0, 1500);
            Chats.ForEach(chat => {
                chat.MessageReceived += chat_MessageReceived;
                if (chat.Enabled)
                {
                    chat.AddChannel = (channel, fromChat) => {
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            ChatChannels.Add(new { ChatName = fromChat.ChatName, ChannelName = channel, ChatIconURL = fromChat.IconURL });
                        });
                    };
                    chat.RemoveChannel = (channel, fromChat) =>
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            var searchItem = ChatChannels.FirstOrDefault(item => item.ChatName == fromChat.ChatName && item.ChannelName == channel && item.ChatIconURL == fromChat.IconURL);
                            if (searchItem != null)
                                ChatChannels.Remove(searchItem);
                        });
                    };
                  Task.Factory.StartNew( ()=> chat.Start() );  
                }
            });
        }

        void chat_MessageReceived(object sender, ChatServiceEventArgs e)
        {
            var message = e.Message;
            var chat = sender as IChat;
            message.ChatIconURL = chat.IconURL;
            message.ChatName = chat.ChatName;
            AddMessageToQueue(message);
        }
        void AddMessageToQueue( ChatMessage message )
        {
            lock (messageQueueLock)
            {                
                messageQueue.Add(message);
                if( message.HighlyImportant )
                {
                    readChatCallback(messageQueue.ToArray(), null);
                    messageQueue.Clear();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool nativeOnly)
        {
            receiveTimer.Dispose();
        }


        public string GetRandomText()
        {
            return String.Empty;
        }


        public ObservableCollection<dynamic> ChatChannels { get; set; }


        public void SendMessage(ChatMessage message)
        {
            var chat = Chats.FirstOrDefault(c => c.ChatName == message.ChatName);
            if( chat != null )
                chat.SendMessage(message);

            //TODO send message implementation
        }
    }
}