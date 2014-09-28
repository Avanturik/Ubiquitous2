using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using UB.Model;
using System.Linq;
using GalaSoft.MvvmLight.Ioc;
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
        [PreferredConstructor]
        public StreamTopicSectionViewModel()
        {

        }
        /// <summary>
        /// Initializes a new instance of the StreamTopicSectionViewModel class.
        /// </summary>
        public StreamTopicSectionViewModel(IStreamTopic streamTopic)
        {
            StreamInfo = streamTopic.Info;
            GameEditBox = new EditBoxViewModel() { 
                Watermark = "Game title",
                Text = StreamInfo.CurrentGame.Name,
                UpdateSuggestions = (name) =>
                {
                    streamTopic.QueryGameList(name);
                    return new ObservableCollection<string>(streamTopic.Games.Select(game => game.Name));
                },
            };
            ChannelIcon = (streamTopic as IChat).IconURL;
        }

        /// <summary>
        /// The <see cref="GameEditBox" /> property's name.
        /// </summary>
        public const string GameEditBoxPropertyName = "GameEditBox";

        private EditBoxViewModel _gameEditBox = new EditBoxViewModel();

        /// <summary>
        /// Sets and gets the GameEditBox property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public EditBoxViewModel GameEditBox
        {
            get
            {
                return _gameEditBox;
            }

            set
            {
                if (_gameEditBox == value)
                {
                    return;
                }

                RaisePropertyChanging(GameEditBoxPropertyName);
                _gameEditBox = value;
                RaisePropertyChanged(GameEditBoxPropertyName);
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