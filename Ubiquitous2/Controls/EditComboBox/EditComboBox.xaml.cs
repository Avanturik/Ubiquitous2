using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UB.Utils;

namespace UB.Controls
{
    /// <summary>
    /// Interaction logic for EditComboBox.xaml
    /// </summary>
    public partial class EditComboBox : UserControl
    {
        private bool isActionDisabled = false;
        private EditComboBoxItem lastSelection = null;
        private EditComboBoxActions comboActions;
        private const int StandardCommandsNumber = 2;
        public EditComboBox()
        {
            InitializeComponent();
            
        }


        /// <summary>
        /// The <see cref="ItemsSource" /> dependency property's name.
        /// </summary>
        public const string ItemsSourcePropertyName = "ItemsSource";

        /// <summary>
        /// Gets or sets the value of the <see cref="ItemsSource" />
        /// property. This is a dependency property.
        /// </summary>
        public ObservableCollection<EditComboBoxItem> ItemsSource
        {
            get
            {
                return (ObservableCollection<EditComboBoxItem>)GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="ItemsSource" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            ItemsSourcePropertyName,
            typeof(ObservableCollection<EditComboBoxItem>),
            typeof(EditComboBox),
            new UIPropertyMetadata(null, (o, e) => {
                var editCombo = o as EditComboBox;
                if( editCombo != null)
                    editCombo.comboActions = new EditComboBoxActions(editCombo);
            }));

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
            typeof(EditComboBox),
            new UIPropertyMetadata(null));

    }
}
