using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using System.Linq;
using UB.Model;
using UB.Properties;

namespace UB.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class UserListViewModel : ViewModelBase
    {        
        IChatDataService _dataService;
        /// <summary>
        /// Initializes a new instance of the UserListViewModel class.
        /// </summary>
        [PreferredConstructor]
        public UserListViewModel(IChatDataService dataService)
        {
            _dataService = dataService;
            Chats = new ObservableCollection<ChatUserListViewModel>();
            AppConfig = Ubiquitous.Default.Config.AppConfig;
            Initialize();
        }

        private void Initialize()
        {
            if (_dataService == null || Chats == null )
                return; 

            if (Chats.Count > 0)
                Chats.Clear();

            foreach( var chat in _dataService.Chats.ToList() )
            {
                if( chat is IChatUserList )
                {
                    Chats.Add(new ChatUserListViewModel() { 
                        Chat = chat,
                        UserList = (chat as IChatUserList).ChatUsers,
                    });
                }
            }
        }

        /// <summary>
        /// The <see cref="Chats" /> property's name.
        /// </summary>
        public const string ChatsPropertyName = "Chats";

        private ObservableCollection<ChatUserListViewModel> _chatUserListViewMoel = null;

        /// <summary>
        /// Sets and gets the Chats property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ChatUserListViewModel> Chats
        {
            get
            {
                return _chatUserListViewMoel;
            }

            set
            {
                if (_chatUserListViewMoel == value)
                {
                    return;
                }

                _chatUserListViewMoel = value;
                RaisePropertyChanged(ChatsPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="AppConfig" /> property's name.
        /// </summary>
        public const string AppConfigPropertyName = "AppConfig";

        private AppConfig _appConfig = null;

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
    }

    public class ChatUserListViewModel : ViewModelBase
    {
        /// <summary>
        /// The <see cref="UserList" /> property's name.
        /// </summary>
        public const string UserListPropertyName = "UserList";

        private ObservableCollection<ChatUser> _userList = null;

        /// <summary>
        /// Sets and gets the UserList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ChatUser> UserList
        {
            get
            {
                return _userList;
            }

            set
            {
                if (_userList == value)
                {
                    return;
                }

                _userList = value;
                RaisePropertyChanged(UserListPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="Chat" /> property's name.
        /// </summary>
        public const string ChatPropertyName = "Chat";

        private IChat _chat = null;

        /// <summary>
        /// Sets and gets the Chat property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public IChat Chat
        {
            get
            {
                return _chat;
            }

            set
            {
                if (_chat == value)
                {
                    return;
                }

                _chat = value;
                RaisePropertyChanged(ChatPropertyName);
            }
        }
    }
}