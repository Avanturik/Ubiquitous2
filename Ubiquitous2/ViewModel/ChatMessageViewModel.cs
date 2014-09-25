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
            Message = message;
            if (Message.ChatIconURL == null)
                Message.ChatIconURL = Icons.MainIcon;
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
        
        private RelayCommand _ignoreUser;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        [JsonIgnore]
        public RelayCommand IgnoreUser
        {
            get
            {
                return _ignoreUser
                    ?? (_ignoreUser = new RelayCommand(
                                          () =>
                                          {
                                              MessengerInstance.Send<YesNoDialog>(new YesNoDialog() { 
                                                  QuestionText = "Ignore " + Message.FromUserName + "@" + Message.ChatName + " ?",
                                                  IsOpenRequest = true,
                                                  YesAction = () => {
                                                      var dataservice = SimpleIoc.Default.GetInstance<IChatDataService>();
                                                      dataservice.AddMessageSenderToIgnoreList(Message);
                                                  },
                                              },"OpenDialog");

                                          }));
            }
        }

        /// <summary>
        /// The <see cref="InMenu" /> property's name.
        /// </summary>
        public const string InMenuPropertyName = "InMenu";

        private bool _inMenu = false;

        /// <summary>
        /// Sets and gets the InMenu property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>

    }

}

