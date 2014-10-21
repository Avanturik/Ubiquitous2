using Devart.Controls;
using GalaSoft.MvvmLight;
using UB.Utils;
using UB.Model;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System.Windows;


namespace UB.ViewModel
{    
    public class ChatMessageViewModel : ViewModelBase, IHeightMeasurer
    {
        private readonly IChatDataService _dataService;

        private double estimatedHeight;
        private double estimatedWidth;

        public ChatMessageViewModel()
        {
            _dataService = SimpleIoc.Default.GetInstance<IChatDataService>();
            Initialize(new ChatMessage("Lorem ipsum") { ChatName = SettingsRegistry.ChatTitleNormalTwitch, ChatIconURL = Icons.DesignMainIcon });
            _dataService.GetRandomMessage(
            (item, error) =>
            {
                if (error != null)
                {
                    // Report error here
                    return;
                }
                Message = item;
                Initialize(item);
            });
        }

        [PreferredConstructor]
        public ChatMessageViewModel(IChatDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetRandomMessage(
            (item, error) =>
            {
                if (error != null)
                {
                    // Report error here
                    return;
                }
                Message = item;
                Initialize(item);
            });
        }

        public ChatMessageViewModel (ChatMessage message)
        {
            _dataService = SimpleIoc.Default.GetInstance<IChatDataService>();
            Initialize(message);
        }

        private void Initialize(ChatMessage message)
        {

            Message = message;
            if (Message.ChatIconURL == null)
                Message.ChatIconURL = Icons.MainIcon;

            if (message.ChatName == null)
                Message.ChatName = SettingsRegistry.ChatTitleNormalTwitch;

            if (IsInDesignMode)
                return;

            if( Application.Current != null)
                AppConfig = (Application.Current as App).AppConfig;

        }
        public ChatMessage Message { get; set; }

        public double GetEstimatedHeight(double width)
        {
            // Do not recalc height if text and width are unchanged
            if (estimatedHeight < 0 || estimatedWidth != width)
            {
                estimatedWidth = width;
                estimatedHeight = TextMeasurer.GetEstimatedHeight(Message.Text, width) + 38; // Add margin
            }
            return estimatedHeight;
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


        private RelayCommand _setExpiration;

        /// <summary>
        /// Gets the SetExpiration.
        /// </summary>
        public RelayCommand SetExpiration
        {
            get
            {
                return _setExpiration
                    ?? (_setExpiration = new RelayCommand(
                    () =>
                    {
                        Message.IsNew = false;
                    }));
            }
        }

        private RelayCommand _setActiveChannel;

        /// <summary>
        /// Gets the SetActiveChannel.
        /// </summary>
        [JsonIgnore]
        public RelayCommand SetActiveChannel
        {
            get
            {
                return _setActiveChannel
                    ?? (_setActiveChannel = new RelayCommand(
                                          () =>
                                          {
                                              MessengerInstance.Send<ChatMessage>(Message, "SetChannel");
                                          }));
            }
        }
        
        private RelayCommand<ChatMessage> _ignoreUser;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        [JsonIgnore]
        public RelayCommand<ChatMessage> IgnoreUser
        {
            get
            {
                return _ignoreUser
                    ?? (_ignoreUser = new RelayCommand<ChatMessage>(
                                          (message) =>
                                          {
                                              if (message == null)
                                                  return;

                                              UI.Dispatch(() => { 
                                                  MessengerInstance.Send<YesNoDialog>(new YesNoDialog() { 
                                                      HeaderText = "Ignore user",
                                                      QuestionText = message.FromUserName + "@" + message.ChatName,
                                                      IsOpenRequest = true,
                                                      YesAction = () => {                                                      
                                                          _dataService.AddMessageSenderToIgnoreList(message);
                                                      },
                                                  },"OpenDialog");
                                              });

                                          }));
            }
        }

        public override void Cleanup()
        {
            Log.WriteInfo("Disposed ChatMessageViewModel");
        }
    }

}

