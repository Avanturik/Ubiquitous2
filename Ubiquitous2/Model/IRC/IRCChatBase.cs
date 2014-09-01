using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using dotIRC;
using UB.Model.IRC;

namespace UB.Model
{
    public class IRCChatBase : IrcClient, IChat
    {
        public event EventHandler<ChatServiceEventArgs> MessageReceived;

        private IRCLoginInfo loginInfo;
        private const String dummyPass = "!@$#@";
        private Timer pingTimer;
        private Timer noPongTimer;
        private const int pingInterval = 30000;
        
        private Random random { get; set; }
        public IRCChatBase( IRCLoginInfo info )
        {
            loginInfo = info;

            if (String.IsNullOrEmpty(LoginInfo.HostName))
                throw new Exception("Hostname must be specified!");
            
            if (String.IsNullOrEmpty(LoginInfo.UserName))
                throw new Exception("Username must be specified!");

            random = new Random();


        }
        public bool IsStopping { get; set; }
        
        public IRCLoginInfo LoginInfo { 
            get { return loginInfo;}
            set {
                if( value.Channels.Length != loginInfo.Channels.Length || 
                    value.Channels.Intersect(loginInfo.Channels).Count() != value.Channels.Length )
                {
                    if (!IsStopping)
                    {
                        Debug.Print("Restarting IRC with new login info");
                        Restart();
                    }
                }
            }
        }

        public bool Start()
        {
            noPongTimer = new Timer((obj) => {
                if (!IsStopping)
                {
                    Debug.Print("No ping reply. Restarting IRC");
                    Restart();
                }
            }, null, Timeout.Infinite, Timeout.Infinite);

            pingTimer = new Timer((obj) =>
            {
                Ping();
                noPongTimer.Change(pingInterval, Timeout.Infinite);
            }, null, Timeout.Infinite, Timeout.Infinite);
            
            IsStopping = false;
            Connect();
            return true;
        }


        public bool Stop()
        {
            Debug.Print("Stopping IRC...");
            noPongTimer.Change(Timeout.Infinite, Timeout.Infinite);
            pingTimer.Change(Timeout.Infinite, Timeout.Infinite);

            IsStopping = true;
            Quit();
            Disconnect();
            return true;
        }

        public bool Restart()
        {
            return (Stop() && Start());
        }

        private void Connect()
        {
            GetServerList((hostList) => {
                var hostCount = hostList.AddressList.Count();
                if (hostCount <= 0)
                    throw new Exception("All servers are down. Domain:" + LoginInfo.HostName);
                
                Connect(hostList.AddressList[random.Next(0,hostCount)], LoginInfo.Port, false, new IrcUserRegistrationInfo()
                { 
                    UserName = LoginInfo.UserName,
                    NickName = LoginInfo.UserName,
                    RealName = LoginInfo.RealName,
                    Password = String.IsNullOrEmpty(LoginInfo.Password) ? dummyPass : LoginInfo.Password
                });
            });
        }

        private void GetServerList(Action<IPHostEntry> callback)
        {
            Utils.Net.TestTCPPort(LoginInfo.HostName, LoginInfo.Port, (hosts, error) =>
            {
                if (error != null)
                    throw error;
                callback(hosts);
            });
        }
        protected override void OnConnected(EventArgs e)
        {
            base.OnConnected(e);
            Thread.Sleep(16);
            if (String.IsNullOrEmpty(LoginInfo.Password) || LoginInfo.Password.Equals(dummyPass))
                JoinChannels();

            pingTimer.Change(0, pingInterval);
        }
        void JoinChannels()
        {
            foreach (String channel in LoginInfo.Channels)
                Channels.Join(
                    "#" + channel.Replace("#","").ToLower()
                );
        }
        protected override void OnPongReceived(IrcPingOrPongReceivedEventArgs e)
        {
            base.OnPongReceived(e);
            noPongTimer.Change(pingInterval, Timeout.Infinite);
        }
        protected override void OnRegistered(EventArgs e)
        {
            base.OnRegistered(e);

            LocalUser.JoinedChannel += LocalUser_JoinedChannel;
            LocalUser.LeftChannel += LocalUser_LeftChannel;
            LocalUser.NoticeReceived += LocalUser_NoticeReceived;
            LocalUser.MessageReceived += LocalUser_MessageReceived;

            JoinChannels();

        }
        protected override void OnRawMessageReceived(IrcRawMessageEventArgs e)
        {
            base.OnRawMessageReceived(e);
            switch( e.Message.Command)
            {
                case "PRIVMSG":
                    if(e.Message.Parameters.Count >= 2)
                    {
                        if (MessageReceived != null)
                        {
                            MessageReceived(this, new ChatServiceEventArgs()
                            {
                                Message = new ChatMessage()
                                {
                                    Text = e.Message.Parameters[1],
                                    Channel = e.Message.Parameters[0],
                                    FromUserName = e.Message.Source.Name,
                                    TimeStamp = DateTime.Now.ToShortTimeString()
                                }
                            });
                        }
                    }
                    break;
                default:

                    break;

            }

        }
        protected override void OnDisconnected(EventArgs e)
        {
            Debug.Print("Disconnect event from IRC");
            if (!IsStopping)
                Restart();

            base.OnDisconnected(e);            
        }
        protected override void OnError(IrcErrorEventArgs e)
        {
            Debug.Print("Error: {0}", e.Error);
            Restart();
            base.OnError(e);
        }
        void LocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            Debug.Print("Message:{0}",e.Text);
            if (MessageReceived != null)
                MessageReceived(this, new ChatServiceEventArgs() {
                    Message = new ChatMessage() { Text = e.Text, FromUserName = e.Source.Name }
                });
        }

        void LocalUser_NoticeReceived(object sender, IrcMessageEventArgs e)
        {
            Debug.Print("Notice:{0}", e.Text);
        }

        void LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
        {
            
        }

        void LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
        {
            
        }

        public bool SendMessage(String channel, ChatMessage message)
        {
            channel = "#" + channel.Replace("#", "").ToLower();
            var ircChannel = Channels.FirstOrDefault(ch => ch.Name.Equals(channel, StringComparison.InvariantCultureIgnoreCase));
            if( null != ircChannel)
            {
                LocalUser.SendMessage(ircChannel, message.Text);
                return true;
            }
            return false;
        }

    }
}
