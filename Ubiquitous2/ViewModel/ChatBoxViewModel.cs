using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using UB.Model;
using UB.Utils;

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
        IChatDataService _dataService;
        /// <summary>
        /// Initializes a new instance of the ChatBoxViewModel class.
        /// </summary>
        public ChatBoxViewModel()
        {

        }

        [PreferredConstructor]
        public ChatBoxViewModel(IChatDataService dataService)
        {
            _dataService = dataService;
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
                        item.Text += " http://asdf.com ";
                        item.Text = Html.ConvertUrlsToLinks(item.Text);
                        item.Text += @" " + Html.CreateImageTag(@"http://static-cdn.jtvnw.net/jtv_user_pictures/chansub-global-emoticon-ebf60cd72f7aa600-24x18.png",24,18);
                        Messages.Add(new ChatMessageViewModel(item));

                    });
            }

            //MessengerInstance.Register<ChatMessage>(this, msg =>
            //{
            //    AddMessages(new ChatMessage[] { msg });
            //});

            _dataService.ReadMessages((messages,error) => {
                AddMessages(messages);
            });
        }


        private void AddMessages(ChatMessage[] messages)
        {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {               
                    foreach( var msg in messages)
                    {
                        Messages.Add(
                            new ChatMessageViewModel(msg)
                        );
                    }
                    if (MessageAdded != null)
                        MessageAdded(this, EventArgs.Empty);
                });
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

                RaisePropertyChanging(MessagesPropertyName);
                _myProperty = value;
                RaisePropertyChanged(MessagesPropertyName);
            }
        }
    }

}