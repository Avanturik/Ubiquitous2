using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class GoodgameChat : IChat, IStreamTopic
    {
        public event EventHandler<ChatServiceEventArgs> MessageReceived;
        private const string emoticonsUrl = @"http://goodgame.ru/css/compiled/smiles.css";
        private const string emoticonImageUrl = @"http://goodgame.ru/images/generated/smiles.png";
        public GoodgameChat( ChatConfig config)
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


    public class GoodgameChannel
    {
        private WebSocketBase webSocket;
        private GoodgameChat _chat;
        private Random random = new Random();
        private bool isAnonymous = false;

        public GoodgameChannel(GoodgameChat chat)
        {
            _chat = chat;
            Status = new StatusBase();
        }
        public StatusBase Status { get; set; }
        
        public string NickName { get; set; }
        public string AuthToken { get; set; }
        
        public void Join(Action<GoodgameChannel> callback, string nickName, string channel, string authToken)
        {

            if (String.IsNullOrWhiteSpace(channel))
                return;

            NickName = nickName;
            AuthToken = authToken;
            ChannelName = "#" + channel.Replace("#", "");

            webSocket = new WebSocketBase();
            webSocket.PingInterval = 0;
            webSocket.Origin = "http://goodgame.ru";
            webSocket.ConnectHandler = () =>
            {
                SendCredentials(NickName, channel, authToken);

                if (callback != null)
                    callback(this);
            };

            webSocket.DisconnectHandler = () =>
            {
                Log.WriteError("Goodgame disconnected {0}", ChannelName);
                if (LeaveCallback != null)
                    LeaveCallback(this);
            };
            webSocket.ReceiveMessageHandler = ReadRawMessage;
            Connect();
        }

        private void SendCredentials(string nickname, string channel, string authToken)
        {
            isAnonymous = String.IsNullOrWhiteSpace(authToken) || String.IsNullOrWhiteSpace(nickname);
            //webSocket.Send(...);
        }
        private void Connect()
        {
            Status.ResetToDefault();

            //TODO: Get gg websocket port
            int port = 0;
            if (int.TryParse(webSocket.Port, out port))
            {
                //TODO: Get gg websocket IP, path etc
                //TODO: Connect websocket
            }
        }
        private void ReadRawMessage(string rawMessage)
        {
            //TODO: parse gg raw messages
        }
        public string ChannelName { get; set; }
        public void Leave()
        {
            Log.WriteInfo("Goodgame.ru leaving {0}", ChannelName);
            
            if( !webSocket.IsClosed )
                webSocket.Disconnect();
        }

        public Action<GoodgameChannel> LeaveCallback { get; set; }
        public Action<ChatMessage> ReadMessage { get; set; }
        public void SendMessage(ChatMessage message)
        {
            if (isAnonymous || String.IsNullOrWhiteSpace(message.Channel) ||
                String.IsNullOrWhiteSpace(message.FromUserName) ||
                String.IsNullOrWhiteSpace(message.Text))
                return;

            //webSocket.Send(asdf);
        }
    }

}
