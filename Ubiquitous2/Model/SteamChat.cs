using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.SteamApi;
using UB.Properties;
using System.Threading;

namespace UB.Model
{
    public class SteamChat : SteamAPISession, IChat
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken ct;
        private Dictionary<string,string> nickNames = new Dictionary<string,string>();
        private string mainAccountId = null;

        public SteamChat( ChatConfig config) : base()
        {
            Config = config;
            ContentParsers = new List<Action<ChatMessage, IChat>>();
            ChatChannels = new List<string>();
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
            Log.WriteInfo("Steam user status changed {0}", e.update.nick);
        }

        void SteamChat_NewMessage(object sender, SteamAPISession.SteamEvent e)
        {
            if( MessageReceived != null && e.update != null && e.update.origin != null )
            {
                if( !nickNames.ContainsKey(e.update.origin))
                {
                    SteamAPISession.User ui = GetUserInfo(e.update.origin);
                    nickNames.Add(e.update.origin, ui.nickname);
                }

                var nickname = nickNames[e.update.origin];
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
            Status.IsLoggedIn = true;
        }
        #endregion

        private bool Login()
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

            nickNames.Clear();

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

            return false;
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
    }
}
