using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using GalaSoft.MvvmLight.Threading;
using UB.Model;
using UB.Properties;
using UB.Utils;

namespace UB.Interactivity
{
    public class UnclosableWindow : Behavior<Window>
    {
        private Window window;
        protected override void OnAttached()
        {            
            window = AssociatedObject;
            window.Closing += window_Closing;
        }

        void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            window.Visibility = Visibility.Hidden;
            e.Cancel = true;

        }
        protected override void OnDetaching()
        {
            window.Closing -= window_Closing;
        }
    }
}
