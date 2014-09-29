using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Command;
using UB.Utils;

namespace UB.Controls
{
   
    [TemplatePart(Name="PART_Popup", Type=typeof(Popup))]
    /// <summary>
    /// Interaction logic for EditBox.xaml
    /// </summary>
    public partial class EditBox : UserControl
    {
        private bool isFirstChange = true;

        public EditBox()
        {
            InitializeComponent();
            SuggestionsListBox.SelectionChanged += (o,e) => {
                if( PART_Edit.Text != SelectedSuggestion && !String.IsNullOrWhiteSpace(SelectedSuggestion))
                    PART_Edit.Text = SelectedSuggestion;

                PART_Popup.IsOpen = false;
            };

            PART_Edit.TextChanged += (o, e) => {
                        if( !isFirstChange && PART_Edit.Text != 
                            SelectedSuggestion && EnableSuggestion && 
                            CommandNeedSuggestion != null && PART_Edit.Text.Length > 2 )
                        {
                                    CommandNeedSuggestion.Execute(null);
                                    PART_Popup.IsOpen = true;                            
                        }
                        isFirstChange = false;
            };
        }

        /// <summary>
        /// The <see cref="EnableSuggestion" /> dependency property's name.
        /// </summary>
        public const string EnableSuggestionPropertyName = "EnableSuggestion";

        /// <summary>
        /// Gets or sets the value of the <see cref="EnableSuggestion" />
        /// property. This is a dependency property.
        /// </summary>
        public bool EnableSuggestion
        {
            get
            {
                return (bool)GetValue(EnableSuggestionProperty);
            }
            set
            {
                SetValue(EnableSuggestionProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="EnableSuggestion" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableSuggestionProperty = DependencyProperty.Register(
            EnableSuggestionPropertyName,
            typeof(bool),
            typeof(EditBox),
            new UIPropertyMetadata(false));

        /// <summary>
        /// The <see cref="CommandNeedSuggestion" /> dependency property's name.
        /// </summary>
        public const string CommandNeedSuggestionPropertyName = "CommandNeedSuggestion";

        /// <summary>
        /// Gets or sets the value of the <see cref="CommandNeedSuggestion" />
        /// property. This is a dependency property.
        /// </summary>
        public RelayCommand CommandNeedSuggestion
        {
            get
            {
                return (RelayCommand)GetValue(CommandNeedSuggestionProperty);
            }
            set
            {
                SetValue(CommandNeedSuggestionProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="CommandNeedSuggestion" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandNeedSuggestionProperty = DependencyProperty.Register(
            CommandNeedSuggestionPropertyName,
            typeof(RelayCommand),
            typeof(EditBox),
            new UIPropertyMetadata(null));

        /// <summary>
        /// The <see cref="SelectedSuggestion" /> dependency property's name.
        /// </summary>
        public const string SelectedSuggestionPropertyName = "SelectedSuggestion";

        /// <summary>
        /// Gets or sets the value of the <see cref="SelectedSuggestion" />
        /// property. This is a dependency property.
        /// </summary>
        public string SelectedSuggestion
        {
            get
            {
                return (string)GetValue(SelectedSuggestionProperty);
            }
            set
            {
                SetValue(SelectedSuggestionProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="SelectedSuggestion" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedSuggestionProperty = DependencyProperty.Register(
            SelectedSuggestionPropertyName,
            typeof(string),
            typeof(EditBox),
            new UIPropertyMetadata(null));

        /// <summary>
        /// The <see cref="Suggestions" /> dependency property's name.
        /// </summary>
        public const string SuggestionsPropertyName = "Suggestions";

        /// <summary>
        /// Gets or sets the value of the <see cref="Suggestions" />
        /// property. This is a dependency property.
        /// </summary>
        public ObservableCollection<string> Suggestions
        {
            get
            {
                return (ObservableCollection<string>)GetValue(SuggestionsProperty);
            }
            set
            {
                SetValue(SuggestionsProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Suggestions" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SuggestionsProperty = DependencyProperty.Register(
            SuggestionsPropertyName,
            typeof(ObservableCollection<string>),
            typeof(EditBox),
            new UIPropertyMetadata(null));

        /// <summary>
        /// The <see cref="Watermark" /> dependency property's name.
        /// </summary>
        public const string WatermarkPropertyName = "Watermark";

        /// <summary>
        /// Gets or sets the value of the <see cref="Watermark" />
        /// property. This is a dependency property.
        /// </summary>
        public string Watermark
        {
            get
            {
                return (string)GetValue(WatermarkProperty);
            }
            set
            {
                SetValue(WatermarkProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Watermark" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(
            WatermarkPropertyName,
            typeof(string),
            typeof(EditBox),
            new UIPropertyMetadata(null));

        /// <summary>
        /// The <see cref="Text" /> dependency property's name.
        /// </summary>
        public const string TextPropertyName = "Text";

        /// <summary>
        /// Gets or sets the value of the <see cref="Text" />
        /// property. This is a dependency property.
        /// </summary>
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Text" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            TextPropertyName,
            typeof(string),
            typeof(EditBox),
            new UIPropertyMetadata(null));
    }
}
