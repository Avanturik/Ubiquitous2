using System;
using System.Threading;
using System.Threading.Tasks;
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
        private bool isInvoked = false;
        protected override void OnAttached()
        {
            scrollViewer = Target;
            scrollViewer.ScrollChanged += scrollViewer_ScrollChanged;
        }

        void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

            if (e.ViewportHeightChange <= 0 && 
                e.ExtentHeightChange <= 0.0 && 
                scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight
                )
            {
                isInvoked = false;
                return;
            }
            ScrollDown();
        }

        protected override void Invoke(object parameter)
        {
            isInvoked = true;
            if (EnableAutoScroll && Duration == 0)
                InstantScrollToBottom();
            else
                ScrollDown();
        }

        protected void ScrollDown()
        {

            if (!EnableAutoScroll || !isInvoked )
                return;

            if (scrollViewer.ViewportHeight >= scrollViewer.ExtentHeight)
                return;
            
            if (storyBoard != null )
            {
                var progress = storyBoard.GetCurrentProgress();
                if( progress < 1 && progress > 0 )
                    return;

                storyBoard.Stop();
                storyBoard = null;
            }

            storyBoard = new Storyboard();            

            DoubleAnimation mainAnimation = new DoubleAnimation(scrollViewer.ExtentHeight - scrollViewer.ViewportHeight, TimeSpan.FromMilliseconds(Duration));
            storyBoard.Children.Add(mainAnimation);
            Storyboard.SetTarget(mainAnimation, AssociatedObject);
            Storyboard.SetTargetProperty(mainAnimation, new PropertyPath(ScrollViewerAttached.VerticalPositionProperty));
            storyBoard.Completed += (obj, e) => { 
                storyBoard = null;
            };
            storyBoard.Begin();
        }

        protected void InstantScrollToBottom()
        {
            var scrollViewer = this.Target;
            scrollViewer.ScrollToBottom();
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

        /// <summary>
        /// The <see cref="IsManual" /> dependency property's name.
        /// </summary>
        public const string IsManualPropertyName = "IsManual";

        /// <summary>
        /// Gets or sets the value of the <see cref="IsManual" />
        /// property. This is a dependency property.
        /// </summary>
        public bool IsManual
        {
            get
            {
                return (bool)GetValue(IsManualProperty);
            }
            set
            {
                SetValue(IsManualProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="IsManual" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsManualProperty = DependencyProperty.Register(
            IsManualPropertyName,
            typeof(bool),
            typeof(AutoScrollAction),
            new UIPropertyMetadata(false));
    }
}
