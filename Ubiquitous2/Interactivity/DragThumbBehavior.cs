using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace UB.Interactivity
{
    public class DragThumb :Behavior<UIElement>
    {
        private Window window;
        protected override void OnAttached()
        {
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            window = Window.GetWindow(AssociatedObject);
        }

        void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;
            if (e.ClickCount > 1)
                SwitchWindowSize();
            else if (e.ClickCount == 1)
                StartDrag();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;        
        }

        private void SwitchWindowSize()
        {
            if (window == null)
                return;
            window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        }
        private void StartDrag()
        {
            if (window == null)
                return;

            window.DragMove();
        }
    }
}
