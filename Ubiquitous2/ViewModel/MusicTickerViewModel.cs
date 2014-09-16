using GalaSoft.MvvmLight;
using UB.Model;
using UB.Utils;

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
        private ICurrentTrackDataService _dataService;
        /// <summary>
        /// Initializes a new instance of the MusicTickerViewModel class.
        /// </summary>
        public MusicTickerViewModel( ICurrentTrackDataService dataService )
        {
            _dataService = dataService;
            initialize();
        }
        private void initialize()
        {
            _dataService.TrackChangeHandler = (track) => UI.Dispatch(() => CurrentTrack = track);
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