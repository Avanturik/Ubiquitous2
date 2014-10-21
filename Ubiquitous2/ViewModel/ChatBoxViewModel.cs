using System;
using System.Collections.ObjectModel;
using System.Web;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using System.Linq;
using UB.Model;
using UB.Utils;
using UB.Properties;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using Newtonsoft.Json;
using GalaSoft.MvvmLight.Command;

namespace UB.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ChatBoxViewModel : ViewModelBase
    {
        public event EventHandler<EventArgs> MessageAdded;
        public event EventHandler<EventArgs> MessageSent;
        private string lastSettings = null;
        private bool firstMessageReceived = false;
        private object lockReadMessages = new object();

        private IService imageService;
        IChatDataService _dataService;
        IGeneralDataService _generalDataService;
        ISettingsDataService _settingsDataService;

        public ChatBoxViewModel()
        {

        }
        /// <summary>
        /// Initializes a new instance of the ChatBoxViewModel class.
        /// </summary>        
        [PreferredConstructor]
        public ChatBoxViewModel(IChatDataService dataService, IGeneralDataService generalDataService, ISettingsDataService settingsDataService)
        {
            _dataService = dataService;
            _generalDataService = generalDataService;
            _settingsDataService = settingsDataService;
            Initialize();
        }

        private void Initialize()
        {
            //Test data
            for (var i = 0; i < 3; i++)
            {
                _dataService.GetRandomMessage(
                    (item, error) =>
                    {
                        if (error != null)
                        {
                            // Report error here
                            return;
                        }
                        item.ChatIconURL = Icons.MainIcon;
                        item.Text += " http://asdf.com";
                        item.Text = MessageParser.ConvertUrlsToLinks(item.Text);
                        //item.Text += @" " + Html.CreateImageTag(Icons.MainHeadsetIcon, 24, 18);
                        Messages.Add(new ChatMessageViewModel(item));

                    });
            }


            if (IsInDesignMode)
            {
                firstMessageReceived = true;
                return;
            }



            _settingsDataService.GetAppSettings((config) => {
                AppConfig = config;
            });


            _dataService.ReadMessages((messages, error) =>
            {

                lock (lockReadMessages)
                {
                    AddMessages(messages);
                    return true;
                }

            });

            MessengerInstance.Register<bool>(this, "MessageSent", msg =>
                {
                    if (MessageSent != null)
                        MessageSent(this, EventArgs.Empty);
                });

            MessengerInstance.Register<bool>(this, "EnableAutoScroll", msg =>
            {
                EnableAutoScroll = msg;
            });           

            if (_generalDataService.Services == null)
                _generalDataService.Start();

            imageService = _generalDataService.GetService(SettingsRegistry.ServiceTitleImageSaver);

            ChatToImageConfig = imageService.Config;
            ChatToImagePath = ChatToImageConfig.GetParameterValue("Filename") as string;



            var webServerService = _generalDataService.GetService(SettingsRegistry.ServiceTitleWebServer);

            if( webServerService != null )
            {
                webServerService.GetData( (obj) => {
                    var webServer = obj as WebServerService;
                    webServer.GetUserHandler = (uri,httpProcessor) =>
                    {
                        if (uri.LocalPath.Equals("/messages.json", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var parameters = HttpUtility.ParseQueryString(uri.Query);
                            var lastMessageId = parameters["id"];
                            ChatMessage[] messages = null;

                            ChatMessageViewModel previousMessage = Messages.FirstOrDefault(m => lastMessageId != null && m.Message.Id.ToString().Equals(lastMessageId.ToString(), StringComparison.InvariantCultureIgnoreCase));
                            if (previousMessage == null )
                                lastMessageId = null;

                            if ( lastMessageId == null)
                            {
                                var messageNumber = 10;
                                messages = Messages.Skip(Math.Max(0, Messages.Count() - messageNumber)).Take(messageNumber).Select(m => m.Message).ToArray();
                            }
                            else
                            {

                                var guid = lastMessageId.ToString();
                                var skipCount = Messages.IndexOf( previousMessage )+1;

                                int waitSteps = 55;
                                while( waitSteps > 0 )
                                {
                                    messages = Messages.Skip(skipCount).Select(m => m.Message).ToArray();
                                    if (messages.Count() > 0)
                                        break;

                                    Thread.Sleep(1000);
                                    waitSteps--;

                                }
                            }
                            if( messages != null )
                            {
                                Json.SerializeToStream(messages, (stream) =>
                                {
                                    webServer.SendJsonToClient(stream,httpProcessor);
                                });
                            }
                            return true;
                        }
                        else if (uri.LocalPath.Equals("/settings.json",StringComparison.InvariantCultureIgnoreCase))
                        {
                            int waitSteps = 55;
                            while( waitSteps > 0  )
                            {
                                if (!JsonConvert.SerializeObject(Ubiquitous.Default.Config.AppConfig).Equals(lastSettings, StringComparison.InvariantCultureIgnoreCase))
                                    break;

                                Thread.Sleep(1000);
                                waitSteps--;
                            }
                            Json.SerializeToStream(Ubiquitous.Default.Config.AppConfig, (stream) =>
                                {
                                    webServer.SendJsonToClient(stream,httpProcessor);
                                });
                            return true;
                        }

                        return false;
                    };
                });

                IsMessageAdded = false;
                IsMessageAdded = true;
            }
        }
        private void AddMessages(ChatMessage[] messages)
        {
            if( IsInDesignMode )
            {
                foreach (var msg in messages)
                {
                    Messages.Add(
                        new ChatMessageViewModel(msg)
                    );
                }
            }
            else
            {
                if( !firstMessageReceived )
                {
                    firstMessageReceived = true;
                    MessengerInstance.Send<bool>(false, "SwitchHelloPlaceHolder");
                }
                UI.Dispatch(() =>
                {               
                    foreach( var msg in messages)
                    {
                        var newMessage = new ChatMessageViewModel(msg);
                        Messages.Add(
                            newMessage
                        );
                    }

                    if (MessageAdded != null)
                        MessageAdded(this, EventArgs.Empty);

                    if (ChatToImageConfig.Enabled)
                    {
                        IsMessageAdded = true;
                        IsMessageAdded = false;
                    }
                });

            }
        }

        /// <summary>
        /// The <see cref="Messages" /> property's name.
        /// </summary>
        public const string MessagesPropertyName = "Messages";

        private ObservableCollection<ChatMessageViewModel> _myProperty = new ObservableCollection<ChatMessageViewModel>();

        /// <summary>
        /// Sets and gets the Messages property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ChatMessageViewModel> Messages
        {
            get
            {
                return _myProperty;
            }

            set
            {
                if (_myProperty == value)
                {
                    return;
                }

                _myProperty = value;
                RaisePropertyChanged(MessagesPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsMessageAdded" /> property's name.
        /// </summary>
        public const string IsMessageAddedPropertyName = "IsMessageAdded";

        private bool _isMessageAdded = false;

        /// <summary>
        /// Sets and gets the IsMessageAdded property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsMessageAdded
        {
            get
            {
                return _isMessageAdded;
            }

            set
            {
                if (_isMessageAdded == value)
                {
                    return;
                }
                _isMessageAdded = value;
                RaisePropertyChanged(IsMessageAddedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="EnableAutoScroll" /> property's name.
        /// </summary>
        public const string EnableAutoScrollPropertyName = "EnableAutoScroll";

        private bool _enableAutoScroll = true;

        /// <summary>
        /// Sets and gets the EnableAutoScroll property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool EnableAutoScroll
        {
            get
            {
                return _enableAutoScroll;
            }

            set
            {
                if (_enableAutoScroll == value)
                {
                    return;
                }
                _enableAutoScroll = value;
                IsScrollBarVisible = !_enableAutoScroll;
                RaisePropertyChanged(EnableAutoScrollPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsScrollBarVisible" /> property's name.
        /// </summary>
        public const string IsScrollBarVisiblePropertyName = "IsScrollBarVisible";

        private bool _isScrollBarVisible = false;

        /// <summary>
        /// Sets and gets the IsScrollBarVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsScrollBarVisible
        {
            get
            {
                return _isScrollBarVisible;
            }

            set
            {
                if (_isScrollBarVisible == value)
                {
                    return;
                }
                _isScrollBarVisible = value;
                RaisePropertyChanged(IsScrollBarVisiblePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ChatToImagePath" /> property's name.
        /// </summary>
        public const string ChatToImagePathPropertyName = "ChatToImagePath";

        private string _chatToImagePath = null;

        /// <summary>
        /// Sets and gets the ChatToImagePath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ChatToImagePath
        {
            get
            {
                return _chatToImagePath;
            }

            set
            {
                if (_chatToImagePath == value)
                {
                    return;
                }
                _chatToImagePath = value;
                imageService.Config.SetParameterValue("Filename", _chatToImagePath);
                RaisePropertyChanged(ChatToImagePathPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="ChatToImageConfig" /> property's name.
        /// </summary>
        public const string ChatToImageConfigPropertyName = "ChatToImageConfig";

        private ServiceConfig _chatToImageConfig = new ServiceConfig();

        /// <summary>
        /// Sets and gets the ChatToImageConfig property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ServiceConfig ChatToImageConfig
        {   
            get
            {
                return _chatToImageConfig;
            }

            set
            {
                if (_chatToImageConfig == value)
                {
                    return;
                }

                _chatToImageConfig = value;
                RaisePropertyChanged(ChatToImageConfigPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="AppConfig" /> property's name.
        /// </summary>
        public const string AppConfigPropertyName = "AppConfig";

        private AppConfig _appConfig = new AppConfig();

        /// <summary>
        /// Sets and gets the AppConfig property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public AppConfig AppConfig
        {
            get
            {
                return _appConfig;
            }

            set
            {
                if (_appConfig == value)
                {
                    return;
                }
                _appConfig = value;
                RaisePropertyChanged(AppConfigPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ChatBoxWidth" /> property's name.
        /// </summary>
        public const string ChatBoxWidthPropertyName = "ChatBoxWidth";

        private int _chatBoxWidth = 0;

        /// <summary>
        /// Sets and gets the ChatBoxWidth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int ChatBoxWidth
        {
            get
            {
                return _chatBoxWidth;
            }

            set
            {
                if (_chatBoxWidth == value)
                {
                    return;
                }

                _chatBoxWidth = value;
                (Application.Current as App).ChatBoxWidth = _chatBoxWidth;


                RaisePropertyChanged(ChatBoxWidthPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ChatBoxHeight" /> property's name.
        /// </summary>
        public const string ChatBoxHeightPropertyName = "ChatBoxHeight";

        private double _chatBoxHeight = 0;

        /// <summary>
        /// Sets and gets the ChatBoxHeight property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double ChatBoxHeight
        {
            get
            {
                return _chatBoxHeight;
            }

            set
            {
                if (_chatBoxHeight == value)
                {
                    return;
                }
                _chatBoxHeight = value;
                (Application.Current as App).ChatBoxHeight = _chatBoxHeight;
                RaisePropertyChanged(ChatBoxHeightPropertyName);
            }
        }

        public override void Cleanup()
        {
                MessengerInstance.Unregister<bool>(this, "MessageSent");
                MessengerInstance.Unregister<bool>(this, "EnableAutoScroll");
        }

    }

}