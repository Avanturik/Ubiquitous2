using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace UB.Interactivity
{
    public class DragWindow :BehaviorBase
    {
        protected override void Attach()
        {
            AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseDown;
        }
        protected override void Cleanup()
        {
            AssociatedObject.PreviewMouseDown -= AssociatedObject_PreviewMouseDown;
        }
        void AssociatedObject_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Released)
                return;

            var window = AssociatedObject as Window;
            window.DragMove();


        }
    }
}
