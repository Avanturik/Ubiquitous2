﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using UB.Model;
using UB.Properties;
using UB.Utils;
using UB.View;
using System.Linq;

namespace UB.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private ISettingsDataService settingsDataService;
        [PreferredConstructor]
        public SettingsViewModel(SettingsDataService dataService, GeneralDataService generalDataService)
        {
            settingsDataService = dataService;

            settingsDataService.GetChatSettings((list) => {
                foreach( ChatConfig chatConfig in list )
                {
                    Chats.Add(new SettingsChatItemViewModel(chatConfig));
                }
            });

            var serviceList = generalDataService.Services.Select(service => new SettingsSectionViewModel(service));
            foreach( var service in serviceList)
            {
                ServiceItemViewModels.Add(service);
            }
        }

        private RelayCommand<string> _selectTheme;

        /// <summary>
        /// Gets the SelectTheme.
        /// </summary>
        public RelayCommand<string> SelectTheme
        {
            get
            {
                return _selectTheme
                    ?? (_selectTheme = new RelayCommand<string>(
                                          (themeName) =>
                                          {
                                              Application.Current.Resources.MergedDictionaries.Clear();
                                              Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
                                              {
                                                  Source = new Uri("/Ubiquitous2;component/Skins/" + themeName + "/Skin.xaml", UriKind.RelativeOrAbsolute)
                                              });
                                          }));
            }
        }


        /// <summary>
        /// The <see cref="ServiceItemViewModels" /> property's name.
        /// </summary>
        public const string ServiceItemViewModelsPropertyName = "ServiceItemViewModels";

        private ObservableCollection<SettingsSectionViewModel> _serviceItemViewModels = new ObservableCollection<SettingsSectionViewModel>();

        /// <summary>
        /// Sets and gets the ServiceItemViewModels property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SettingsSectionViewModel> ServiceItemViewModels
        {
            get
            {
                return _serviceItemViewModels;
            }

            set
            {
                if (_serviceItemViewModels == value)
                {
                    return;
                }

                RaisePropertyChanging(ServiceItemViewModelsPropertyName);
                _serviceItemViewModels = value;
                RaisePropertyChanged(ServiceItemViewModelsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Chats" /> property's name.
        /// </summary>
        public const string ChatsPropertyName = "Chats";

        private ObservableCollection<SettingsChatItemViewModel> _chats = new ObservableCollection<SettingsChatItemViewModel>();

        /// <summary>
        /// Sets and gets the Chats property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SettingsChatItemViewModel> Chats
        {
            get
            {
                return _chats;
            }

            set
            {
                if (_chats == value)
                {
                    return;
                }

                RaisePropertyChanging(ChatsPropertyName);
                _chats = value;
                RaisePropertyChanged(ChatsPropertyName);
            }
        
        }

        public bool? Show()
        {
            var settings = new SettingsWindow();
            settings.Owner = Application.Current.MainWindow;
            return settings.ShowDialog();
        }
    }
}