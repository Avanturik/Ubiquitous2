using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ServiceLocation;
using UB.Model.IRC;
using UB.Properties;
using UB.Utils;

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
        private WebServer webServer;
        //Disposable
        private Timer receiveTimer;

        public ChatDataService()
        {
            settingsDataService = ServiceLocator.Current.GetInstance<SettingsDataService>();
            random = new Random();
            ChatChannels = new ObservableCollection<dynamic>();
            ChatChannels.Add(new { ChatName = "AllChats", ChannelName = "#allchats", ChatIconURL = Icons.MainIcon });

            Task.Factory.StartNew(() => startWebServer());
            Task.Factory.StartNew(() => StartAllChats());
        }


        private void startWebServer()
        {
            if (Ubiqiutous.Default.WebServerPort == 0 && Ubiqiutous.Default.WebServerPort > 65535)
                return;

            webServer = new WebServer(Ubiqiutous.Default.WebServerPort);
            webServer.GetUserHandler = (uri) =>
            {
                if( uri.LocalPath.Equals("/messages.json",StringComparison.InvariantCultureIgnoreCase))
                {
                    var parameters = HttpUtility.ParseQueryString(uri.Query);
                    var lastMessageId = parameters["lastid"];
                    if( lastMessageId == null )
                    {
                        // return last n messages
                        GetRandomMessage((msg,err) => {
                            Json.SerializeToStream(msg, (stream) => {
                                webServer.SendJsonToClient(stream);
                            });
                        });                        
                    }
                    else
                    {
                        // return messages after specified
                    }
                    return true;
                }

                return false;
            };

        }
        private void stopWebServer()
        {
            webServer.Stop();
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
                ChatIconURL =  Icons.MainIcon,
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
            var chat = GetChat(chatName);

            if (chat == null)
                return;

            if (enabled)
            {
                chat.Enabled = true;
                chat.Start();
            }
            else
            {
                chat.Enabled = false;
                chat.Stop();
            }

            Log.WriteInfo("switching {0} to {1}", chatName, enabled);
        }

        public IChat GetChat( String chatName )
        {
            return Chats.FirstOrDefault(chat => chat.ChatName.Equals(chatName, StringComparison.InvariantCultureIgnoreCase));
        }
        public void StopAllChats()
        {
            Chats.ForEach(chat => chat.Stop());
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
                chat.AddChannel = (channel, fromChat) =>
                {
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
                if (chat.Enabled)
                {
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
            if( message.ChatName == "AllChats")
            {
                foreach( var chat in Chats)
                {
                    foreach( var channel in chat.ChatChannels )
                    {
                        DispatchMessage(new ChatMessage() { 
                            Channel = channel,
                            ChatIconURL = chat.IconURL,
                            ChatName = chat.ChatName,
                            FromUserName = chat.NickName,
                            HighlyImportant = message.HighlyImportant,
                            IsSentByMe = message.IsSentByMe,
                            Text = message.Text,
                            TimeStamp = DateTime.Now.ToShortTimeString(),                            
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
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    if( chat.Enabled && chat.Status.IsLoggedIn )
                        chat.SendMessage(message);
                });
            }
        }



        public void Stop()
        {
            stopWebServer();
            StopAllChats();
        }

    }
}