using System.Collections.ObjectModel;
using System.Threading;
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
    public class SubFollowDonationViewModel : ViewModelBase
    {
        private IGreetingsDataService dataService;
        /// <summary>
        /// Initializes a new instance of the SubFollowDonationViewModel class.
        /// </summary>
        public SubFollowDonationViewModel( IGreetingsDataService dataService  )
        {
            this.dataService = dataService;
            this.dataService.GetGreetings( (greetings) => {
                ShowGreeting = false;
                ShowGreeting = true;
                greetings.ForEach(greet => Greetings.Add(new Greeting(greet.Title, greet.Message)));
            });
        }

        /// <summary>
        /// The <see cref="Greetings" /> property's name.
        /// </summary>
        public const string GreetingsPropertyName = "Greetings";

        private ObservableCollection<dynamic> _greetings = new ObservableCollection<dynamic>();

        /// <summary>
        /// Sets and gets the Greetings property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<dynamic> Greetings
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