using System.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
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
        [PreferredConstructor]
        public MainViewModel(IChatDataService dataService)
        {
            _dataService = dataService;
            SendText = dataService.GetRandomText();
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
        /// <summary>
        /// The <see cref="IsMouseOver" /> property's name.
        /// </summary>
        public const string IsMouseOverPropertyName = "IsMouseOver";

        private bool _isMouseOver = false;

        /// <summary>
        /// Sets and gets the IsMouseOver property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsMouseOver
        {
            get
            {
                return _isMouseOver;
            }

            set
            {
                if (_isMouseOver == value)
                {
                    return;
                }

                RaisePropertyChanging(IsMouseOverPropertyName);
                _isMouseOver = value;
                RaisePropertyChanged(IsMouseOverPropertyName);
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
                                              IsMouseOver = false;
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
                                                IsMouseOver = true;
                                                if (IsFocused)
                                                    IsOverlayVisible = true;
                                          }));
            }
        }


        /// <summary>
        /// The <see cref="SendText" /> property's name.
        /// </summary>
        public const string SendTextPropertyName = "SendText";

        private string _sendText = "";

        /// <summary>
        /// Sets and gets the SendText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SendText
        {
            get
            {
                return _sendText;
            }

            set
            {
                if (_sendText == value)
                {
                    return;
                }

                RaisePropertyChanging(SendTextPropertyName);
                _sendText = value;
                RaisePropertyChanged(SendTextPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsFocused" /> property's name.
        /// </summary>
        public const string IsFocusedPropertyName = "IsFocused";

        private bool _isFocused = true;

        /// <summary>
        /// Sets and gets the IsFocused property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsFocused
        {
            get
            {
                return _isFocused;
            }

            set
            {
                if (_isFocused == value)
                {
                    return;
                }

                RaisePropertyChanging(IsFocusedPropertyName);
                _isFocused = value;
                RaisePropertyChanged(IsFocusedPropertyName);
            }
        }

        private RelayCommand _setFocused;

        /// <summary>
        /// Gets the SetFocused.
        /// </summary>
        public RelayCommand SetFocused
        {
            get
            {
                return _setFocused
                    ?? (_setFocused = new RelayCommand(
                                          () =>
                                          {
                                              IsFocused = true;
                                              if (IsMouseOver)
                                                  IsOverlayVisible = true;
                                          }));
            }
        }

        private RelayCommand _setUnFocused;

        /// <summary>
        /// Gets the SetUnFocused.
        /// </summary>
        public RelayCommand SetUnFocused
        {
            get
            {
                return _setUnFocused
                    ?? (_setUnFocused = new RelayCommand(
                                          () =>
                                          {
                                              IsFocused = false;
                                              IsOverlayVisible = false;
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