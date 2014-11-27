using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ServiceLocation;
using UB.Properties;
using UB.Utils;

namespace UB.Model
{
    public class ChatDataService : IChatDataService, IDisposable
    {
        private List<ChatMessage> messageQueue = new List<ChatMessage>();
        private object messageQueueLock = new object();
        private ISettingsDataService settingsDataService;
        private IGeneralDataService _dataService;
        private IDatabase _databaseService;
        private ChatToFileService historyService;
        private Random random;
        private Func<ChatMessage[], Exception,bool> readChatCallback;
        private List<ChatConfig> chatConfigs;
        private List<IChat> chats;
        private SteamChat steamChat;
        private Timer timerUpdateDatabase;
        private HashSet<string> ignoreList = new HashSet<string>();
        //Disposable
        private DispatcherTimer receiveTimer = new DispatcherTimer();

        public ChatDataService()
        {
            settingsDataService = ServiceLocator.Current.GetInstance<ISettingsDataService>();
            _dataService = ServiceLocator.Current.GetInstance<IGeneralDataService>();
            _databaseService = ServiceLocator.Current.GetInstance<IDatabase>();

            historyService = _dataService.Services.FirstOrDefault(service => service.Config.ServiceName == SettingsRegistry.ServiceTitleChatToFile) as ChatToFileService;
            random = new Random();            
            Start();
        }

        public void Start()
        {
            ChatChannels = new ObservableCollection<dynamic>();
            ChatChannels.Add(new { ChatName = "AllChats", ChannelName = "#allchats", ChatIconURL = Icons.MainIcon });

            Task.Factory.StartNew(() => StartAllChats());
        }
        public List<IChat> Chats
        {
            set
            {
                if( chats != value)
                    chats = value;
            }
            get
            {
                if( chats == null )
                {
                    chats = new List<IChat>();
                    settingsDataService.GetChatSettings((configs) =>
                    {
                        chatConfigs = configs;
                    });
                    chats = chatConfigs.Select(config => SettingsRegistry.ChatFactory[config.ChatName](config)).ToList();
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
                ChatIconURL =  Icons.MainIcon,
                Channel = "#loremipsum"
            };
            callback(message, null);
        }
        

        public void ReadMessages( Func<ChatMessage[], Exception, bool> callback)
        {
            readChatCallback = callback;
        }

        public async void SwitchChat( String chatName, bool enabled)
        {
            var chat = GetChat(chatName);

            if (chat == null)
                return;

            chat.Enabled = enabled;

            await Task.Run(() => ChatStatusHandler(chat));

            if (enabled)
            {
                await Task.Run(() => chat.Start());
            }
            else
            {
                await Task.Run (() => chat.Stop());
            }
            Log.WriteInfo("switching {0} to {1}", chatName, enabled);
        }
        public IChat GetChat( String chatName )
        {
            if (chatName == null)
                return null;

            return Chats.FirstOrDefault(chat => chat != null && chat.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase));
        }
        public void StopAllChats()
        {
            var stopChats = Chats.Where(chat => chat.Enabled == true).ToList();

            var result = Parallel.For(0, 1, (i) => { });
            

            Task[] stopTasks = new Task[stopChats.Count()];
            for (int i = 0; i < stopTasks.Length; i++)
            {
                var index = i;
                stopTasks[i] = Task.Factory.StartNew(() =>
                {
                    if( i < stopChats.Count )
                        stopChats[i].Stop();
                });
                Thread.Sleep(16);
            }
            Task.WaitAll(stopTasks, 1000);
        }
        private void UpdateDatabase()
        {
            foreach (var chat in Chats.Where(chat => chat != null && chat.Config.Enabled && chat.Status != null))
                _databaseService.AddViewersCount( chat.ChatName, chat.Status.ViewersCount);
        }
        public void StartAllChats()
        {
            steamChat = (SteamChat)GetChat(SettingsRegistry.ChatTitleSteam);
            if (historyService.Config.Enabled)
                historyService.Start();

            //Accumulate messages and update ViewModel periodically            
            receiveTimer.Interval = TimeSpan.FromMilliseconds(1500);
            receiveTimer.Tick += receiveTimer_Tick;
            receiveTimer.Start();

            timerUpdateDatabase = new Timer((o) => { UpdateDatabase(); }, this, 0, 60000);

            int waitChatStatus = 5000;
            Chats.ForEach(chat =>
            {
                chat.MessageReceived += chat_MessageReceived;
                chat.AddChannel = (channel, fromChat) =>
                {
                    UI.Dispatch(() => ChatChannels.Add(new { ChatName = fromChat.ChatName, ChannelName = channel, ChatIconURL = fromChat.IconURL }));
                };
                chat.RemoveChannel = (channel, fromChat) =>
                {
                    UI.Dispatch(() =>
                    {
                        var searchItem = ChatChannels.FirstOrDefault(item => item.ChatName == fromChat.ChatName && item.ChannelName == channel && item.ChatIconURL == fromChat.IconURL);
                        if (searchItem != null)
                            ChatChannels.Remove(searchItem);

                        if (ChatChannels.Count <= 0)
                        {
                            if( !chat.Status.IsStopping )
                            {
                                chat.Status.IsConnected = false;
                                chat.Status.IsLoggedIn = false;
                                chat.Restart();
                            }
                        }
                    });
                };
                if (chat.Enabled)
                {
                    Task.Factory.StartNew(() => {
                        var c = chat;
                        c.Start();

                        while (ChatStatusHandler == null && waitChatStatus > 0)
                        {
                            waitChatStatus -= 50;
                            Thread.Sleep(100);
                        }

                        if( ChatStatusHandler != null )
                            ChatStatusHandler(c);
                    });
                }
            });


        }

        void receiveTimer_Tick(object sender, EventArgs e)
        {
            if (messageQueue.Count > 0 && readChatCallback != null)
            {
                lock (messageQueueLock)
                {
                    var messageList = messageQueue.ToList();
                    if (readChatCallback(messageQueue.ToArray(), null))
                    {
                        Task.Factory.StartNew(() =>
                        {
                            if (steamChat != null && steamChat.Enabled)
                                messageList.ForEach(message => steamChat.SendMessage(message));
                        });

                        messageQueue.Clear();
                    }
                }
            }
        }

        void chat_MessageReceived(object sender, ChatServiceEventArgs e)
        {
            var message = e.Message;
            var chat = sender as IChat;
            message.ChatIconURL = chat.IconURL;
            message.ChatName = chat.ChatName;
            if( CheckFilters( message ))
                AddMessageToQueue(message);
        }
        void AddMessageToQueue( ChatMessage message )
        {
            lock (messageQueueLock)
            {
                historyService.AddToHistory(message);
                messageQueue.Add(message);
                if( message.HighlyImportant )
                {
                    if( readChatCallback(messageQueue.ToArray(), null) )
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
            
        }

        public string GetRandomText()
        {
            return String.Empty;
        }


        public ObservableCollection<dynamic> ChatChannels { get; set; }


        public void SendMessage(ChatMessage message)
        {            
            if( message.ChatName == "AllChats")
            {
                foreach( var chat in Chats)
                {
                    foreach( var channel in chat.ChatChannels )
                    {
                        DispatchMessage(new ChatMessage() { 
                            Channel = channel.ChannelName,
                            ChatIconURL = chat.IconURL,
                            ChatName = chat.ChatName,
                            FromUserName = chat.NickName,
                            HighlyImportant = message.HighlyImportant,
                            IsSentByMe = message.IsSentByMe,
                            Text = message.Text,               
                        });
                    }
                }
            }
            else
            {
                DispatchMessage(message);
            }
        }

        private void DispatchMessage( ChatMessage message )
        {
            var chat = GetChat(message.ChatName);
            if (chat != null)
            {
                UI.Dispatch(() =>
                {
                    if( chat.Enabled && chat.Status.IsLoggedIn )
                        chat.SendMessage(message);
                });
            }
        }



        public void Stop()
        {
            StopAllChats();
            ChatChannels.Clear();
            Chats.Clear();
            Chats = null;
            timerUpdateDatabase.Change(Timeout.Infinite, Timeout.Infinite);
        }



        public Action<IChat> ChatStatusHandler
        {
            get;
            set;
        }


        public void AddMessageSenderToIgnoreList(ChatMessage message)
        {
            string key = String.Format( "{0}@{1}", message.FromUserName, message.ChatName);
            if( !ignoreList.Contains(key) )
            {
                ignoreList.Add(key);
            }
        }

        private bool CheckFilters(ChatMessage message)
        {
            string ignoreKey = String.Format("{0}@{1}", message.FromUserName, message.ChatName);
            if (ignoreList.Contains(ignoreKey))
                return false;

            return true;
        }
    }
}