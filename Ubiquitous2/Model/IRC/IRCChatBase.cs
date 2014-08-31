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

        private const String dummyPass = "!@$#@";
        private IRCLoginInfo loginInfo { get; set; }
        private Random random { get; set; }
        public IRCChatBase( IRCLoginInfo info )
        {
            loginInfo = info;

            if (String.IsNullOrEmpty(loginInfo.HostName))
                throw new Exception("Hostname must be specified!");
            
            if (String.IsNullOrEmpty(loginInfo.UserName))
                throw new Exception("Username must be specified!");

            random = new Random();

        }

        public bool Start()
        {
            Connect();
            return true;
        }


        public bool Stop()
        {
            Disconnect();
            return true;
        }

        private void Connect()
        {
            Utils.Net.DemandTCPPermission();
            GetServerList((hostList) => {
                var hostCount = hostList.AddressList.Count();
                if (hostCount <= 0)
                    throw new Exception("All servers are down. Domain:" + loginInfo.HostName);
                
                Connect(hostList.AddressList[random.Next(0,hostCount)], loginInfo.Port, false, new IrcUserRegistrationInfo()
                { 
                    UserName = loginInfo.UserName,
                    NickName = loginInfo.UserName,
                    RealName = loginInfo.RealName,
                    Password = String.IsNullOrEmpty(loginInfo.Password) ? dummyPass : loginInfo.Password
                });
            });
        }

        private void GetServerList(Action<IPHostEntry> callback)
        {
            Utils.Net.TestTCPPort(loginInfo.HostName, loginInfo.Port, (hosts, error) =>
            {
                if (error != null)
                    throw error;
                callback(hosts);
            });
        }
        protected override void OnConnected(EventArgs e)
        {
            base.OnConnected(e);
            Thread.Sleep(500);
            if (String.IsNullOrEmpty(loginInfo.Password) || loginInfo.Equals(dummyPass))
                JoinChannel();

        }
        void JoinChannel()
        {
            var channel = (String.IsNullOrEmpty(loginInfo.Channel) ? "#" + loginInfo.UserName : loginInfo.Channel.Contains('#') ? loginInfo.Channel : "#" + loginInfo.Channel).ToLower();
            Channels.Join(channel);
            //test multiple channels
            Channels.Join("#nightblue3");
            Channels.Join("#herdyn");
            Channels.Join("#starladder1");
        }
        protected override void OnRegistered(EventArgs e)
        {
            base.OnRegistered(e);

            LocalUser.JoinedChannel += LocalUser_JoinedChannel;
            LocalUser.LeftChannel += LocalUser_LeftChannel;
            LocalUser.NoticeReceived += LocalUser_NoticeReceived;
            LocalUser.MessageReceived += LocalUser_MessageReceived;

            JoinChannel();

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
            Stop();
            Start();
            base.OnDisconnected(e);
        }
        protected override void OnError(IrcErrorEventArgs e)
        {
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

    }
}
