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
using UB.Model;

namespace UB.ViewModel
{
    public class SettingsChatItemViewModel : ViewModelBase, IHeightMeasurer
    {
        public bool Enabled { get; set; }
        public String ChatName { get; set; }
        private ISettingsDataService _dataService;
        
        [PreferredConstructor]
        public SettingsChatItemViewModel(ISettingsDataService dataService)
        {
                    _dataService = dataService;
                    _dataService.GetRandomChatSetting(
                    (item) =>
                    {
                        Enabled = item.Enabled;
                        ChatName = item.ChatName;
                    });

                    SettingsFields.Add(
                        new SettingsFieldViewModel("Text", "User name:", "loremIpsumUser")
                        );
        }

        public SettingsChatItemViewModel(ChatConfig config)
        {
            Enabled = config.Enabled;
            ChatName = config.ChatName;
            foreach( var param in config.Parameters )
            {
                SettingsFields.Add(
                        new SettingsFieldViewModel(param.DataType, param.Label, param.Value)
                    );
            }


        }
        public double GetEstimatedHeight(double availableWidth)
        {
            return 30;
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
    }
}
