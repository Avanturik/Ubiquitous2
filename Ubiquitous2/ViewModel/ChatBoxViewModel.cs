using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using UB.Model;

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

            MessengerInstance.Register<ChatMessage>(this, msg =>
            {
                AddMessage(msg);
            });

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
        }
        
        private void AddMessage(ChatMessage msg)
        {
            Messages.Add(new ChatMessageViewModel(msg));
            if (MessageAdded != null)
                MessageAdded(this, EventArgs.Empty);
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