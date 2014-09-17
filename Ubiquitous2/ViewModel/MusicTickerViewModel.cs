using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using UB.Model;
using UB.Utils;
using System.Linq;
using System.Threading.Tasks;

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
        private GeneralDataService _dataService;
        private IService _lastFmService;
        /// <summary>
        /// Initializes a new instance of the MusicTickerViewModel class.
        /// </summary>
        [PreferredConstructor]
        public MusicTickerViewModel( GeneralDataService dataService)
        {
            _dataService = dataService;
            initialize();
        }
        private void initialize()
        {
            _lastFmService = _dataService.Services.FirstOrDefault(service => service.Config.ServiceName == SettingsRegistry.ServiceTitleMusicTicker);
            _lastFmService.GetData((dataObject) =>
            {
                CurrentTrack = dataObject as MusicTrackInfo;
            });
            Config = _lastFmService.Config;
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

                RaisePropertyChanging(ConfigPropertyName);
                _config = value;
                RaisePropertyChanged(ConfigPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="CurrentTrack" /> property's name.
        /// </summary>
        public const string CurrentTrackPropertyName = "CurrentTrack";

        private MusicTrackInfo _myProperty = null;

        /// <summary>
        /// Sets and gets the CurrentTrack property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public MusicTrackInfo CurrentTrack
        {
            get
            {
                return _myProperty;
            }

            set
            {
                if (_myProperty == value)
                {
                    return;
                }

                RaisePropertyChanging(CurrentTrackPropertyName);
                _myProperty = value;
                RaisePropertyChanged(CurrentTrackPropertyName);
            }
        }

    }
}