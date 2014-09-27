using GalaSoft.MvvmLight;
using UB.Model;
namespace UB.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class StreamTopicSectionViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the StreamTopicSectionViewModel class.
        /// </summary>
        public StreamTopicSectionViewModel()
        {
        }

        /// <summary>
        /// The <see cref="StreamServiceIcon" /> property's name.
        /// </summary>
        public const string StreamServiceIconPropertyName = "StreamServiceIcon";

        private string _streamServiceIcon = Icons.DesignMainHeadsetIcon;

        /// <summary>
        /// Sets and gets the StreamServiceIcon property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string StreamServiceIcon
        {
            get
            {
                return _streamServiceIcon;
            }

            set
            {
                if (_streamServiceIcon == value)
                {
                    return;
                }

                RaisePropertyChanging(StreamServiceIconPropertyName);
                _streamServiceIcon = value;
                RaisePropertyChanged(StreamServiceIconPropertyName);
            }
        }
    }
}