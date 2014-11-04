using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UB.Utils;

namespace UB.Model
{
    class GamingLiveChat : ChatBase, IStreamTopic
    {
        private WebClientBase loginWebClient = new WebClientBase();   
        private GamingLiveGameList jsonGames;
        private object lockSearch = new object();
        private object pollerLock = new object();
        public GamingLiveChat(ChatConfig config) : base(config)
        {
            CreateChannel = () => { return new GamingLiveChannel(this); };

            NickName = "__$anonymous";

            ContentParsers.Add(MessageParser.ParseURLs);
            ContentParsers.Add(MessageParser.ParseSimpleImageTags);

            Info = new StreamInfo()
            {
                HasDescription = false,
                HasGame = true,
                CurrentGame = new Game(),
                HasTopic = true,
                ChatName = Config.ChatName,
            };

            Games = new ObservableCollection<Game>();
        }

        #region IChat
        public override bool Login()
        {
            try
            {
                if (!LoginWithToken())
                {
                    if (!LoginWithUsername())
                    {
                        Status.IsLoginFailed = true;
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteInfo("Gaminglive authorization exception {0}", e.Message);
                return false;
            }

            return true;
        }
        public bool LoginWithUsername()
        {
            var userName = Config.GetParameterValue("Username") as string;
            var password = Config.GetParameterValue("Password") as string;

            if (String.IsNullOrWhiteSpace(userName) || String.IsNullOrWhiteSpace(password))
            {
                IsAnonymous = true;
                return true;
            }
            NickName = userName;

            var authString = String.Format(@"{{""email"":""{0}"",""password"":""{1}""}}", userName, password);

            SetCommonHeaders();
            var authToken = this.With(x => loginWebClient.Upload("https://api.gaminglive.tv/auth/session", authString))
                                .With(x => JToken.Parse(x))
                                .With(x => x.Value<string>("authToken"));

            if (authToken == null)
            {
                Log.WriteError("Login to gaminglive.tv failed.");
                return false;
            }
            else
            {
                IsAnonymous = false;
                Config.SetParameterValue("AuthToken", authToken);
                Config.SetParameterValue("AuthTokenCredentials", userName + password);
                return true;

            }
        }
        public bool LoginWithToken()
        {
            var authToken = (string)Config.GetParameterValue("AuthToken");
            var userName = Config.GetParameterValue("Username") as string;
            var password = Config.GetParameterValue("Password") as string;
            var tokenCredentials = Config.GetParameterValue("AuthTokenCredentials") as string;

            if (tokenCredentials != userName + password)
                return false;


            if (String.IsNullOrEmpty(userName))
            {
                IsAnonymous = true;
                return true;
            }

            if (String.IsNullOrWhiteSpace(authToken))
                return false;

            SetCommonHeaders();
            loginWebClient.Headers["Auth-Token"] = authToken;

            var response = this.With(x => loginWebClient.Download("https://api.gaminglive.tv/auth/me"))
                .With(x => String.IsNullOrWhiteSpace(x) ? null : x)
                .With(x => JToken.Parse(x));

            if (response == null)
            {
                return false;
            }

            var isOk = response.Value<bool>("ok");
            NickName = (string)response.Value<dynamic>("user").login;

            if (isOk && !String.IsNullOrWhiteSpace(NickName))
            {
                IsAnonymous = false;
                return true;
            }

            Config.SetParameterValue("AuthToken", String.Empty);
            return false;
        }
        private void SetCommonHeaders()
        {
            loginWebClient.Headers["Content-Type"] = @"application/json;charset=UTF-8";
            loginWebClient.Headers["Accept"] = @"application/json, text/plain, */*";
            loginWebClient.Headers["Accept-Encoding"] = "gzip,deflate";
        }

        #endregion
        #region IStreamTopic
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
            lock (lockSearch)
            {
                Games.Clear();
                jsonGames.games.Where(game => game.name.ToLower().StartsWith(gameName.ToLower())).Select(game => new Game() { Id = game.id, Name = game.name }).ToList().ForEach(game => Games.Add(game));
                if (callback != null)
                    UI.Dispatch(() => callback());
            }
        }

        JToken GetLiveStreamInfo()
        {
            var getUrl = @"https://api.gaminglive.tv/channels/{0}?authToken={1}";
            var userName = Config.GetParameterValue("Username") as string;
            var authToken = Config.GetParameterValue("AuthToken") as string;
            loginWebClient.ContentType = ContentType.JsonUTF8;
            return this.With(x => loginWebClient.Download(String.Format(getUrl, HttpUtility.UrlEncode(userName.ToLower()), authToken)))
                            .With(x => JToken.Parse(x));

        }
        public void GetTopic()
        {
            if (!Status.IsLoggedIn)
                return;

            GetGameList();

            Task.Factory.StartNew(() =>
            {
                var jsonInfo = GetLiveStreamInfo();

                if (jsonInfo == null)
                    return;

                Info.Topic = jsonInfo["name"].ToObject<string>();
                var game = jsonInfo["game"];
                if (game != null)
                {
                    Info.CurrentGame.Name = game["name"].ToObject<string>();
                    Info.CurrentGame.Id = game["id"].ToObject<string>();
                }

                Info.CanBeRead = true;
                Info.CanBeChanged = true;

                if (StreamTopicAcquired != null)
                    UI.Dispatch(() => StreamTopicAcquired());
            });
        }
        private void GetGameList()
        {
            Task.Factory.StartNew(() =>
            {
                jsonGames = Json.DeserializeUrl<GamingLiveGameList>(@"http://api.gaminglive.tv/games");
                if (jsonGames == null)
                    return;
            });
        }
        public void SetTopic()
        {
            if (!Status.IsLoggedIn)
                return;

            var userName = Config.GetParameterValue("Username") as string;
            var authToken = Config.GetParameterValue("AuthToken") as string;
            var gameId = this.With(x => jsonGames.games.FirstOrDefault(game => game.name.Equals(Info.CurrentGame.Name, StringComparison.InvariantCultureIgnoreCase)))
                            .With(x => x.id);

            var jsonInfo = new GamingLiveChannelUpdate
            {
                slug = userName.ToLower(),
                owner = userName.ToLower(),
                name = Info.Topic,
                authToken = authToken,
                gameId = gameId,
            };



            Json.SerializeToStream<GamingLiveChannelUpdate>(jsonInfo, (stream) =>
            {
                var putUrl = @"https://api.gaminglive.tv/channels/";
                if (null == loginWebClient.PatchStream(putUrl, stream))
                {
                    //Authentication data expired ? Let's login again
                    LoginWithUsername();
                    jsonInfo.authToken = Config.GetParameterValue("AuthToken") as string;

                    Json.SerializeToStream<GamingLiveChannelUpdate>(jsonInfo, (streamRetry) =>
                    {
                        loginWebClient.PatchStream(putUrl, streamRetry);
                    });
                }
            });
        }

        public Action StreamTopicAcquired
        {
            get;
            set;

        }      
        #endregion

    }

    public class GamingLiveChannel : ChatChannelBase
    {
        private Timer pingTimer;
        private const int pingInterval = 60000;
        private WebSocketBase webSocket;
        private WebSocketBase secondWebSocket;
        private object pollerLock = new object();
        private WebPoller statsPoller;
        private bool isJoined = false;
        public GamingLiveChannel(IChat chat)
        {
            Chat = chat;
        }
        public override void Join(Action<IChatChannel> callback, string channel)
        {
            ChannelName = "#" + channel.Replace("#", "");
            
            SetupStatsWatcher();

            JoinCallback = callback;

            webSocket = new WebSocketBase();
            webSocket.Host = "54.76.144.150";

            webSocket.Origin = "http://www.gaminglive.tv";
            webSocket.Path = String.Format("/chat/{0}?nick={1}&authToken={2}", 
                ChannelName.Replace("#", ""), 
                Chat.IsAnonymous ? "__$anonymous" : Chat.NickName, 
                Chat.IsAnonymous ? "__$anonymous" : Chat.Config.GetParameterValue("AuthToken").ToString());

            webSocket.DisconnectHandler = () =>
            {
                if (LeaveCallback != null)
                    LeaveCallback(this);
            };
            webSocket.ReceiveMessageHandler = ReadRawMessage;
            webSocket.Connect();

            pingTimer = new Timer((sender) =>
            {
                secondWebSocket = new WebSocketBase();
                secondWebSocket.Host = webSocket.Host;
                secondWebSocket.Origin = webSocket.Origin;
                secondWebSocket.Path = String.Format("/chat/{0}?nick={1}&authToken={2}", ChannelName.Replace("#", ""), "__$anonymous", "__$anonymous");
                secondWebSocket.Connect();
                Thread.Sleep(5000);
                secondWebSocket.Disconnect();

            }, this, 500, pingInterval);

        }
        private void ReadRawMessage(string rawMessage)        
        {
            if (!Chat.Status.IsConnected)
            {
                Chat.Status.IsStarting = false;
                Chat.Status.IsConnected = true;
            }

            if (!isJoined && JoinCallback != null)
            {
                JoinCallback(this);
                isJoined = true;
            }
            
            if( !String.IsNullOrWhiteSpace(rawMessage))
            {
                if( !Chat.IsAnonymous )
                    Chat.Status.IsLoggedIn = true;

                var json = JToken.Parse(rawMessage);
                var nickName = (string)json.Value<dynamic>("user").nick;
                var text = json.Value<string>("message");
                var memberType = json.Value<string>("mtype");
                if (memberType != null && memberType.Equals("BOT", StringComparison.InvariantCultureIgnoreCase))
                {
                    Log.WriteInfo("Gaminglive bot greeting from {0} on {1}", nickName, ChannelName);
                    return;
                }

                if (String.IsNullOrWhiteSpace(nickName) || String.IsNullOrWhiteSpace(text))
                    return;

                ChannelStats.MessagesCount++;
                Chat.UpdateStats();

                if(ReadMessage != null)
                    ReadMessage(new ChatMessage() { 
                        Channel = ChannelName,
                        ChatIconURL = Chat.IconURL,
                        ChatName = Chat.ChatName,
                        FromUserName = nickName,
                        HighlyImportant = false,
                        IsSentByMe = false,
                        Text = text
                    });

            }
        }
        public override void Leave()
        {
            Log.WriteInfo("Gaminglive leaving {0}", ChannelName);
            webSocket.Disconnect();
        }

        public override void SendMessage( ChatMessage message )
        {
            if (Chat.IsAnonymous)
                return;

            dynamic jsonMessage = new { message = message.Text, color = "orange" };
            webSocket.Send(JsonConvert.SerializeObject(jsonMessage));
        }

        public override void SetupStatsWatcher()
        {
            statsPoller = new WebPoller()
            {
                Id = ChannelName,
                Uri = new Uri(String.Format(@"http://api.gaminglive.tv/channels/{0}", ChannelName.Replace("#", ""))),
            };

            statsPoller.ReadStream = (stream) =>
            {
                lock (pollerLock)
                {
                    var channelInfo = Json.DeserializeStream<GamingLiveChannelStats>(stream);
                    statsPoller.LastValue = channelInfo;
                    int viewers = 0;
                    if (channelInfo != null && channelInfo.state != null )
                        viewers = channelInfo.state.viewers;

                    ChannelStats.ViewersCount = viewers;
                    Chat.UpdateStats();
                }
            };
            statsPoller.Start();
        }
    }
    #region Gaminglive json classes

    public class GamingLiveChannelUpdate
    {
        public string slug { get; set; }
        public string owner { get; set; }
        public string name { get; set; }
        public string gameId { get; set; }
        public string authToken { get; set; }
    }

    public class GamingLiveGame
    {
        public string id { get; set; }
        public string name { get; set; }
        public string miniImg { get; set; }
        public string largeImg { get; set; }
    }

    public class GamingLiveStream
    {
        public string rootUrl { get; set; }
        public List<string> qualities { get; set; }
    }

    public class GamingLiveState
    {
        public int viewers { get; set; }
        public string thumbnailUrl { get; set; }
        public GamingLiveStream stream { get; set; }
    }

    public class GamingLiveChannelStats
    {
        public string slug { get; set; }
        public string name { get; set; }
        public string owner { get; set; }
        public string offlineImg { get; set; }
        public GamingLiveGame game { get; set; }
        public GamingLiveState state { get; set; }
        public int views { get; set; }
        public int followers { get; set; }
    }

    public class GamingLiveGameList
    {
        public List<GamingLiveGame> games { get; set; }
    }
    #endregion
}
