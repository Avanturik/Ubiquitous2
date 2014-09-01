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
            Storyboard.SetTargetProperty(mainAnimation, new PropertyPath(ScrollViewerAttached.VerticalPositionProperty));
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

    }
}
