using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UB.Model
{
    public class DummyChat : ChatBase
    {
        public DummyChat(ChatConfig config)
            : base(config)
        {
            EmoticonFallbackUrl = @"Content\dummy.html";
            EmoticonUrl = "http://dummy.com";

            CreateChannel = () => { return new DummyChannel(this); };

            ReceiveOwnMessages = true;

            //ContentParsers.Add(MessageParser.ParseURLs);
            //ContentParsers.Add(MessageParser.ParseEmoticons);

            //IStreamTopic 

            //Info = new StreamInfo()            
            //{
            //    HasDescription = false,
            //    HasGame = true,
            //    HasTopic = true,
            //    ChatName = Config.ChatName,
            //};


            //Games = new ObservableCollection<Game>();

        }

        public override void DownloadEmoticons(string url)
        {

        }

        public override bool Login()
        {
            throw new NotImplementedException();
        }

    }

    public class DummyChannel : ChatChannelBase
    {
        private object pollerLock = new object();
        private WebPoller statsPoller;
        private Random random = new Random();

        public DummyChannel(IChat chat)
        {
            Chat = chat;
        }

        public override void Leave()
        {
            Log.WriteInfo("Dummychannel leaving {0}", ChannelName);
            //Disconnect channel
        }

        public override void SendMessage(ChatMessage message)
        {
            if (Chat.IsAnonymous || String.IsNullOrWhiteSpace(message.Channel) ||
                String.IsNullOrWhiteSpace(message.FromUserName) ||
                String.IsNullOrWhiteSpace(message.Text))
                return;

            //Send message
        }

        public override void Join(Action<IChatChannel> callback, string channel)
        {
            ChannelName = "#" + channel.Replace("#", "");

            SetupStatsWatcher();

            if (String.IsNullOrWhiteSpace(channel))
                return;

            //LeaveCallback call on disconnect

            JoinCallback = callback;

            //Connect to chat

        }

        public override void SetupStatsWatcher()
        {
            statsPoller = new WebPoller()
            {
                Id = ChannelName,
                Uri = new Uri(String.Format(@"http://dummy.com/channel={0}", ChannelName.Replace("#", ""))),
            };

            statsPoller.ReadString = (stream) =>
            {
                lock (pollerLock)
                {
                    //var channelInfo = JsonConvert.DeserializeObject<ChannelStatus>(stream);
                    //statsPoller.LastValue = channelInfo;
                    //int viewers = 0;
                    //if (channelInfo != null && int.TryParse(channelInfo.spectators, out viewers))
                    //{
                    //    ChannelStats.ViewersCount = viewers;
                    //    Chat.UpdateStats();
                    //}
                }
            };
            statsPoller.Start();
        }
    }
}
