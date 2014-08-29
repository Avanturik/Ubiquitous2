using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using UB.Model;
using UB.Model.IRC;

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
        IDataService _dataService;
        /// <summary>
        /// Initializes a new instance of the ChatBoxViewModel class.
        /// </summary>
        public ChatBoxViewModel()
        {

        }

        [PreferredConstructor]
        public ChatBoxViewModel(IDataService dataService)
        {
            _dataService = dataService;
            //Test data
            for (var i = 0; i < 3; i++)
            {
                _dataService.GetMessage(
                    (item, error) =>
                    {
                        if (error != null)
                        {
                            // Report error here
                            return;
                        }

                        Messages.Add(new ChatMessageViewModel(item));

                    });
            }

            if (IsInDesignMode)
                return;
            MessengerInstance.Register<ChatMessage>(this, msg =>
            {
                AddMessage(msg);
            });

            var userchannel = "goodguygarry";

            var irc = new IRCChatBase(new IRCLoginInfo()
            {
                Channel = userchannel,
                Port = 6667,
                UserName = "justinfan123412893",
                HostName = "irc.twitch.tv",
            });
            irc.MessageReceived += irc_MessageReceived;
            irc.Start();

        }

        void irc_MessageReceived(object sender, ChatServiceEventArgs e)
        {
            AddMessage(new ChatMessage() { 
                ImageSource = @"/favicon.ico",
                FromUserName = e.Messages[0].FromUserName,
                Text = e.Messages[0].Text
            });
        }
        
        private void AddMessage(ChatMessage msg)
        {
            DispatcherHelper.Initialize();
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    Messages.Add(new ChatMessageViewModel(msg));
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