using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devart.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using UB.Model;

namespace UB.ViewModel
{
    public class SettingsSectionViewModel : ViewModelBase, IHeightMeasurer
    {
        private IService _service;
        public SettingsSectionViewModel(IService service)
        {
            _service = service;
            _enabled = _service.Config.Enabled;
            _name = _service.Config.ServiceName;
            _iconURL = service.Config.IconURL;

            foreach( var parameter in service.Config.Parameters )
            {
                if (parameter.IsVisible)
                    SettingsFields.Add(new SettingsFieldViewModel(parameter));
            }

            //chatDataService = ServiceLocator.Current.GetInstance<IChatDataService>();
            //chatConfig = config;
            //_enabled = config.Enabled;
            //_chatName = config.ChatName;
            //_chatIconURL = config.IconURL ?? _chatIconURL;
            //foreach (var param in config.Parameters)
            //{
            //    if (param.IsVisible)
            //        SettingsFields.Add(
            //            new SettingsFieldViewModel(param)
            //        );
            //}        
        }

        /// <summary>
        /// The <see cref="Name" /> property's name.
        /// </summary>
        public const string NamePropertyName = "ServiceName";

        private String _name = "LoremIpsum";

        /// <summary>
        /// Sets and gets the ChatName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public String Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (_name == value)
                {
                    return;
                }

                RaisePropertyChanging(NamePropertyName);
                _name = value;
                RaisePropertyChanged(NamePropertyName);
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

        private RelayCommand _restart;

        /// <summary>
        /// Gets the RestartChat.
        /// </summary>
        public RelayCommand Restart
        {
            get
            {
                return _restart
                    ?? (_restart = new RelayCommand(
                                          () =>
                                          {
                                              Task.Factory.StartNew(() =>
                                              {
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
                if (_service.Config != null)
                    _service.Config.Enabled = _enabled;

                if (_enabled)
                    _service.Stop();
                else
                    _service.Start();

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
        /// The <see cref="IconURL" /> property's name.
        /// </summary>
        public const string IconURLPropertyName = "IconURL";

        private string _iconURL = Icons.MainIcon;

        /// <summary>
        /// Sets and gets the ChatIconURL property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string IconURL
        {
            get
            {
                return _iconURL;
            }

            set
            {
                if (_iconURL == value)
                {
                    return;
                }

                RaisePropertyChanging(IconURLPropertyName);
                _iconURL = value;
                RaisePropertyChanged(IconURLPropertyName);
            }
        }

        public double GetEstimatedHeight(double availableWidth)
        {
            return 30;
        }
    }
}
