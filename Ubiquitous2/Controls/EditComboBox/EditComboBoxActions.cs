using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using UB.Utils;

namespace UB.Controls
{
    public class EditComboBoxActions
    {
        private ObservableCollection<EditComboBoxItem> _items;
        private const string newItemPrefix = "New #";
        public EditComboBoxActions(ObservableCollection<EditComboBoxItem> items)
        {
            _items = items;
            FillDefaultActions();
        }

        private void FillDefaultActions()
        {
            if (_items == null)
                return;

            var defaultCommands = new Dictionary<string, Action>() { 
                {"<New...>", Add},
                {"<Delete...>", Del},
            };

            foreach( var item in _items )
            {
                if( item.SelectAction == null )
                    item.SelectAction = () => Select(item);
            }

            for (int i = 0; i < defaultCommands.Count; i++)
                _items.Insert(i, new EditComboBoxItem() { 
                    Title = defaultCommands.Keys.ElementAt(i), 
                    SelectAction = defaultCommands.Values.ElementAt(i), 
                    IsUndeletable = true, 
                    IsUnselectable = true 
                });

        }

        public Action<EditComboBoxItem> AddAction { get; set; }
        public Action<EditComboBoxItem> DelAction { get; set; }
        public Action<EditComboBoxItem> SelectAction { get; set; }

        private void Add()
        {
            var current = _items.FirstOrDefault(item => !item.IsUnselectable && item.IsCurrent);
            if (current != null)
                current.IsCurrent = false;

            var newItem = new EditComboBoxItem()
            {
                Title = String.Format("{0}{1}",newItemPrefix, _items.Count(item => item.Title.StartsWith(newItemPrefix)) + 1),
                IsUnselectable = false,
                IsCurrent = true,
            };
            newItem.SelectAction = () => Select(newItem);

            _items.Add(newItem);

            if (AddAction != null)
                AddAction(newItem);
        }

        private void Del()
        {
            _items.RemoveAll(item => !item.IsUndeletable && item.IsCurrent == true);
            var firstNonCommand = _items.FirstOrDefault(item => !item.IsUnselectable);
            if (firstNonCommand != null)
                firstNonCommand.IsCurrent = true;

            if (DelAction != null)
                DelAction(firstNonCommand);
        }

        private void Select(EditComboBoxItem item)
        {
            var currentItem = _items.FirstOrDefault(i => i.IsCurrent);
            if( currentItem != null )
                currentItem.IsCurrent = false;

            item.IsCurrent = true;

            if (SelectAction != null)
                SelectAction(item);
        }

    }
}
