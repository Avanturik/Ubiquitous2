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
        private MusicTickerWindow tickerWindow;
        private ISettingsDataService settingsDataService;
        [PreferredConstructor]
        public SettingsViewModel( SettingsDataService dataService, IGeneralDataService generalDataService )
        {
            WebServerPort = Ubiquitous.Default.WebServerPort;

            settingsDataService = dataService;

            settingsDataService.GetChatSettings((list) => {
                foreach( ChatConfig chatConfig in list )
                {
                    Chats.Add(new SettingsChatItemViewModel(chatConfig));
                }
            });

            var serviceList = generalDataService.Services.Select(service => new SettingsServiceItemViewModel(service));
            foreach( IService service in serviceList)
            {
                ServiceItemViewModels.Add(new SettingsServiceItemViewModel(service));
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
        /// The <see cref="WebServerPort" /> property's name.
        /// </summary>
        public const string WebServerPortPropertyName = "WebServerPort";

        private int _webServerPort = 8080;

        /// <summary>
        /// Sets and gets the WebServerPort property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int WebServerPort
        {
            get
            {
                return _webServerPort;
            }

            set
            {
                if (_webServerPort == value)
                {
                    return;
                }
                RaisePropertyChanging(WebServerPortPropertyName);
                _webServerPort = value;
                Ubiquitous.Default.WebServerPort = value;
                LocalWebURL = "http://localhost:" + _webServerPort;
                RaisePropertyChanged(WebServerPortPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="LocalWebURL" /> property's name.
        /// </summary>
        public const string LocalWebURLPropertyName = "LocalWebURL";

        private string _localWebURL = "http://localhost:8080";

        /// <summary>
        /// Sets and gets the LocalWebURL property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string LocalWebURL
        {
            get
            {
                return _localWebURL;
            }

            set
            {
                if (_localWebURL == value)
                {
                    return;
                }

                RaisePropertyChanging(LocalWebURLPropertyName);
                _localWebURL = value;
                RaisePropertyChanged(LocalWebURLPropertyName);
            }
        }

        private RelayCommand _openLocalHost;

        /// <summary>
        /// Gets the OpenLocalHost.
        /// </summary>
        public RelayCommand OpenLocalHost
        {
            get
            {
                return _openLocalHost
                    ?? (_openLocalHost = new RelayCommand(
                                          () =>
                                          {
                                              Process.Start(LocalWebURL);
                                          }));
            }
        }

        /// <summary>
        /// The <see cref="ServiceItemViewModels" /> property's name.
        /// </summary>
        public const string ServiceItemViewModelsPropertyName = "ServiceItemViewModels";

        private ObservableCollection<SettingsServiceItemViewModel> _serviceItemViewModels = new ObservableCollection<SettingsServiceItemViewModel>();

        /// <summary>
        /// Sets and gets the ServiceItemViewModels property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<SettingsServiceItemViewModel> ServiceItemViewModels
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


        private RelayCommand<PasswordBox> _lastFMPasswordChanged;

        /// <summary>
        /// Gets the LastFMPasswordChanged.
        /// </summary>
        public RelayCommand<PasswordBox> LastFMPasswordChanged
        {
            get
            {
                return _lastFMPasswordChanged
                    ?? (_lastFMPasswordChanged = new RelayCommand<PasswordBox>(
                                          (box) =>
                                          {
                                              Properties.Ubiquitous.Default.LastFMPassword = box.Password;
                                          }));
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