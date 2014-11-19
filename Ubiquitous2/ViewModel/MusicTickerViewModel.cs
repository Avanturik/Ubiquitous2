using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using UB.Model;
using UB.Utils;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using UB.View;

namespace UB.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MusicTickerViewModel : ViewModelBase
    {
        private IGeneralDataService _dataService;
        private IService _lastFmService;
        private IService imageService;
        /// <summary>
        /// Initializes a new instance of the MusicTickerViewModel class.
        /// </summary>
        [PreferredConstructor]
        public MusicTickerViewModel( IGeneralDataService dataService)
        {
            _dataService = dataService;
            initialize();
        }
        private void initialize()
        {
            imageService = _dataService.GetService(SettingsRegistry.ServiceTitleImageSaver);
            ImageServiceConfig = imageService.Config;
            MusicTickerToImagePath = ImageServiceConfig.GetParameterValue("FilenameMusic") as string;

            _lastFmService = _dataService.Services.FirstOrDefault(service => service.Config.ServiceName == SettingsRegistry.ServiceTitleMusicTicker);
            _lastFmService.GetData((dataObject) =>
            {
                CurrentTrack = dataObject as MusicTrackInfo;
                CurrentTrack.PropertyChanged += (o,e) => {
                    if (e.PropertyName == MusicTrackInfo.ImagePropertyName)
                    {
                        if( imageService.Config.Enabled )
                        {
                            UI.Dispatch(() => {
                                IsNeedSave = true;
                                IsNeedSave = false;                        
                            });
                        }
                    }
                };
                    
            });
            Config = _lastFmService.Config;

            Config.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == ServiceConfig.EnabledPropertyName)
                {
                    if (Config.Enabled)
                        Win.ShowMusicTicker();
                    else
                        Win.HideMusicTicker();
                }
            };

            AppConfig = (Application.Current as App).AppConfig;

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
        /// The <see cref="IsNeedSave" /> property's name.
        /// </summary>
        public const string IsNeedSavePropertyName = "IsNeedSave";

        private bool _isNeedSave = false;

        /// <summary>
        /// Sets and gets the IsNeedSave property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsNeedSave
        {
            get
            {
                return _isNeedSave;
            }

            set
            {
                if (_isNeedSave == value)
                {
                    return;
                }
                _isNeedSave = value;
                RaisePropertyChanged(IsNeedSavePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ImageServiceConfig" /> property's name.
        /// </summary>
        public const string ImageServiceConfigPropertyName = "ImageServiceConfig";

        private ServiceConfig _chatToImageService = null;

        /// <summary>
        /// Sets and gets the ImageServiceConfig property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ServiceConfig ImageServiceConfig
        {
            get
            {
                return _chatToImageService;
            }

            set
            {
                if (_chatToImageService == value)
                {
                    return;
                }
                _chatToImageService = value;
                RaisePropertyChanged(ImageServiceConfigPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Config" /> property's name.
        /// </summary>
        public const string ConfigPropertyName = "Config";

        private ServiceConfig _config = null;

        /// <summary>
        /// Sets and gets the Config property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ServiceConfig Config
        {
            get
            {
                return _config;
            }

            set
            {
                if (_config == value)
                {
                    return;
                }
                _config = value;
                RaisePropertyChanged(ConfigPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="CurrentTrack" /> property's name.
        /// </summary>
        public const string CurrentTrackPropertyName = "CurrentTrack";

        private MusicTrackInfo _currentTrack = null;

        /// <summary>
        /// Sets and gets the CurrentTrack property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public MusicTrackInfo CurrentTrack
        {
            get
            {
                return _currentTrack;
            }

            set
            {
                if (_currentTrack == value)
                {
                    return;
                }
                _currentTrack = value;
                _currentTrack.PropertyChanged += (o, e) =>
                {
                    if (e.PropertyName == MusicTrackInfo.ImagePropertyName)
                    {
                        if( imageService.Config.Enabled )
                        {
                            UI.Dispatch(() =>
                            {
                                IsNeedSave = true;
                                IsNeedSave = false;
                            });
                        }
                    }
                };
                RaisePropertyChanged(CurrentTrackPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="MusicTickerToImagePath" /> property's name.
        /// </summary>
        public const string MusicTickerToImagePathPropertyName = "MusicTickerToImagePath";

        private string _musicTickerToImagePath = null;

        /// <summary>
        /// Sets and gets the MusicTickerToImagePath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string MusicTickerToImagePath
        {
            get
            {
                return _musicTickerToImagePath;
            }

            set
            {
                if (_musicTickerToImagePath == value)
                {
                    return;
                }
                _musicTickerToImagePath = value;
                imageService.Config.SetParameterValue("FilenameMusic", _musicTickerToImagePath);
                RaisePropertyChanged(MusicTickerToImagePathPropertyName);
            }
        }

    }
}