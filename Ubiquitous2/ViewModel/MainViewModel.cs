using System.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using UB.Model;

namespace UB.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IChatDataService _dataService;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IChatDataService dataService)
        {
            _dataService = dataService;
            var test = Properties.Ubiqiutous.Default.LoremIpsum;
            Properties.Ubiqiutous.Default.Save();

        }


        private RelayCommand _addMessage;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand AddMessage
        {
            get
            {
                return _addMessage
                    ?? (_addMessage = new RelayCommand(
                                          () =>
                                          {
                                              _dataService.GetRandomMessage((item, error) => {
                                                  MessengerInstance.Send<ChatMessage>(item);
                                              });
                                          }));
            }
        }

        private RelayCommand _showSettings;

        /// <summary>
        /// Gets the ShowSettings.
        /// </summary>
        public RelayCommand ShowSettings
        {
            get
            {
                return _showSettings
                    ?? (_showSettings = new RelayCommand(
                                          () =>
                                          {
                                              var settings = new SettingsViewModel();
                                              settings.Show();
                                          }));
            }
        }

        private RelayCommand _exitApplication;

        /// <summary>
        /// Gets the ExitApplication.
        /// </summary>
        public RelayCommand ExitApplication
        {
            get
            {
                return _exitApplication
                    ?? (_exitApplication = new RelayCommand(
                                          () =>
                                          {
                                              Properties.Ubiqiutous.Default.Save();
                                          }));
            }
        }
        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}