using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace UB.Interactivity
{
    public class ScrollViewerAttached
    {

        
        /// <summary>
        /// The VerticalPosition attached property's name.
        /// </summary>
        public const string VerticalPositionPropertyName = "VerticalPosition";

        /// <summary>
        /// Gets the value of the VerticalPosition attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the VerticalPosition property of the specified object.</returns>
        public static double GetVerticalPosition(DependencyObject obj)
        {
            return (double)obj.GetValue(VerticalPositionProperty);
        }

        /// <summary>
        /// Sets the value of the VerticalPosition attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the VerticalPosition value of the specified object.</param>
        public static void SetVerticalPosition(DependencyObject obj, double value)
        {
            obj.SetValue(VerticalPositionProperty, value);
        }

        /// <summary>
        /// Identifies the VerticalPosition attached property.
        /// </summary>
        public static readonly DependencyProperty VerticalPositionProperty = DependencyProperty.RegisterAttached(
            VerticalPositionPropertyName,
            typeof(double),
            typeof(ScrollViewer),
            new UIPropertyMetadata(0.0, (obj, e) =>
            {
                var scrollViewer = obj as ScrollViewer;

                if (scrollViewer == null)
                    return;

                double newOffset = (double)e.NewValue;

                scrollViewer.ScrollToVerticalOffset(newOffset);
            }));

    }
}
