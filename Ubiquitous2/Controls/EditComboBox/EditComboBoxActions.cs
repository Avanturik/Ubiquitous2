using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UB.Controls
{
    public class EditComboBoxActions
    {
        private EditComboBox editCombo;
        private ComboBox combo;
        private ObservableCollection<EditComboBoxItem> items;
        private EditComboBoxItem previousSelection;
        private const string ActionNewName = "<New...>";
        private const string ActionDelName = "<Delete...>";
        public EditComboBoxActions(EditComboBox comboBox)
        {
            editCombo = comboBox;
            items = editCombo.ItemsSource;
            editCombo.PART_Combo.SelectionChanged += PART_Combo_SelectionChanged;
            editCombo.PART_Combo.TextInput += PART_Combo_TextInput;
            FillDefaultActions();
        }

        void PART_Combo_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var text = e.Text;
            //TODO rename item
        }

        void PART_Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            //TODO call actions
            previousSelection = editCombo.PART_Combo.SelectedItem as EditComboBoxItem;
        }

        private void FillDefaultActions()
        {
            if (items == null)
                return;

            if( !items.Any( item => item.Title.Equals(ActionNewName, StringComparison.InvariantCultureIgnoreCase)))
            {
                items.Insert(0, new EditComboBoxItem() { Title = ActionNewName, SelectAction = AddAction });
                items.Insert(1, new EditComboBoxItem() { Title = ActionDelName, SelectAction = DelAction });
            }
        }

        private void AddAction()
        {
            items.Add(new EditComboBoxItem() { Title = "New #" + items.Count(item => item.Title.StartsWith("New #"))+1, SelectAction = SelAction });
        }

        private void DelAction()
        {
            
        }

        private void SelAction()
        {

        }

    }
}
