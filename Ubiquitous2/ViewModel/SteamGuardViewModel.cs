using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using UB.Model;
using UB.Properties;
namespace UB.ViewModel
{
    public class SteamGuardViewModel : ViewModelBase
    {

        private RelayCommand<Window> _cancelCommand;
        private SteamChat steamChat;
        public SteamGuardViewModel( IChatDataService dataService)
        {
            steamChat = (SteamChat)dataService.Chats.FirstOrDefault(chat => chat.ChatName.Equals(SettingsRegistry.ChatTitleSteam));
        }
        /// <summary>
        /// Gets the CancelCommand.
        /// </summary>
        public RelayCommand<Window> CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand<Window>((window) => {

                        if (steamChat != null)
                            steamChat.SteamGuardKey = "Cancel";

                        window.Close();
                    }));
            }
        }



        private RelayCommand<Window> _okCommand;

        /// <summary>
        /// Gets the OKCommand.
        /// </summary>
        public RelayCommand<Window> OKCommand
        {
            get
            {
                return _okCommand
                    ?? (_okCommand = new RelayCommand<Window>((window) =>
                    {
                        if (!String.IsNullOrWhiteSpace(Code))
                        {
                            if (steamChat != null)
                                steamChat.SteamGuardKey = Code;

                            window.Close();
                        }
                    }));
            }
        }
        /// <summary>
        /// The <see cref="Code" /> property's name.
        /// </summary>
        public const string CodePropertyName = "Code";

        private string _code = null;

        /// <summary>
        /// Sets and gets the Code property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Code
        {
            get
            {
                return _code;
            }

            set
            {
                if (_code == value)
                {
                    return;
                }

                RaisePropertyChanging(CodePropertyName);
                _code = value;
                RaisePropertyChanged(CodePropertyName);
            }
        }
    }
}
