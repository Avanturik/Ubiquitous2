using System.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
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
                                              var settings = ServiceLocator.Current.GetInstance<SettingsViewModel>();
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


        /// <summary>
        /// The <see cref="IsOverlayVisible" /> property's name.
        /// </summary>
        public const string IsOverlayVisiblePropertyName = "IsOverlayVisible";

        private bool _isOverlayVisible = false;

        /// <summary>
        /// Sets and gets the IsOverlayVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsOverlayVisible
        {
            get
            {
                return _isOverlayVisible;
            }

            set
            {
                if (_isOverlayVisible == value)
                {
                    return;
                }

                RaisePropertyChanging(IsOverlayVisiblePropertyName);
                _isOverlayVisible = value;
                RaisePropertyChanged(IsOverlayVisiblePropertyName);
            }
        }

        private RelayCommand _hideOverlay;

        /// <summary>
        /// Gets the HideOverlay.
        /// </summary>
        public RelayCommand HideOverlay
        {
            get
            {
                return _hideOverlay
                    ?? (_hideOverlay = new RelayCommand(
                                          () =>
                                          {
                                              IsOverlayVisible = false;
                                          }));
            }
        }

        private RelayCommand _showOverlay;

        /// <summary>
        /// Gets the ShowOverlay.
        /// </summary>
        public RelayCommand ShowOverlay
        {
            get
            {
                return _showOverlay
                    ?? (_showOverlay = new RelayCommand(
                                          () =>
                                          {
                                              IsOverlayVisible = true;
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