using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.SteamApi;
using UB.Properties;
using System.Threading;
using UB.Utils;

namespace UB.Model
{
    public class SteamChat : SteamAPISession, IChat
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken ct;
        private Dictionary<string,User> friends = new Dictionary<string,User>();
        private string messageFormatString;
        private HashSet<string> whiteList;
        public SteamChat( ChatConfig config) : base()
        {
            Config = config;
            ContentParsers = new List<Action<ChatMessage, IChat>>();
            ChatChannelNames = new List<string>();
            Emoticons = new List<Emoticon>();
            Status = new StatusBase();
            Users = new Dictionary<string, ChatUser>();

            Logon += SteamChat_Logon;
            NewMessage += SteamChat_NewMessage;
            FriendStateChange += SteamChat_FriendStateChange;
            Typing += SteamChat_Typing;
            SteamGuard += SteamChat_SteamGuard;

            Enabled = Config.Enabled;
            HideViewersCounter = true;

            whiteList = new HashSet<string>();
            foreach( var nick in Config.Parameters.StringArrayValue("Whitelist").Select(chan => chan.ToLower()).ToList())
            {
                whiteList.Add(nick);
            }
            messageFormatString = config.GetParameterValue("MessageFormat") as string;
        }

        #region SteamAPI events
        void SteamChat_SteamGuard(object sender, SteamAPISession.SteamEvent e)
        {
            RequestData("SteamGuardCode");
            SteamGuardKey = (string)Config.GetParameterValue("AuthToken");
        }

        void SteamChat_Typing(object sender, SteamAPISession.SteamEvent e)
        {
            Log.WriteInfo("Steam user typing");
        }

        void SteamChat_FriendStateChange(object sender, SteamAPISession.SteamEvent e)
        {
            AddFriendToCache(e.update.origin);
            friends[e.update.origin].status = e.update.status;

            Log.WriteInfo("Steam user status changed {0}", e.update.nick);
        }
        void AddFriendToCache( string origin )
        {
            if (!friends.ContainsKey(origin))
            {
                User friend = GetUserInfo(origin);
                friends.Add(origin, friend);
            }
        }
        void SteamChat_NewMessage(object sender, SteamAPISession.SteamEvent e)
        {
            //TODO: implement send to current chat
            if( MessageReceived != null && e.update != null && e.update.origin != null )
            {
                AddFriendToCache(e.update.origin);

                var nickname = friends[e.update.origin].nickname;
                if( !String.IsNullOrWhiteSpace(nickname) )
                {
                    MessageReceived(this, new ChatServiceEventArgs()
                    {
                        Message = new ChatMessage()
                        {
                            Channel = "#" + nickname,
                            ChatIconURL = this.IconURL,
                            ChatName = this.ChatName,
                            FromUserName = nickname,
                            HighlyImportant = false,
                            IsSentByMe = true,
                            Text = e.update.message,
                        }
                    });                
                }
            }
        }

        void SteamChat_Logon(object sender, SteamAPISession.SteamEvent e)
        {
            Log.WriteInfo("Steam logged in");
            var friendList = GetFriends();
            foreach( var friend in friendList )
            {
                AddFriendToCache(friend.steamid);
            }
            Status.IsLoggedIn = true;
        }
        #endregion

        public bool Login()
        {
            if (!LoginWithToken())
            {
                if (!LoginWithUsername())
                {
                    Status.IsLoginFailed = true;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        private bool LoginWithToken()
        {
            var authToken = Config.GetParameterValue("AuthToken") as string;
            var userName = Config.GetParameterValue("Username") as string;
            var password = Config.GetParameterValue("Password") as string;
            var tokenCredentials = Config.GetParameterValue("AuthTokenCredentials") as string;

            if (tokenCredentials != userName + password)
                return false;

            if (String.IsNullOrWhiteSpace(authToken))
                return false;

            if( Authenticate(authToken) == LoginStatus.LoginSuccessful)
            {
                return true;
            }

            Config.SetParameterValue("AuthToken", String.Empty);

            return false;
        }

        private bool LoginWithUsername()
        {
            var userName = Config.GetParameterValue("Username") as string;
            var password = Config.GetParameterValue("Password") as string;
            if( String.IsNullOrWhiteSpace(userName) || String.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            Config.SetParameterValue("AuthToken", RSALogin(userName, password));
            Config.SetParameterValue("AuthTokenCredentials", userName + password);

            return LoginWithToken();
        }

        private void PollServer()
        {
            while( !ct.IsCancellationRequested )
            {
                Poll();                
            }
        }

        #region IChat implementation
        public event EventHandler<ChatServiceEventArgs> MessageReceived;

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

        public bool Enabled
        {
            get;
            set;
        }

        public bool Start()
        {
            Log.WriteInfo("Starting Steam chat");
            if (Status.IsStarting || Status.IsConnected || Status.IsLoggedIn || Config == null)
            {
                return true;
            }
            Status.IsStarting = true;
            if (Login())
            {
                Status.IsConnecting = true;
                ct = cts.Token;
                Task myTask = Task.Factory.StartNew(() => PollServer(),ct);
            }
            return false;
        }

        public bool Stop()
        {

            Log.WriteInfo("Stopping Steam chat");

            if (ct != null)
                cts.Cancel();

            friends.Clear();

            if (Status.IsStopping)
                return false;

            Status.IsStopping = true;

            Status.ResetToDefault();
            return true;
        }

        public bool Restart()
        {
            return false;
        }

        public bool SendMessage(ChatMessage message)
        {
            messageFormatString = String.IsNullOrWhiteSpace(messageFormatString) ? message.FormatString : messageFormatString;
            message.FormatString = messageFormatString;
            foreach( var friend in friends.ToList())
            {
                if (whiteList.Count > 0 && !whiteList.Contains(friend.Value.nickname))
                    continue;
                if( friend.Value.status != UserStatus.Offline )
                    base.SendMessage(friend.Value, MessageParser.HtmlToPlainText( message.FormattedText));
            }
            return false;
        }

        public Dictionary<string, ChatUser> Users
        {
            get;
            set;
        }

        public List<string> ChatChannelNames
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
        #endregion


        public Func<string, object> RequestData
        {
            get;
            set;
        }


        public bool HideViewersCounter
        {
            get;
            set;
        }


        public void JoinChannels()
        {
            throw new NotImplementedException();
        }


        public void ReadMessage(ChatMessage message)
        {
            throw new NotImplementedException();
        }


        public bool InitEmoticons()
        {
            throw new NotImplementedException();
        }


        public Func<IChatChannel> CreateChannel
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public void UpdateStats()
        {
            throw new NotImplementedException();
        }

        public List<IChatChannel> ChatChannels
        {
            get
            {
                return ChatChannelNames.Select(x => new ChatChannelBase() { ChannelName = x } as IChatChannel).ToList();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public bool IsAnonymous
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
