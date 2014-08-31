using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace UB.Interactivity
{
    public class AutoScrollAction : TargetedTriggerAction<ScrollViewer>
    {
        private Storyboard storyBoard;
        private ScrollViewer scrollViewer;

        protected override void Invoke(object parameter)
        {
            ScrollDown();
        }

        protected void ScrollDown()
        {
            Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.Render, null);
            storyBoard = new Storyboard();


            scrollViewer = this.Target;

            DoubleAnimation mainAnimation = new DoubleAnimation()
            {
                From = scrollViewer.VerticalOffset,
                To = scrollViewer.ScrollableHeight,
                DecelerationRatio = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(Duration)),                
            };

            storyBoard.Children.Add(mainAnimation);
            Storyboard.SetTarget(mainAnimation, AssociatedObject);            
            Storyboard.SetTargetProperty(mainAnimation, new PropertyPath(VerticalPositionProperty));
            storyBoard.Begin(scrollViewer);
        }

        /// <summary>
        /// The <see cref="Duration" /> dependency property's name.
        /// </summary>
        public const string DurationPropertyName = "Duration";

        /// <summary>
        /// Gets or sets the value of the <see cref="Duration" />
        /// property. This is a dependency property.
        /// </summary>
        public double Duration
        {
            get
            {
                return (double)GetValue(DurationProperty);
            }
            set
            {
                SetValue(DurationProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Duration" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
            DurationPropertyName,
            typeof(double),
            typeof(AutoScrollAction),
            new UIPropertyMetadata(0.0));



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
