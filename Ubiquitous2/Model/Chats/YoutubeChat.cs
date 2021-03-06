﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UB.Utils;

namespace UB.Model
{
    public class YoutubeChat : ChatBase
    {
        private object iconParseLock = new object();
        private WebClientBase webClient = new WebClientBase();
        public YoutubeChat(ChatConfig config)
            : base(config)
        {
            EmoticonFallbackUrl = @"Content\youtube-emoji.css";
            EmoticonUrl = "https://s.ytimg.com/yts/cssbin/www-livestreaming_chat_emoji-webp-vflZqACWb.css";

            CreateChannel = () => { return new YoutubeChannel(this); };

            ReceiveOwnMessages = true;

            //ContentParsers.Add(MessageParser.ParseURLs);
            ContentParsers.Add(MessageParser.ParseEmoji);
        }

        public override void DownloadEmoticons(string url)
        {
            //image: ssl.gstatic.com/chat/emoji/emoji20png-61089cf406d77f75e149071e74d8f714.png
            lock (iconParseLock)
            {
                if (IsFallbackEmoticons && IsWebEmoticons)
                    return;

                var list = new List<Emoticon>();
                if (Emoticons == null)
                    Emoticons = new List<Emoticon>();


                var content = webClient.Download(url);
                if (String.IsNullOrWhiteSpace(content))
                    return;
                MatchCollection matches = Regex.Matches(content, @"yt-emoji-icon.yt-emoji-([0-9,a-z]+)\s*{(.*?)}", RegexOptions.IgnoreCase);

                if (matches.Count <= 0)
                {
                    Log.WriteError("Unable to get Youtube emoticons!");
                }
                else
                {
                    string originalUrl = null;
                    foreach (Match match in matches)
                    {
                        if (match.Groups.Count >= 2)
                        {
                            var smileName = match.Groups[1].Value;
                            var cssClassDefinition = match.Groups[2].Value;

                            var background = Css.GetBackground(cssClassDefinition);

                            if (background != null && !String.IsNullOrWhiteSpace(background.url) && background.width > 0 && background.height > 0)
                            {
                                originalUrl = String.Format("http://{0}", background.url.Replace("//", ""));
                                var modifiedUrl = String.Format(@"/ubiquitous/cache?ubx={0}&uby={1}&ubw={2}&ubh={3}&uburl={4}",
                                    background.x, background.y, background.width, background.height, HttpUtility.UrlEncode(originalUrl));

                                list.Add(new Emoticon(String.Format(@"\u{0}", smileName),
                                    modifiedUrl,
                                    background.width,
                                    background.height
                                ));
                            }
                        }
                    }
                    if (list.Count > 0)
                    {
                        Uri uri;
                        if (!String.IsNullOrWhiteSpace(originalUrl) && Uri.TryCreate(originalUrl, UriKind.Absolute, out uri))
                        {

                            var imageDataService = SimpleIoc.Default.GetInstance<IImageDataSource>();

                            using (var memoryStream = webClient.DownloadToMemoryStream(originalUrl))
                            {
                                if (memoryStream == null)
                                {
                                    Log.WriteError("Web Youtube emoticons aren't available!");
                                    using( var memStream = webClient.DownloadToMemoryStream(@"Content\youtubeemoji.png"))
                                    {
                                        if (memStream != null)
                                            imageDataService.AddImage(uri, memStream);
                                    }
                                }
                            }
                        }
                        Emoticons = list;
                        if (IsFallbackEmoticons)
                            IsWebEmoticons = true;

                        IsFallbackEmoticons = true;
                    }
                }
            }

        }

        public override bool Login()
        {
            return true;
        }

    }

    public class YoutubeChannel : ChatChannelBase
    {
        private object pollerLock = new object();
        private object chatLock = new object();
        private WebPoller statsPoller;
        private WebPoller chatPoller;
        private string videoId = null;
        private string lastTime = null;
        private WebClientBase webClient;
        private Random random = new Random();
        private Timer checkTimer;

        public YoutubeChannel(IChat chat)
        {
            Chat = chat;
            InitWebClient();
        }

        public override void Leave()
        {
            Log.WriteInfo("Youtubechannel leaving {0}", ChannelName);
            
            if( checkTimer != null)
                checkTimer.Change(Timeout.Infinite, Timeout.Infinite);
            if( chatPoller != null )
                chatPoller.Stop();
            if( statsPoller != null )
                statsPoller.Stop();
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
            if (String.IsNullOrWhiteSpace(channel))
                return;
            GetStreamId();
            JoinCallback = callback;
        }
        private void InitWebClient()
        {
            webClient = new WebClientBase();
            webClient.Timeout = 30000;
            webClient.KeepAlive = false;
            webClient.IsAnonymous = true;
            webClient.ErrorHandler = (error) => {
                InitWebClient();
            };
        }
        public void GetStreamId()
        {
            checkTimer = new Timer((obj) =>
            {
                var youtubeChannel = obj as YoutubeChannel;
                var channelUrl = webClient.GetRedirectUrl(String.Format(@"https://www.youtube.com/user/{0}/live", ChannelName.Replace("#", "")));
                var id = Re.GetSubString(channelUrl, @"v=([^&]+)");
                if (!String.IsNullOrWhiteSpace(id) && videoId != id )
                {
                    youtubeChannel.videoId = id;
                    youtubeChannel.checkTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    if( chatPoller != null )
                        chatPoller.Stop();
                    if( statsPoller != null)
                        statsPoller.Stop();

                    youtubeChannel.SetupPollers();
                }
            }, this, 0, random.Next(50000,60000));
        }
        public void SetupPollers()
        {

            if( !String.IsNullOrWhiteSpace(videoId) )
            {
                #region Chatpoller
                int rc = 0;
                int poll_delay = 30000;
                lastTime = Time.UnixTimestamp().ToString();
                chatPoller = new WebPoller()
                {
                    Id = ChannelName,
                    Uri = new Uri(String.Format(@"https://www.youtube.com/live_comments?action_get_comments=1&video_id={0}&lt={1}&rc={2}&pd={3}&format=json",
                        videoId,
                        lastTime,
                        rc,
                        poll_delay)),
                    IsLongPoll = true,     
                    Interval = 30000,
                    TimeoutMs = 30000,
                    IsAnonymous = true,
                    KeepAlive = false,
                    Delay = poll_delay,
                    IsTimeStamped = false,
                };
                chatPoller.ReadString = (text) =>
                {
                    if (String.IsNullOrWhiteSpace(text))
                        return;

                    lock (chatLock)
                    {
                        rc++;
                        if (rc > 5)
                            rc = 0;

                        chatPoller.Uri = new Uri(String.Format(@"https://www.youtube.com/live_comments?action_get_comments=1&video_id={0}&lt={1}&rc={2}&pd={3}&format=json",
                            videoId,
                            lastTime,
                            rc,
                            poll_delay));

                        chatPoller.Interval = random.Next(20000, 30000);
                        chatPoller.TimeoutMs = random.Next(30000, 35000);
                        chatPoller.Delay = random.Next(20000, 30000);

                        if (String.IsNullOrWhiteSpace(text))
                            return;

                        string chatJson = this.With(x => XDocument.Parse(text))
                        .With(x => x.Root.Element("html_content"))
                        .With(x => (string)x.Value);

                        if (!String.IsNullOrWhiteSpace(chatJson))
                        {
                            var generalInfo = JObject.Parse(chatJson);
                            if (generalInfo != null)
                            {
                                if (String.IsNullOrEmpty(generalInfo["latest_time"].ToString()) ||
                                    (lastTime != null &&
                                    generalInfo["latest_time"].ToString() == lastTime))
                                    return;

                                long intLastTime;
                                long.TryParse(lastTime, out intLastTime);
                                lastTime = generalInfo["latest_time"].ToString();
                                var comments = JArray.Parse(generalInfo["comments"].ToString());

                                foreach (var comment in comments)
                                {
                                    var time_created = comment["time_created"].ToObject<long>();

                                    if (time_created <= intLastTime)
                                        continue;

                                    var author_name = comment["author_name"].ToString();
                                    var comment_text = comment["comment"].ToString();

                                                                     
                                    if (String.IsNullOrEmpty(author_name) ||
                                        String.IsNullOrEmpty(comment_text))
                                        return;

                                    if (ReadMessage != null)
                                        ReadMessage(new ChatMessage()
                                        {
                                            Channel = ChannelName,
                                            ChatIconURL = Chat.IconURL,
                                            ChatName = Chat.ChatName,
                                            FromUserName = author_name,
                                            HighlyImportant = false,
                                            IsSentByMe = false,
                                            Text = comment_text,
                                        });


                                    Chat.UpdateStats();
                                    ChannelStats.MessagesCount++;
                                }
                            }
                        }


                    }
                };
                chatPoller.Start();
                #endregion
                #region Statspoller
                statsPoller = new WebPoller()
                {
                    Id = ChannelName,
                    Uri = new Uri(String.Format(@"http://www.youtube.com/live_stats?v={0}&t={1}", videoId, Time.UnixTimestamp().ToString())),
                    IsLongPoll = false,
                    Interval = 60000,
                };

                statsPoller.ReadString = (text) =>
                {
                    if (String.IsNullOrWhiteSpace(text))
                        return;

                    lock (pollerLock)
                    {
                        Int32 viewers = 0;
                        Int32.TryParse(text, out viewers);
                        ChannelStats.ViewersCount = viewers;
                        Chat.UpdateStats();
                    }
                };
                statsPoller.Start();                
                #endregion
                JoinCallback(this);
            }
        }
    }
}
