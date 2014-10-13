using System.Collections.ObjectModel;
using System.Threading;
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
    public class SubFollowDonationViewModel : ViewModelBase
    {
        private IGreetingsDataService dataService;
        private Timer greetingTimer;
        /// <summary>
        /// Initializes a new instance of the SubFollowDonationViewModel class.
        /// </summary>
        public SubFollowDonationViewModel( IGreetingsDataService dataService  )
        {
            this.dataService = dataService;
            greetingTimer = new Timer((sender) => {
                this.dataService.GetGreetings((greeting) =>
                {
                    ShowGreeting = false;
                    ShowGreeting = true;
                    UI.Dispatch(() => Greetings.Add(greeting));
                });
            }, this, 10000, 10000);
        }

        /// <summary>
        /// The <see cref="Greetings" /> property's name.
        /// </summary>
        public const string GreetingsPropertyName = "Greetings";

        private ObservableCollection<Greeting> _greetings = new ObservableCollection<Greeting>();

        /// <summary>
        /// Sets and gets the Greetings property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Greeting> Greetings
        {
            get
            {
                return _greetings;
            }

            set
            {
                if (_greetings == value)
                {
                    return;
                }
                _greetings = value;
                RaisePropertyChanged(GreetingsPropertyName);
            }

        }

        /// <summary>
        /// The <see cref="ShowGreeting" /> property's name.
        /// </summary>
        public const string ShowGreetingPropertyName = "ShowGreeting";

        private bool _showGreeting = false;

        /// <summary>
        /// Sets and gets the ShowGreeting property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool ShowGreeting
        {
            get
            {
                return _showGreeting;
            }

            set
            {
                if (_showGreeting == value)
                {
                    return;
                }
                _showGreeting = value;
                RaisePropertyChanged(ShowGreetingPropertyName);
            }
        }
    }
}