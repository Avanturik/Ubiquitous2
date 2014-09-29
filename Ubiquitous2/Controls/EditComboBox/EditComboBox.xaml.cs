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

namespace UB.Controls
{
    /// <summary>
    /// Interaction logic for EditComboBox.xaml
    /// </summary>
    public partial class EditComboBox : UserControl
    {
        private bool isActionDisabled = false;
        private EditComboBoxItem lastSelection = null;
        private const int StandardCommandsNumber = 2;
        public EditComboBox()
        {
            InitializeComponent();
            PART_Combo.SelectionChanged += (o,e) => {
                var selectedItem = PART_Combo.SelectedItem as EditComboBoxItem;
                var text = PART_Combo.Text;
                if (selectedItem == null)
                {
                    if( !String.IsNullOrWhiteSpace(text) && lastSelection != null) 
                    {
                        lastSelection.Title = text;
                    }
                    return;
                }

                    
                if (PART_Combo.SelectedIndex > StandardCommandsNumber - 1)
                    lastSelection = selectedItem;

                if (selectedItem.SelectAction != null && !isActionDisabled)
                    selectedItem.SelectAction();
            };
        }

        /// <summary>
        /// The <see cref="Items" /> dependency property's name.
        /// </summary>
        public const string ItemsPropertyName = "Items";

        /// <summary>
        /// Gets or sets the value of the <see cref="Items" />
        /// property. This is a dependency property.
        /// </summary>
        public ObservableCollection<EditComboBoxItem> Items
        {
            get
            {
                return (ObservableCollection<EditComboBoxItem>)GetValue(ItemsProperty);
            }
            set
            {
                SetValue(ItemsProperty, value);
            }
        }

        private void AddNewItem()
        {
            var source = PART_Combo.ItemsSource as ObservableCollection<EditComboBoxItem>;
            if( source == null )
                return;

            var newTitle = "New #";
            var newItemIndex = 1;
            while (source.Any(item => item.Title.Equals(String.Format("{0}{1}", newTitle, newItemIndex))))
                newItemIndex++;

            var newItem = new EditComboBoxItem() { Title = String.Format("{0}{1}",newTitle,newItemIndex) };
            source.Add(newItem);
            isActionDisabled = true;
            PART_Combo.SelectedItem = newItem;
            isActionDisabled = false;

        }

        private void DeleteCurrent()
        {
            if (Items == null)
                return;

            if( Items.Count > StandardCommandsNumber && lastSelection != null )
            {
                var source = PART_Combo.ItemsSource as ObservableCollection<EditComboBoxItem>;
                source.Remove(lastSelection);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Items" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            ItemsPropertyName,
            typeof(ObservableCollection<EditComboBoxItem>),
            typeof(EditComboBox),
            new UIPropertyMetadata(null, (o, e) => {
                var editCombo = o as EditComboBox;
                if (editCombo == null)
                    return;

                var newCollection = e.NewValue as ObservableCollection<EditComboBoxItem>;
                if( newCollection == null )
                {
                    editCombo.PART_Combo.ItemsSource = null;
                    return;
                }

                if( !newCollection.Any( item => item.Title == "<New...>" ))
                {
                    newCollection.Insert(0, new EditComboBoxItem()
                    {
                        Title = "<New...>",
                        SelectAction = () => editCombo.AddNewItem(),
                    });
                    newCollection.Insert(1, new EditComboBoxItem()
                    {
                        Title = "<Delete...>",
                        SelectAction = () => editCombo.DeleteCurrent(),
                    });
                }

                editCombo.PART_Combo.ItemsSource = newCollection;
                editCombo.isActionDisabled = true;
                editCombo.PART_Combo.SelectedIndex = 0;
                editCombo.isActionDisabled = false;
            }));

    }
}
