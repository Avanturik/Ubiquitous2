using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using UB.Model;
using System.Linq;
using UB.Controls;
using UB.Properties;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using UB.Utils;

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

            //Fill stream topic sections with current web values
            foreach (var chat in _dataService.Chats)
            {
                if (chat is IStreamTopic)
                {
                    var streamTopic = chat as IStreamTopic;
                    streamTopic.Info.ChatName = chat.ChatName;
                    StreamTopics.Add(new StreamTopicSectionViewModel(streamTopic));
                }
            }
            
            //Preset combobox
            TopicPresets = new ObservableCollection<EditComboBoxItem>();
            foreach( var preset in Ubiquitous.Default.Config.StreamInfoPresets )
            {
                TopicPresets.Add(new EditComboBoxItem() { 
                    LinkedObject = preset,
                    Title = preset.PresetName,
                });
            }
        }

        private RelayCommand _loadWeb;

        /// <summary>
        /// Gets the LoadWeb.
        /// </summary>
        public RelayCommand LoadWeb
        {
            get
            {
                return _loadWeb
                    ?? (_loadWeb = new RelayCommand(
                                          () =>
                                          {
                                              StreamTopics.Clear();
                                              foreach (var chat in _dataService.Chats)
                                              {
                                                  if (chat is IStreamTopic)
                                                  {
                                                      var streamTopic = chat as IStreamTopic;
                                                      streamTopic.GetTopic();
                                                      StreamTopics.Add(new StreamTopicSectionViewModel(streamTopic));
                                                  }
                                              }                                              
                                          }));
            }
        }

        private RelayCommand _renamePreset;

        /// <summary>
        /// Gets the RenamePreset.
        /// </summary>
        public RelayCommand RenamePreset
        {
            get
            {
                return _renamePreset
                    ?? (_renamePreset = new RelayCommand(
                                          () =>
                                          {
                                              
                                          }));
            }
        }

        private RelayCommand _selectPreset;

        /// <summary>
        /// Gets the SelectPreset.
        /// </summary>
        public RelayCommand SelectPreset
        {
            get
            {
                return _selectPreset
                    ?? (_selectPreset = new RelayCommand(
                                          () =>
                                          {
                                              var linkedSettings = this.With( x => TopicPresets.FirstOrDefault(item => item.IsCurrent))
                                                  .With(x => x.LinkedObject as StreamInfoPreset );

                                              if (linkedSettings == null)
                                                  return;

                                              foreach (var chat in _dataService.Chats)
                                              {
                                                  if (chat is IStreamTopic)
                                                  {
                                                      var streamTopic = chat as IStreamTopic;
                                                      var newInfo = linkedSettings.StreamTopics.FirstOrDefault(item => item.ChatName == chat.ChatName);
                                                      if (newInfo == null)
                                                          continue;

                                                      streamTopic.Info.CurrentGame.Id = newInfo.CurrentGame.Id;
                                                      streamTopic.Info.CurrentGame.Name = newInfo.CurrentGame.Name;
                                                      streamTopic.Info.Topic = newInfo.Topic;
                                                      streamTopic.Info.Description = newInfo.Description;

                                                  }
                                              }  

                                          }));
            }
        }

        private RelayCommand _deletePreset;

        /// <summary>
        /// Gets the DeletePreset.
        /// </summary>
        public RelayCommand DeletePreset
        {
            get
            {
                return _deletePreset
                    ?? (_deletePreset = new RelayCommand(
                                          () =>
                                          {
                                              
                                          }));
            }
        }

        private RelayCommand _addPreset;

        /// <summary>
        /// Gets the AddPreset.
        /// </summary>
        public RelayCommand AddPreset
        {
            get
            {
                return _addPreset
                    ?? (_addPreset = new RelayCommand(
                                          () =>
                                          {
                                              var selected = TopicPresets.FirstOrDefault( item => item.IsCurrent );
                                              if( selected == null )
                                                  return;

                                              var newPreset = new StreamInfoPreset() { PresetName = selected.Title };
                                              newPreset.StreamTopics = new List<StreamInfo>();
                                              foreach( var sectionViewMmodel in StreamTopics )
                                              {
                                                  var info = sectionViewMmodel.StreamInfo;
                                                  newPreset.StreamTopics.Add( info.GetCopy() );
                                              }

                                              Ubiquitous.Default.Config.StreamInfoPresets.Add( newPreset );
                                          }));
            }
        }

        private RelayCommand _updateWeb;

        /// <summary>
        /// Gets the UpdateWeb.
        /// </summary>
        public RelayCommand UpdateWeb
        {
            get
            {
                return _updateWeb
                    ?? (_updateWeb = new RelayCommand(
                                          () =>
                                          {
                                                foreach( var chat in _dataService.Chats )
                                                {
                                                    if( chat is IStreamTopic)
                                                    {
                                                        var streamTopic = chat as IStreamTopic;
                                                        //TODO: update istreamtopic with current viewmodel settings, call SetTopic
                                                    }
                                                }
                                          }));
            }
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