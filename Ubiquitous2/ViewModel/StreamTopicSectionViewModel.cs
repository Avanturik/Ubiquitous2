using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using UB.Model;
using System.Linq;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Command;
using System.Threading.Tasks;
using UB.Utils;
namespace UB.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class StreamTopicSectionViewModel : ViewModelBase
    {
        private IStreamTopic _streamTopic;

        [PreferredConstructor]
        public StreamTopicSectionViewModel()
        {
            StreamInfo = new StreamInfo() { 
                HasDescription = true,
            };
            ChannelIcon = Icons.DesignMainHeadsetIcon;
        }

        /// <summary>
        /// Initializes a new instance of the StreamTopicSectionViewModel class.
        /// </summary>
        public StreamTopicSectionViewModel(IStreamTopic streamTopic)
        {
            _streamTopic = streamTopic;
            StreamInfo = _streamTopic.Info;
            ChannelIcon = (streamTopic as IChat).IconURL;
            EnableGameSuggestion = true;
        }


        /// <summary>
        /// The <see cref="EnableGameSuggestion" /> property's name.
        /// </summary>
        public const string EnableGameSuggestionPropertyName = "EnableGameSuggestion";

        private bool _enableGameSuggestion = false;

        /// <summary>
        /// Sets and gets the EnableGameSuggestion property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool EnableGameSuggestion
        {
            get
            {
                return _enableGameSuggestion;
            }

            set
            {
                if (_enableGameSuggestion == value)
                {
                    return;
                }

                RaisePropertyChanging(EnableGameSuggestionPropertyName);
                _enableGameSuggestion = value;
                RaisePropertyChanged(EnableGameSuggestionPropertyName);
            }
        }

        private RelayCommand _commandNeedSuggestion;

        /// <summary>
        /// Gets the CommandNeedSuggestion.
        /// </summary>
        public RelayCommand CommandNeedSuggestion
        {
            get
            {
                return _commandNeedSuggestion
                    ?? (_commandNeedSuggestion = new RelayCommand(
                                          () =>
                                          {
                                              Task.Factory.StartNew(() => {
                                                  _streamTopic.QueryGameList(StreamInfo.CurrentGame.Name, () =>
                                                  {
                                                      GameSuggestions = new ObservableCollection<string>(_streamTopic.Games.OrderBy( item => item.Name).Select(game => game.Name));
                                                  });                                              
                                              });
                                          }));
            }
        }

        /// <summary>
        /// The <see cref="GameSuggestions" /> property's name.
        /// </summary>
        public const string GameSuggestionsPropertyName = "GameSuggestions";

        private ObservableCollection<string> _gameSuggestions = new ObservableCollection<string>();

        /// <summary>
        /// Sets and gets the GameSuggestions property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<string> GameSuggestions
        {
            get
            {
                return _gameSuggestions;
            }

            set
            {
                if (_gameSuggestions == value)
                {
                    return;
                }

                RaisePropertyChanging(GameSuggestionsPropertyName);
                _gameSuggestions = value;
                RaisePropertyChanged(GameSuggestionsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ChannelIcon" /> property's name.
        /// </summary>
        public const string ChannelIconPropertyName = "ChannelIcon";

        private string _channelIcon = Icons.MainHeadsetIcon;

        /// <summary>
        /// Sets and gets the ChannelIcon property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ChannelIcon
        {
            get
            {
                return _channelIcon;
            }

            set
            {
                if (_channelIcon == value)
                {
                    return;
                }

                RaisePropertyChanging(ChannelIconPropertyName);
                _channelIcon = value;
                RaisePropertyChanged(ChannelIconPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="StreamInfo" /> property's name.
        /// </summary>
        public const string StreamInfoPropertyName = "StreamInfo";

        private StreamInfo _streamInfo = new StreamInfo();

        /// <summary>
        /// Sets and gets the StreamInfo property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public StreamInfo StreamInfo
        {
            get
            {
                return _streamInfo;
            }

            set
            {
                if (_streamInfo == value)
                {
                    return;
                }

                RaisePropertyChanging(StreamInfoPropertyName);
                _streamInfo = value;
                RaisePropertyChanged(StreamInfoPropertyName);
            }
        }
        
    }
}