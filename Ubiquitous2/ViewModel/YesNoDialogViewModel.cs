using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using UB.Model;

namespace UB.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class YesNoDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the YesNoDialogViewModel class.
        /// </summary>
        public YesNoDialogViewModel()
        {
            MessengerInstance.Register<YesNoDialog>(this, "OpenDialog", (request) => {
                if (request.IsOpenRequest)
                {
                    Request = request;
                    IsVisible = true;                    
                }
            });
        }

        /// <summary>
        /// The <see cref="Request" /> property's name.
        /// </summary>
        public const string RequestPropertyName = "Request";

        private YesNoDialog _request = new YesNoDialog() { QuestionText = "Close?" };

        /// <summary>
        /// Sets and gets the Request property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public YesNoDialog Request
        {
            get
            {
                return _request;
            }

            set
            {
                if (_request == value)
                {
                    return;
                }

                RaisePropertyChanging(RequestPropertyName);
                _request = value;
                RaisePropertyChanged(RequestPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsVisible" /> property's name.
        /// </summary>
        public const string IsVisiblePropertyName = "IsVisible";

        private bool _isVisible = false;

        /// <summary>
        /// Sets and gets the IsVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }

            set
            {
                if (_isVisible == value)
                {
                    return;
                }

                RaisePropertyChanging(IsVisiblePropertyName);
                _isVisible = value;
                RaisePropertyChanged(IsVisiblePropertyName);
            }
        }

        private RelayCommand _yesCommand;

        /// <summary>
        /// Gets the YesCommand.
        /// </summary>
        public RelayCommand YesCommand
        {
            get
            {
                return _yesCommand
                    ?? (_yesCommand = new RelayCommand(
                                          () =>
                                          {
                                              Request.Result = true;
                                              ProcessResult();

                                          }));
            }
        }


        private RelayCommand _noCommand;

        /// <summary>
        /// Gets the NoCommand.
        /// </summary>
        public RelayCommand NoCommand
        {
            get
            {
                return _noCommand
                    ?? (_noCommand = new RelayCommand(
                                          () =>
                                          {
                                              Request.Result = false;
                                              ProcessResult();
                                          }));
            }
        }

        private void ProcessResult()
        {
            IsVisible = false;
            if (Request.Result && Request.YesAction != null)
                Request.YesAction();
            else if( !Request.Result && Request.NoAction != null )
                Request.NoAction();

            IsVisible = false;
            //MessengerInstance.Send<YesNoDialog>(Request, "DialogResult");
        }
    }
}