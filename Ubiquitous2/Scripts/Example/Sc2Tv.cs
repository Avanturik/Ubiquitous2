using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Model;
using UB.Utils;


public class Sc2TvScript : IScript
{
    //Chat config
    public object OnConfigRequest()
    {
        return new ChatConfig()
        {
            ChatName = "Sc2tv.ru", // Unique chat name           
            IconURL = AppDomain.CurrentDomain.GetData("DataDirectory") + @"\Scripts\Example\Sc2Tv.png", // Icon path
        };
    }
    //Chat object creation
    public object OnObjectRequest(object config)
    {
        //Return instance
        return new Sc2TvChat(config as ChatConfig);
    }
}

//Chat implementation
public class Sc2TvChat : ChatBase
{
    public Sc2TvChat(ChatConfig config)
        : base(config)
    {
        EmoticonUrl = "http://chat.sc2tv.ru/js/smiles.js";
        EmoticonFallbackUrl = AppDomain.CurrentDomain.GetData("DataDirectory") + @"\Scripts\Example\Sc2TvSmilesFallback.js";

        CreateChannel = () => { return new Sc2TvChannel(this); };

        ReceiveOwnMessages = false;

        //ContentParsers.Add(MessageParser.ParseURLs);
        //ContentParsers.Add(MessageParser.ParseEmoticons);
    }

    public override void DownloadEmoticons(string url)
    {

    }

    public override bool Login()
    {
        IsAnonymous = true;
        return true;
    }

}

//Chat channel implementation
public class Sc2TvChannel : ChatChannelBase
{
    private object pollerLock = new object();
    private object chatLock = new object();
    private WebPoller chatPoller;
    private string channelId = null;
    private string lastTime = null;
    private Random random = new Random();

    public Sc2TvChannel(IChat chat)
    {
        Log.WriteInfo("Initializing sc2tv");
        Chat = chat;
    }

    public override void Leave()
    {
        Log.WriteInfo("Sc2Tv leaving {0}", ChannelName);

        if (chatPoller != null)
            chatPoller.Stop();
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
        Log.WriteInfo("Joining sc2tv");

        ChannelName = "#" + channel.Replace("#", "");
        if (String.IsNullOrWhiteSpace(channel))
            return;
        GetStreamId();
        JoinCallback = callback;
    }
    public void GetStreamId()
    {
        channelId = "226459";
    }
    public void SetupPollers()
    {
        Log.WriteInfo("Setup sc2tv pollers");

        if (!String.IsNullOrWhiteSpace(channelId))
        {
            #region Chatpoller
            chatPoller = new WebPoller()
            {
                Id = ChannelName,
                Uri = new Uri(String.Format(@"http://chat.sc2tv.ru/memfs/channel-{0}.json", channelId)),
                IsLongPoll = false,
                Interval = 5000,
                TimeoutMs = 10000,
                IsAnonymous = false,
                KeepAlive = false,
                IsTimeStamped = true,
            };
            chatPoller.ReadStream = (stream) =>
            {
                if( stream == null )
                    return;

                lock (chatLock)
                {
                    var messagesJson = Json.DeserializeStream<dynamic>(stream);
                    if (messagesJson == null)
                        return;

                    Json.ParseArray(messagesJson.messages);

                    //if (!String.IsNullOrWhiteSpace(chatJson))
                    //{
                    //    var generalInfo = JObject.Parse(chatJson);
                    //    if (generalInfo != null)
                    //    {
                    //        if (String.IsNullOrEmpty(generalInfo["latest_time"].ToString()) ||
                    //            (lastTime != null &&
                    //            generalInfo["latest_time"].ToString() == lastTime))
                    //            return;

                    //        long intLastTime;
                    //        long.TryParse(lastTime, out intLastTime);
                    //        lastTime = generalInfo["latest_time"].ToString();
                    //        var comments = JArray.Parse(generalInfo["comments"].ToString());

                    //        foreach (var comment in comments)
                    //        {
                    //            var time_created = comment["time_created"].ToObject<long>();

                    //            if (time_created <= intLastTime)
                    //                continue;

                    //            var author_name = comment["author_name"].ToString();
                    //            var comment_text = comment["comment"].ToString();


                    //            if (String.IsNullOrEmpty(author_name) ||
                    //                String.IsNullOrEmpty(comment_text))
                    //                return;

                    //            if (ReadMessage != null)
                    //                ReadMessage(new ChatMessage()
                    //                {
                    //                    Channel = ChannelName,
                    //                    ChatIconURL = Chat.IconURL,
                    //                    ChatName = Chat.ChatName,
                    //                    FromUserName = author_name,
                    //                    HighlyImportant = false,
                    //                    IsSentByMe = false,
                    //                    Text = comment_text,
                    //                });


                    //            Chat.UpdateStats();
                    //            ChannelStats.MessagesCount++;
                    //        }
                    //    }
                    //}
                }
            };
            chatPoller.Start();
            #endregion
            JoinCallback(this);
        }
    }
}


