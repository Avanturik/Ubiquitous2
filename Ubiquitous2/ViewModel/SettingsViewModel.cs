using System;
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
        public SettingsViewModel(ISettingsDataService dataService, IGeneralDataService generalDataService)
        {
            settingsDataService = dataService;
            CurrentTheme = Ubiquitous.Default.Config.AppConfig.ThemeName;
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

            AppConfig = Ubiquitous.Default.Config.AppConfig;
        }
        /// <summary>
        /// The <see cref="CurrentTheme" /> property's name.
        /// </summary>
        public const string CurrentThemePropertyName = "CurrentTheme";

        private string _currentTheme = null;

        /// <summary>
        /// Sets and gets the CurrentTheme property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string CurrentTheme
        {
            get
            {
                return _currentTheme;
            }

            set
            {
                if (_currentTheme == value)
                {
                    return;
                }
                _currentTheme = value;
                RaisePropertyChanged(CurrentThemePropertyName);
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
                                              CurrentTheme = null;
                                              Theme.SwitchTheme(themeName);
                                              CurrentTheme = themeName;
                                          }));
            }
        }

        private RelayCommand _reopenMainWindow;

        /// <summary>
        /// Gets the ReopenMainWindow.
        /// </summary>
        public RelayCommand ReopenMainWindow
        {
            get
            {
                return _reopenMainWindow
                    ?? (_reopenMainWindow = new RelayCommand(
                                          () =>
                                          {
                                              MessengerInstance.Send<bool>(true, "ReopenMainWindow");
                                              Application.Current.MainWindow.Close();
                                              Application.Current.MainWindow = new MainWindow();
                                              Application.Current.MainWindow.Show();
                                              Win.ReloadAllWindows();
                                          }));
            }
        }

        /// <summary>
        /// The <see cref="AppConfig" /> property's name.
        /// </summary>
        public const string AppConfigPropertyName = "AppConfig";

        private AppConfig _appConfig = new AppConfig();

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

        /// <summary>
        /// The <see cref="PersonalizationFields" /> property's name.
        /// </summary>
        public const string PersonalizationFieldsPropertyName = "PersonalizationFields";

        private ObservableCollection<SettingsFieldViewModel> _personalizationFields = new ObservableCollection<SettingsFieldViewModel>();

        /// <summary>
        /// Sets and gets the PersonalizationFields property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SettingsFieldViewModel> PersonalizationFields
        {
            get
            {
                return _personalizationFields;
            }

            set
            {
                if (_personalizationFields == value)
                {
                    return;
                }
                _personalizationFields = value;
                RaisePropertyChanged(PersonalizationFieldsPropertyName);
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
                _chats = value;
                RaisePropertyChanged(ChatsPropertyName);
            }
        
        }
    }
}