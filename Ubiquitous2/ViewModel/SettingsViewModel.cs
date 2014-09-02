using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using UB.Model;

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
        /// <summary>
        /// Initializes a new instance of the SettingsViewModel class.
        /// </summary>
        public SettingsViewModel()
        {
            
        }

        [PreferredConstructor]
        public SettingsViewModel( ISettingsDataService dataService )
        {
            settingsDataService = dataService;

            settingsDataService.GetSettings("chats", (list) => {
                foreach( dynamic chat in list )
                {
                    Chats.Add(new SettingsChatItemViewModel() { 
                        ChatName = chat.ChatName,
                        Enabled = chat.Enabled,                    
                    });
                }
            });
                 
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
            return settings.ShowDialog();
        }
    }
}