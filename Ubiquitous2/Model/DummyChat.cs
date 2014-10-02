using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class DummyChat : IChat, IStreamTopic
    {
        public event EventHandler<ChatServiceEventArgs> MessageReceived;

        public DummyChat(ChatConfig config)
        {
            Config = config;
            ContentParsers = new List<Action<ChatMessage, IChat>>();
            ChatChannels = new List<string>();
            Emoticons = new List<Emoticon>();
            Status = new StatusBase();
            Users = new Dictionary<string, ChatUser>();

            Info = new StreamInfo()
            {
                HasDescription = false,
                HasGame = true,
                HasTopic = true,
                ChatName = Config.ChatName,
            };

            Games = new ObservableCollection<Game>();

            Enabled = Config.Enabled;
        }
        public string ChatName
        {
            get;
            set;
        }

        public string IconURL
        {
            get;
            set;

        }

        public string NickName
        {
            get;
            set;

        }

        public bool HideViewersCounter
        {
            get;
            set;

        }

        public bool Enabled
        {
            get;
            set;

        }

        public bool Start()
        {
            return true;
        }

        public bool Stop()
        {
            return true;
        }

        public bool Restart()
        {
            return true;
        }

        public bool SendMessage(ChatMessage message)
        {
            return true;
        }

        public Dictionary<string, ChatUser> Users
        {
            get;
            set;

        }

        public List<string> ChatChannels
        {
            get;
            set;

        }

        public Action<string, IChat> AddChannel
        {
            get;
            set;

        }

        public Action<string, IChat> RemoveChannel
        {
            get;
            set;

        }

        public Func<string, object> RequestData
        {
            get;
            set;

        }

        public List<Action<ChatMessage, IChat>> ContentParsers
        {
            get;
            set;

        }

        public List<Emoticon> Emoticons
        {
            get;
            set;

        }

        public void DownloadEmoticons(string url)
        {
           
        }

        public ChatConfig Config
        {
            get;
            set;

        }

        public StatusBase Status
        {
            get;
            set;

        }

        public StreamInfo Info
        {
            get;
            set;

        }

        public ObservableCollection<Game> Games
        {
            get;
            set;

        }

        public string SearchQuery
        {
            get;
            set;

        }

        public void QueryGameList(string gameName, Action callback)
        {
           
        }

        public void GetTopic()
        {
           
        }

        public void SetTopic()
        {
            
        }

        public Action StreamTopicAcquired
        {
            get;
            set;

        }
    }
}
