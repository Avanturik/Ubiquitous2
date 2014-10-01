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
using GalaSoft.MvvmLight.Command;
using UB.Model;
using UB.Utils;

namespace UB.Controls
{
    /// <summary>
    /// Interaction logic for EditComboBox.xaml
    /// </summary>
    public partial class EditComboBox : UserControl
    {
        private EditComboBoxActions comboActions;
        
        public EditComboBox()
        {
            InitializeComponent();
            PART_Combo.SelectionChanged += PART_Combo_SelectionChanged;
        }

        void PART_Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var action = this.With(x => sender as ComboBox)
                .With(x => x.SelectedItem as EditComboBoxItem)
                .With(x => x.SelectAction);

            if (action != null)
                action();
        }

        /// <summary>
        /// The <see cref="CommandRename" /> dependency property's name.
        /// </summary>
        public const string CommandRenamePropertyName = "CommandRename";

        /// <summary>
        /// Gets or sets the value of the <see cref="CommandRename" />
        /// property. This is a dependency property.
        /// </summary>
        public RelayCommand CommandRename
        {
            get
            {
                return (RelayCommand)GetValue(CommandRenameProperty);
            }
            set
            {
                SetValue(CommandRenameProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="CommandRename" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandRenameProperty = DependencyProperty.Register(
            CommandRenamePropertyName,
            typeof(RelayCommand),
            typeof(EditComboBox),
            new UIPropertyMetadata(null));

        /// <summary>
        /// The <see cref="CommandAdd" /> dependency property's name.
        /// </summary>
        public const string CommandAddPropertyName = "CommandAdd";

        /// <summary>
        /// Gets or sets the value of the <see cref="CommandAdd" />
        /// property. This is a dependency property.
        /// </summary>
        public RelayCommand CommandAdd
        {
            get
            {
                return (RelayCommand)GetValue(CommandAddProperty);
            }
            set
            {
                SetValue(CommandAddProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="CommandAdd" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandAddProperty = DependencyProperty.Register(
            CommandAddPropertyName,
            typeof(RelayCommand),
            typeof(EditComboBox),
            new UIPropertyMetadata(null));

        /// <summary>
        /// The <see cref="CommandDelete" /> dependency property's name.
        /// </summary>
        public const string CommandDeletePropertyName = "CommandDelete";

        /// <summary>
        /// Gets or sets the value of the <see cref="CommandDelete" />
        /// property. This is a dependency property.
        /// </summary>
        public RelayCommand<EditComboBoxItem> CommandDelete
        {
            get
            {
                return (RelayCommand<EditComboBoxItem>)GetValue(CommandDeleteProperty);
            }
            set
            {
                SetValue(CommandDeleteProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="CommandDelete" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandDeleteProperty = DependencyProperty.Register(
            CommandDeletePropertyName,
            typeof(RelayCommand<EditComboBoxItem>),
            typeof(EditComboBox),
            new UIPropertyMetadata(null));

        /// <summary>
        /// The <see cref="CommandSelect" /> dependency property's name.
        /// </summary>
        public const string CommandSelectPropertyName = "CommandSelect";

        /// <summary>
        /// Gets or sets the value of the <see cref="CommandSelect" />
        /// property. This is a dependency property.
        /// </summary>
        public RelayCommand CommandSelect
        {
            get
            {
                return (RelayCommand)GetValue(CommandSelectProperty);
            }
            set
            {
                SetValue(CommandSelectProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="CommandSelect" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandSelectProperty = DependencyProperty.Register(
            CommandSelectPropertyName,
            typeof(RelayCommand),
            typeof(EditComboBox),
            new UIPropertyMetadata(null));

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

                if (editCombo == null)
                    return;

                var newValue = e.NewValue as ObservableCollection<EditComboBoxItem>;                               

                editCombo.comboActions = new EditComboBoxActions(newValue);

                editCombo.comboActions.AddAction = (item) =>
                {
                    editCombo.PART_Combo.SelectedItem = item;

                    if (editCombo.CommandAdd != null)
                        editCombo.CommandAdd.Execute(null);
                };
                editCombo.comboActions.DelAction = (item) =>
                {
                    var newCurrent = editCombo.ItemsSource.FirstOrDefault( comboItem => !comboItem.IsUnselectable );
                    if( newCurrent != null)
                        newCurrent.IsCurrent = true;
                    
                    editCombo.PART_Combo.SelectedItem = newCurrent;

                    if (editCombo.CommandDelete != null && item != null)
                        editCombo.CommandDelete.Execute(item);
                };
                editCombo.comboActions.SelectAction = (item) =>
                {
                    if (editCombo.CommandSelect != null)
                        editCombo.CommandSelect.Execute(null);
                };
                editCombo.comboActions.RenameAction = (item) =>
                    {
                        if (editCombo.CommandRename != null)
                            editCombo.CommandRename.Execute(null);
                    };
            }));

    }
}
