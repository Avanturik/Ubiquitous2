using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using UB.Utils;

namespace UB.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class EditBoxViewModel : ViewModelBase
    {
        private object lockUpdateSuggestions = new object();
        /// <summary>
        /// Initializes a new instance of the MvvmViewModel1 class.
        /// </summary>
        [PreferredConstructor]
        public EditBoxViewModel()
        {

        }

        public Func<string, ObservableCollection<string>> UpdateSuggestions { get; set; }

        /// <summary>
        /// The <see cref="Suggestions" /> property's name.
        /// </summary>
        public const string SuggestionsPropertyName = "Suggestions";

        private ObservableCollection<string> _suggestions = new ObservableCollection<string>();

        /// <summary>
        /// Sets and gets the Suggestions property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<string> Suggestions
        {
            get
            {
                return _suggestions;
            }

            set
            {
                if (_suggestions == value)
                {
                    return;
                }

                RaisePropertyChanging(SuggestionsPropertyName);
                _suggestions = value;
                RaisePropertyChanged(SuggestionsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Watermark" /> property's name.
        /// </summary>
        public const string WatermarkPropertyName = "Watermark";

        private string _string = "watermark";

        /// <summary>
        /// Sets and gets the Watermark property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Watermark
        {
            get
            {
                return _string;
            }

            set
            {
                if (_string == value)
                {
                    return;
                }

                RaisePropertyChanging(WatermarkPropertyName);
                _string = value;
                RaisePropertyChanged(WatermarkPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Text" /> property's name.
        /// </summary>
        public const string TextPropertyName = "Text";

        private string _text = null;

        /// <summary>
        /// Sets and gets the Text property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                if (_text == value)
                {
                    return;
                }

                RaisePropertyChanging(TextPropertyName);
                _text = value;
                RaisePropertyChanged(TextPropertyName);

                Task.Factory.StartNew(() =>
                {
                    lock (lockUpdateSuggestions)
                        UI.Dispatch(() => {
                            if( UpdateSuggestions != null)
                                Suggestions = UpdateSuggestions(_text);
                        });
                });
            }
        }

        private RelayCommand<string> _suggestionSelect;

        /// <summary>
        /// Gets the SuggestionSelect.
        /// </summary>
        public RelayCommand<string> SuggestionSelect
        {
            get
            {
                return _suggestionSelect
                    ?? (_suggestionSelect = new RelayCommand<string>(
                                          (selectedValue) =>
                                          {
                                              
                                          }));
            }
        }
    }
}