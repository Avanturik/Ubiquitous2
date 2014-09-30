using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace UB.Controls
{
    public class ComboBoxWOKeyNavigation : ComboBox
    {
        private HashSet<Key> ignoreKeys = new HashSet<Key> { Key.Up, Key.Down, Key.PageDown, Key.PageUp, Key.Left, Key.Right };
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (ignoreKeys.Contains(e.Key))
                e.Handled = true;
        }
    }

}
