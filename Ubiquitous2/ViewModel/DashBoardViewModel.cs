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
        private IStreamPageDataService _streamDataService;
        /// <summary>
        /// Initializes a new instance of the DashBoardViewModel class.
        /// </summary>
        [PreferredConstructor]
        public DashBoardViewModel(IChatDataService dataService, IStreamPageDataService streamDataService)
        {
            _dataService = dataService;
            _streamDataService = streamDataService;
            Initialize();
        }

        private void Initialize()
        {

            InitializeTopicSections();
            
            //Preset combobox
            TopicPresets = new ObservableCollection<EditComboBoxItem>();
            _streamDataService.GetPresets((presets) => {
                if (presets != null)
                {
                    presets.ForEach(preset => {
                        var editComboItem = new EditComboBoxItem()
                        {
                            LinkedObject = preset,
                            Title = preset.PresetName,
                        };
                        TopicPresets.Add(editComboItem);

                    });
                }
            });
        }

        #region Stream topic       
        private void InitializeTopicSections()
        {
            StreamTopics.Clear();
            _streamDataService.GetStreamTopics((topics) => {
                if (topics != null)
                    topics.ForEach( topic => StreamTopics.Add(new StreamTopicSectionViewModel(topic)) );
            });
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
                                              _streamDataService.LoadTopicsFromWeb();
                                              InitializeTopicSections();
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
                                              var selected = TopicPresets.FirstOrDefault(item => item.IsCurrent);
                                              if (selected != null)
                                                  (selected.LinkedObject as StreamInfoPreset).PresetName = selected.Title;
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
                                              _streamDataService.GetStreamTopics((streams) => {
                                                  streams.ForEach(stream => {
                                                      var newInfo = linkedSettings.StreamTopics.FirstOrDefault(item => item.ChatName == stream.Info.ChatName);
                                                      if (newInfo == null)
                                                      {
                                                          newInfo = stream.Info.GetCopy();
                                                          linkedSettings.StreamTopics.Add(newInfo);
                                                      }
                                                      stream.Info = newInfo;
                                                  });
                                              });
                                              InitializeTopicSections();
                                          }));
            }
        }

        private RelayCommand<EditComboBoxItem> _deletePreset;

        /// <summary>
        /// Gets the DeletePreset.
        /// </summary>
        public RelayCommand<EditComboBoxItem> DeletePreset
        {
            get
            {
                return _deletePreset
                    ?? (_deletePreset = new RelayCommand<EditComboBoxItem>(
                                          (item) =>
                                          {
                                              _streamDataService.RemovePreset(item.LinkedObject as StreamInfoPreset);
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
                                              if( selected != null )
                                                selected.LinkedObject = _streamDataService.AddPreset(selected.Title);
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
                                              _streamDataService.UpdateTopicsOnWeb();
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
                _streamTopics = value;
                RaisePropertyChanged(StreamTopicsPropertyName);
            }
        }
        #endregion
    }
}