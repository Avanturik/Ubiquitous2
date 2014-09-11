using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devart.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using UB.Model;

namespace UB.ViewModel
{
    public class SettingsChatItemViewModel : ViewModelBase, IHeightMeasurer
    {
        private ChatConfig chatConfig;
        private SettingsDataService _dataService;
        private IChatDataService chatDataService;

        [PreferredConstructor]
        public SettingsChatItemViewModel(SettingsDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetRandomChatSetting(
            (item) =>
            {
                _enabled = item.Enabled;
                _chatName = item.ChatName;
                _chatIconURL = item.IconURL ?? _chatIconURL;
                foreach (var field in item.Parameters)
                {
                    if (field.IsVisible)
                        SettingsFields.Add(
                            new SettingsFieldViewModel(field)
                        );
                }
            });

        }

        public SettingsChatItemViewModel(ChatConfig config)
        {
            chatDataService = ServiceLocator.Current.GetInstance<IChatDataService>();
            chatConfig = config;
            _enabled = config.Enabled;
            _chatName = config.ChatName;
            _chatIconURL = config.IconURL ?? _chatIconURL;
            foreach( var param in config.Parameters )
            {
                if( param.IsVisible)
                    SettingsFields.Add(
                        new SettingsFieldViewModel(param)
                    );
            }
        }
        public double GetEstimatedHeight(double availableWidth)
        {
            return 30;
        }

        /// <summary>
        /// The <see cref="ChatName" /> property's name.
        /// </summary>
        public const string ChatNamePropertyName = "ChatName";

        private String _chatName = "LoremIpsum";

        /// <summary>
        /// Sets and gets the ChatName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public String ChatName
        {
            get
            {
                return _chatName;
            }

            set
            {
                if (_chatName == value)
                {
                    return;
                }

                RaisePropertyChanging(ChatNamePropertyName);
                _chatName = value;
                RaisePropertyChanged(ChatNamePropertyName);
            }
        }
        /// <summary>
        /// The <see cref="CalculatedHeight" /> property's name.
        /// </summary>
        public const string CalculatedHeightPropertyName = "CalculatedHeight";

        private double _calculatedHeight = 30;

        /// <summary>
        /// Sets and gets the CalculatedHeight property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double CalculatedHeight
        {
            get
            {
                return SettingsFields.Count * 30 + 20;
            }

            set
            {
                if (_calculatedHeight == value)
                {
                    return;
                }

                RaisePropertyChanging(CalculatedHeightPropertyName);
                _calculatedHeight = value;
                RaisePropertyChanged(CalculatedHeightPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SettingsFields" /> property's name.
        /// </summary>
        public const string SettingsFieldsPropertyName = "SettingsFields";

        private ObservableCollection<SettingsFieldViewModel> _settingsFields = new ObservableCollection<SettingsFieldViewModel>();

        /// <summary>
        /// Sets and gets the SettingsFields property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SettingsFieldViewModel> SettingsFields
        {
            get
            {
                return _settingsFields;
            }

            set
            {
                if (_settingsFields == value)
                {
                    return;
                }

                RaisePropertyChanging(SettingsFieldsPropertyName);
                _settingsFields = value;
                RaisePropertyChanged(SettingsFieldsPropertyName);
            }
        }

        private RelayCommand _restartChat;

        /// <summary>
        /// Gets the RestartChat.
        /// </summary>
        public RelayCommand RestartChat
        {
            get
            {
                return _restartChat
                    ?? (_restartChat = new RelayCommand(
                                          () =>
                                          {
                                              Task.Factory.StartNew(() => {
                                                  if (this.Enabled)
                                                  {
                                                      this.Enabled = false;
                                                      this.Enabled = true;
                                                  }
                                              
                                              });
                                          }));
            }
        }

        private RelayCommand _toggleEdit;

        /// <summary>
        /// Gets the ToggleEdit.
        /// </summary>
        public RelayCommand ToggleEdit
        {
            get
            {
                return _toggleEdit
                    ?? (_toggleEdit = new RelayCommand(
                                          () =>
                                          {
                                              Expanded = !Expanded;
                                              
                                          }));
            }
        }

        /// <summary>
        /// The <see cref="EditLinkTitle" /> property's name.
        /// </summary>
        public const string EditLinkTitlePropertyName = "EditLinkTitle";

        private String _editLinkTitle;

        /// <summary>
        /// Sets and gets the EditLinkTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public String EditLinkTitle
        {
            get
            {
                return Expanded == true ? "done" : "edit";
            }

            set
            {
                if (_editLinkTitle == value)
                {
                    return;
                }

                RaisePropertyChanging(EditLinkTitlePropertyName);
                _editLinkTitle = value;
                RaisePropertyChanged(EditLinkTitlePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Expanded" /> property's name.
        /// </summary>
        public const string ExpandedPropertyName = "Expanded";

        private bool? _expanded = false;

        /// <summary>
        /// Sets and gets the IsExpanded property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool? Expanded
        {
            get
            {
                return _expanded;
            }

            set
            {
                if (_expanded == value)
                {
                    return;
                }
                RaisePropertyChanging(ExpandedPropertyName);
                _expanded = value;
                EditLinkTitle = _expanded == true ? "done" : "edit";
                RaisePropertyChanged(ExpandedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Enabled" /> property's name.
        /// </summary>
        public const string EnabledPropertyName = "Enabled";

        private bool _enabled = false;

        /// <summary>
        /// Sets and gets the Enabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                if (_enabled == value)
                {
                    return;
                }

                RaisePropertyChanging(EnabledPropertyName);
                _enabled = value;
                IsLoaderVisible = true;
                if (chatConfig != null)
                    chatConfig.Enabled = _enabled;

                if( chatDataService != null && ChatName != null )
                {
                    chatDataService.SwitchChat(ChatName, _enabled);
                }
                IsLoaderVisible = false;
                RaisePropertyChanged(EnabledPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsLoaderVisible" /> property's name.
        /// </summary>
        public const string IsLoaderVisiblePropertyName = "IsLoaderVisible";

        private bool _isLoaderVisible = false;

        /// <summary>
        /// Sets and gets the IsLoaderVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsLoaderVisible
        {
            get
            {
                return _isLoaderVisible;
            }

            set
            {
                if (_isLoaderVisible == value)
                {
                    return;
                }

                RaisePropertyChanging(IsLoaderVisiblePropertyName);
                _isLoaderVisible = value;
                RaisePropertyChanged(IsLoaderVisiblePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ChatIconURL" /> property's name.
        /// </summary>
        public const string ChatIconURLPropertyName = "ChatIconURL";

        private string _chatIconURL = Icons.MainIcon;

        /// <summary>
        /// Sets and gets the ChatIconURL property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ChatIconURL
        {
            get
            {
                return _chatIconURL;
            }

            set
            {
                if (_chatIconURL == value)
                {
                    return;
                }

                RaisePropertyChanging(ChatIconURLPropertyName);
                _chatIconURL = value;
                RaisePropertyChanged(ChatIconURLPropertyName);
            }
        }
    }
}
