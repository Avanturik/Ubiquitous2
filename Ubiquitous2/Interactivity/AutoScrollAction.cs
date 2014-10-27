using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;
using UB.Utils;

namespace UB.Interactivity
{
    public class AutoScrollAction : TargetedTriggerAction<ScrollViewer>
    {
        private Storyboard storyBoard;
        private ScrollViewer scrollViewer;
        protected override void Invoke(object parameter)
        {
            if (EnableAutoScroll && Duration == 0)
                InstantScrollToBottom();
            else if (EnableAutoScroll)
                ScrollDown();
        }

        protected void ScrollDown()
        {
            //TODO to get really smooth scroll I need to calculate message height more precisely

            for (int i = 0; i < 2; i++)
            {
                Dispatcher.BeginInvoke(new Action(() => { InstantScrollToBottom(); }), DispatcherPriority.ContextIdle, null);
            }
            

            return; 

            storyBoard = new Storyboard();

            scrollViewer = this.Target;
            scrollViewer.InvalidateScrollInfo();

            if (Dispatcher != null)
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.Render, null);

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
            storyBoard.Completed += storyBoard_Completed;
            storyBoard.Begin(scrollViewer);
        }

        void storyBoard_Completed(object sender, EventArgs e)
        {
            using (var timer = new Timer((obj) =>
            {
                UI.Dispatch(() => InstantScrollToBottom());
            }, this, 100, Timeout.Infinite)) { };
        }

        protected void InstantScrollToBottom()
        {
            var scrollViewer = this.Target;

            scrollViewer.ScrollToBottom();
            //if (Dispatcher != null)
            //    Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);

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
        /// The <see cref="EnableAutoScroll" /> dependency property's name.
        /// </summary>
        public const string EnableAutoScrollPropertyName = "EnableAutoScroll";

        /// <summary>
        /// Gets or sets the value of the <see cref="EnableAutoScroll" />
        /// property. This is a dependency property.
        /// </summary>
        public bool EnableAutoScroll
        {
            get
            {
                return (bool)GetValue(EnableAutoScrollProperty);
            }
            set
            {
                SetValue(EnableAutoScrollProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="EnableAutoScroll" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableAutoScrollProperty = DependencyProperty.Register(
            EnableAutoScrollPropertyName,
            typeof(bool),
            typeof(AutoScrollAction),
            new UIPropertyMetadata(true));

        /// <summary>
        /// The <see cref="WaitSizeChange" /> dependency property's name.
        /// </summary>
        public const string WaitSizeChangePropertyName = "WaitSizeChange";

        /// <summary>
        /// Gets or sets the value of the <see cref="WaitSizeChange" />
        /// property. This is a dependency property.
        /// </summary>
        public bool WaitSizeChange
        {
            get
            {
                return (bool)GetValue(WaitSizeChangeProperty);
            }
            set
            {
                SetValue(WaitSizeChangeProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="WaitSizeChange" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty WaitSizeChangeProperty = DependencyProperty.Register(
            WaitSizeChangePropertyName,
            typeof(bool),
            typeof(AutoScrollAction),
            new UIPropertyMetadata(false));
    }
}
