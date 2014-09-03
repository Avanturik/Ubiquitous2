using System;
using System.Collections.Generic;
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
        private bool isConnecting = true;
        private Dictionary<String, Action<IrcRawMessageEventArgs>> rawMessageHandlers;

        private Random random { get; set; }
        public IRCChatBase( IRCLoginInfo info )
        {
            loginInfo = info;
            if (!info.Channels.Any(ch => ch.Equals(loginInfo.UserName, StringComparison.InvariantCultureIgnoreCase)))
                info.Channels = info.Channels.Union(new String[] { loginInfo.UserName.ToLower() }).ToArray();

            for (int i = 0; i < loginInfo.Channels.Length; i++)
            {
                loginInfo.Channels[i] = "#" + loginInfo.Channels[i].Replace("#", "");
            }

            if (String.IsNullOrEmpty(LoginInfo.HostName))
                throw new Exception("Hostname must be specified!");
            
            if (String.IsNullOrEmpty(LoginInfo.UserName))
                throw new Exception("Username must be specified!");

            random = new Random();

            rawMessageHandlers = new Dictionary<string, Action<IrcRawMessageEventArgs>>()
            {
                {"PRIVMSG", ReadPrivateMessage}
            };

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
            isConnecting = true;
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
        private void ReadPrivateMessage(IrcRawMessageEventArgs e)
        {
            if (e.Message.Parameters.Count >= 2)
            {
                if (MessageReceived != null)
                {
                    var message = new ChatMessage()
                        {
                            Text = e.Message.Parameters[1],
                            Channel = e.Message.Parameters[0],
                            FromUserName = e.Message.Source.Name,
                            TimeStamp = DateTime.Now.ToShortTimeString()
                        };
                    
                    if( loginInfo.Channels.Contains(message.Channel) )
                        MessageReceived(this, new ChatServiceEventArgs()
                        {
                            Message = message
                        });
                }
            }
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

        void JoinChannels()
        {
            pingTimer.Change(0, pingInterval);
            foreach (String channel in LoginInfo.Channels)
                Channels.Join( channel );
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
        }
        protected override void OnRawMessageReceived(IrcRawMessageEventArgs e)
        {
            base.OnRawMessageReceived(e);
            
            if (rawMessageHandlers.ContainsKey(e.Message.Command))
                rawMessageHandlers[e.Message.Command](e);

            if (isConnecting)
            {
                isConnecting = false;
                JoinChannels();
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

        void LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
        {
            // TODO: IRC leave message handler
        }

        void LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
        {
            // TODO: IRC join message handler
            
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


        public virtual String ChatName { get { return String.Empty; } }
        public virtual String IconURL { get { return String.Empty; } }
        public bool Enabled { get; set; }

    }
}
