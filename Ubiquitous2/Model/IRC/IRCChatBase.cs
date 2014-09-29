using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using dotIRC;
using UB.Model.IRC;
using UB.Model;
using UB.Utils;
using System.Text.RegularExpressions;


namespace UB.Model
{
    public class IRCChatBase : IrcClient, IChat
    {
        public event EventHandler<ChatServiceEventArgs> MessageReceived;
        public event EventHandler<StringEventArgs> NoticeReceived;
        public event EventHandler<ChatUserEventArgs> ChatUserJoined;
        public event EventHandler<ChatUserEventArgs> ChatUserLeft;

        private IRCLoginInfo loginInfo;
        private const String dummyPass = "blah";
        private Timer pingTimer;
        private Timer noPongTimer;
        private const int pingInterval = 30000;
        private Dictionary<String, Action<IrcRawMessageEventArgs>> rawMessageHandlers;
        public Random Random { get; set; }

        public IRCChatBase( IRCLoginInfo info )
        {
            Status = new StatusBase();

            loginInfo = info;

            Random = new Random();

            rawMessageHandlers = new Dictionary<string, Action<IrcRawMessageEventArgs>>()
            {
                {"PRIVMSG", ReadPrivateMessage},
                {"JOIN", UserJoin},
                {"PART", UserLeft},
                {"NOTICE", ReadNotice}
            };

            ContentParsers = new List<Action<ChatMessage, IChat>>();

        }
        
        public IRCLoginInfo LoginInfo { 
            get { return loginInfo;}
            set
            {
                loginInfo = value;
            }
        }

        public virtual bool Start()
        {
            Log.WriteInfo("Trying to start IRC for {0}, chatname: {1}", LoginInfo.UserName,ChatName);
            if (Status.IsConnecting || Status.IsStarting )
                return false;

            if (Config == null)
                return false;

            var tries = 0;
            while (Status.IsStopping && tries < 1000)
            {
                Thread.Sleep(60);
                tries++;
            }

            Initialize();

            noPongTimer = new Timer((obj) =>
            {
                if (!Status.IsStopping)
                {
                    Log.WriteError("No ping reply. Restarting IRC");
                    Status.ResetToDefault();
                    Restart();
                }
            }, null, Timeout.Infinite, Timeout.Infinite);

            pingTimer = new Timer((obj) =>
            {
                Ping();
                noPongTimer.Change(pingInterval, Timeout.Infinite);
            }, null, Timeout.Infinite, Timeout.Infinite);

            if (String.IsNullOrEmpty(LoginInfo.HostName))
                throw new Exception("Hostname must be specified!");

            if (String.IsNullOrEmpty(LoginInfo.UserName))
                throw new Exception("Username must be specified!");

            Status.IsStarting = true;
            Status.IsStopping = false;
            Status.IsConnecting = true;
            Connect();
            return true;         
        }


        public virtual bool Stop()
        {
            Log.WriteInfo("Trying to stop IRC for {0}, chatname: {1}", LoginInfo.UserName, ChatName);
            if (Status.IsConnecting || Status.IsStarting || Status.IsStopping)
                return false;

            Status.IsStarting = false;
            Status.IsConnecting = false;
            Status.IsStopping = true;

            if( noPongTimer != null ) noPongTimer.Change(Timeout.Infinite, Timeout.Infinite);
            if( pingTimer != null ) pingTimer.Change(Timeout.Infinite, Timeout.Infinite);

            Quit("bye!");
            Thread.Sleep(1000);
            return true;
        }

        public bool Restart()
        {
            return (Stop() && Start());
        }

        private void Connect()
        {
            if( Regex.IsMatch( LoginInfo.HostName, @"\d+\.\d+\.\d+\.\d+"))
            {
                Connect(LoginInfo.HostName, LoginInfo.Port, false, new IrcUserRegistrationInfo()
                {
                    UserName = LoginInfo.UserName,
                    NickName = LoginInfo.UserName,
                    RealName = LoginInfo.RealName,
                    Password = String.IsNullOrEmpty(LoginInfo.Password) ? dummyPass : LoginInfo.Password
                });
            }
            else
            {
                GetServerList((hostList) =>
                {

                    if (hostList == null || hostList.AddressList.Count() <= 0)
                    {
                        Log.WriteError("All servers are down. Domain:" + LoginInfo.HostName);
                        return;
                    }

                    Connect(hostList.AddressList[Random.Next(0, hostList.AddressList.Count())], LoginInfo.Port, false, new IrcUserRegistrationInfo()
                    {
                        UserName = LoginInfo.UserName,
                        NickName = LoginInfo.UserName,
                        RealName = LoginInfo.RealName,
                        Password = String.IsNullOrEmpty(LoginInfo.Password) ? dummyPass : LoginInfo.Password
                    });
                });
            }
        }
        private void UserJoin(IrcRawMessageEventArgs e)
        {
            Status.IsJoined = true;
            var user = e.Message.Source;
            if (e.Message.Parameters.Count < 1)
                return;

            var channel = e.Message.Parameters[0];
            if (ChatUserJoined != null)
                ChatUserJoined(this, new ChatUserEventArgs(new ChatUser()
                {
                    NickName = user.Name,
                    Channel = channel
                }));
            Log.WriteInfo("User {0} joined to {1}", user, channel);
        }
        private void UserLeft(IrcRawMessageEventArgs e)
        {
            var user = e.Message.Source;
            if (e.Message.Parameters.Count < 1)
                return;

            var channel = e.Message.Parameters[0];

            if (ChatUserLeft != null)
                ChatUserLeft(this, new ChatUserEventArgs(new ChatUser()
                {
                    NickName = user.Name,
                    Channel = channel
                }));

            Log.WriteInfo("User {0} left {1}", user, channel);

        }
        private void ReadPrivateMessage(IrcRawMessageEventArgs e)
        {
            if (e.Message.Parameters.Count >= 2)
            {

                RaiseMessageReceive(e.Message.Parameters[1], //text
                                    e.Message.Parameters[0], //channel
                                    e.Message.Source.Name //name
                                    );
            }
        }
        protected void RaiseMessageReceive( string text, string channel, string name, bool important = false, bool isSentByMe = false )
        {
            if (MessageReceived != null)
            {
                var message = new ChatMessage()
                {
                    Text = text,
                    Channel = channel,
                    FromUserName = name,
                    IsSentByMe = isSentByMe,
                    HighlyImportant = important,
                };

                if (loginInfo.Channels.Contains(message.Channel))
                {
                    if (ContentParsers != null)
                        ContentParsers.ForEach(parser => parser(message, this));

                    MessageReceived(this, new ChatServiceEventArgs()
                    {
                        Message = message
                    });
                }
            }
        }

        private void ReadNotice(IrcRawMessageEventArgs e)
        {
            if (e.Message.Parameters.Count >= 1)
            {
                if (NoticeReceived != null)
                    NoticeReceived(this, new StringEventArgs(e.RawContent));
            }
        }
        private void GetServerList(Action<IPHostEntry> callback)
        {
            Utils.Net.TestTCPPort(LoginInfo.HostName, LoginInfo.Port, (hosts, error) =>
            {
                callback(hosts);
            });
        }

        void JoinChannels()
        {
            pingTimer.Change(pingInterval, pingInterval);
            foreach (String channel in LoginInfo.Channels)
                Channels.Join( channel );
        }
        protected override void OnPongReceived(IrcPingOrPongReceivedEventArgs e)
        {
            base.OnPongReceived(e);
            noPongTimer.Change(pingInterval, Timeout.Infinite);
        }
        protected override void OnRawMessageReceived(IrcRawMessageEventArgs e)
        {
            base.OnRawMessageReceived(e);
            
            if (rawMessageHandlers.ContainsKey(e.Message.Command))
                rawMessageHandlers[e.Message.Command](e);

            if (Status.IsConnecting)
            {
                Status.IsConnecting = false;
                Status.IsConnected = true;
                JoinChannels();
            }
        }
        protected override void OnDisconnected(EventArgs e)
        {
            Log.WriteInfo("Disconnect event from IRC");
            if (!Status.IsStopping)
                Restart();
            else
                Status.IsStopping = false;

            Status.ResetToDefault();
        }
        protected override void OnError(IrcErrorEventArgs e)
        {
            Status.ResetToDefault();

            Log.WriteError("Error: {0}", e.Error);
            Restart();
            base.OnError(e);
        }

        public virtual bool SendMessage(ChatMessage message)
        {
            var channel = "#" + message.Channel.Replace("#", "").ToLower();
            var ircChannel = Channels.FirstOrDefault(ch => ch.Name.Equals(channel, StringComparison.InvariantCultureIgnoreCase));
            if( null != ircChannel)
            {
                LocalUser.SendMessage(ircChannel, message.Text);
                return true;
            }
            return false;
        }


        public bool Enabled { get; set; }
        


        public List<Action<ChatMessage, IChat>> ContentParsers
        {
            get;
            set;
        }


        public ChatConfig Config
        {
            get;
            set;        
        }


        public virtual String ChatName { get; set; }
        public virtual String IconURL { get; set; }
        public virtual List<Emoticon> Emoticons { get; set; }
        public virtual void DownloadEmoticons(string url) { }

        public new Dictionary<string, ChatUser> Users
        {
            get;
            set;
        }


        public StatusBase Status
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



        public List<string> ChatChannels
        {
            get;
            set;
        }


        public string NickName
        {
            get;
            set;
        }


        public Func<string, object> RequestData
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


        public bool HideViewersCounter
        {
            get;
            set;

        }
    }
}
