using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using UB.Model;
using System.Linq;
using UB.Controls;

namespace UB.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DashBoardViewModel : ViewModelBase
    {
        private IChatDataService _dataService;
        /// <summary>
        /// Initializes a new instance of the DashBoardViewModel class.
        /// </summary>
        [PreferredConstructor]
        public DashBoardViewModel(IChatDataService dataService)
        {
            _dataService = dataService;
            Initialize();
        }

        private void Initialize()
        {
            foreach( var chat in _dataService.Chats )
            {
                if( chat is IStreamTopic)
                {
                    var streamTopic = chat as IStreamTopic;
                    StreamTopics.Add(new StreamTopicSectionViewModel(streamTopic));
                }
            }
            //TopicPresets = new ObservableCollection<EditComboBoxItem>();
        }


        /// <summary>
        /// The <see cref="TopicPresets" /> property's name.
        /// </summary>
        public const string TopicPresetsPropertyName = "TopicPresets";

        private ObservableCollection<EditComboBoxItem> _topicPresets = new ObservableCollection<EditComboBoxItem>();

        /// <summary>
        /// Sets and gets the TopicPresets property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<EditComboBoxItem> TopicPresets
        {
            get
            {
                return _topicPresets;
            }

            set
            {
                if (_topicPresets == value)
                {
                    return;
                }

                RaisePropertyChanging(TopicPresetsPropertyName);
                _topicPresets = value;
                RaisePropertyChanged(TopicPresetsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="StreamTopics" /> property's name.
        /// </summary>
        public const string StreamTopicsPropertyName = "StreamTopics";

        private ObservableCollection<StreamTopicSectionViewModel> _streamTopics = new ObservableCollection<StreamTopicSectionViewModel>();

        /// <summary>
        /// Sets and gets the StreamTopics property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<StreamTopicSectionViewModel> StreamTopics
        {
            get
            {
                return _streamTopics;
            }

            set
            {
                if (_streamTopics == value)
                {
                    return;
                }

                RaisePropertyChanging(StreamTopicsPropertyName);
                _streamTopics = value;
                RaisePropertyChanged(StreamTopicsPropertyName);
            }
        }
    }
}