using Devart.Controls;
using GalaSoft.MvvmLight;
using UB.Utils;
using UB.Model;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;


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
                    });
        }

        public ChatMessageViewModel (ChatMessage message)
        {
            _dataService = SimpleIoc.Default.GetInstance<IChatDataService>();
            Message = message;

            if (Message.ChatIconURL == null)
                Message.ChatIconURL = Icons.MainIcon;

            if (message.ChatName == null)
                Message.ChatName = SettingsRegistry.ChatTitleNormalTwitch;
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

