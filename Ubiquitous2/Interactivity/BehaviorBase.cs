using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace UB.Interactivity
{
    public class BehaviorBase: Behavior<FrameworkElement>
    {
        private bool IsCleanedUp { get; set; }
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
            Attach();
        }
        protected override void OnDetaching()
        {
            if (!IsCleanedUp)
            {
                RemoveEvents();
                Cleanup();
            }
            base.OnDetaching();
        }

        void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            if(!IsCleanedUp)
            {
                RemoveEvents();
                Cleanup();
            }
        }
        private void RemoveEvents()
        {
            if (!IsCleanedUp)
            {
                IsCleanedUp = true;
                AssociatedObject.Loaded -= AssociatedObject_Loaded;
                AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            }
        }
        void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeEvents();
        }
        protected virtual void Attach()
        {

        }
        protected virtual void Cleanup()
        {

        }
        protected virtual void InitializeEvents()
        {

        }
    }
}
