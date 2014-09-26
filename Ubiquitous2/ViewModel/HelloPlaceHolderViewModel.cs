using GalaSoft.MvvmLight;

namespace UB.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class HelloPlaceHolderViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MvvmViewModel1 class.
        /// </summary>
        public HelloPlaceHolderViewModel()
        {
            MessengerInstance.Register<bool>(this, "SwitchHelloPlaceHolder", (isVisible) =>
            {
                IsPlaceHolderVisible = isVisible;
            });
        }

        /// <summary>
        /// The <see cref="IsPlaceHolderVisible" /> property's name.
        /// </summary>
        public const string IsPlaceHolderVisiblePropertyName = "IsPlaceHolderVisible";

        private bool _isPlaceHolderVisible = IsInDesignModeStatic?false:true;

        /// <summary>
        /// Sets and gets the IsPlaceHolderVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPlaceHolderVisible
        {
            get
            {
                return _isPlaceHolderVisible;
            }

            set
            {
                if (_isPlaceHolderVisible == value)
                {
                    return;
                }

                RaisePropertyChanging(IsPlaceHolderVisiblePropertyName);
                _isPlaceHolderVisible = value;
                RaisePropertyChanged(IsPlaceHolderVisiblePropertyName);
            }
        }
    }
}