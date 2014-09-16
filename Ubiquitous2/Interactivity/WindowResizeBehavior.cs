using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Interop;
using System.Windows.Media;
using UB.Utils;
namespace UB.Interactivity
{
    public class WindowResizeBehavior : Behavior<FrameworkElement>
    {
        private WindowResizer windowResizer;
        private Window window;
        protected override void OnAttached()
        {
            window = this.With(x => (FrameworkElement)AssociatedObject)
                .With(x => Window.GetWindow(x));
            
            if (window == null)
                return;

            if (!window.IsLoaded)
                window.SourceInitialized += window_SourceInitialized;
            else
                Initialize(window);
        }


        void window_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
              Initialize(sender);
        }
        protected override void OnDetaching()
        {
            window.SourceInitialized -= window_SourceInitialized;
        }
        void Initialize(object obj)
        {
            var hwndSource = (HwndSource)PresentationSource.FromVisual((Visual)obj);

            if (TopLeftGrip == null || TopGrip == null || TopRightGrip == null || RightGrip == null || BottomRightGrip == null || BottomGrip == null || BottomLeftGrip == null || LeftGrip == null)
                return;

            windowResizer = new WindowResizer(window,
                hwndSource,
               new WindowBorder(BorderPosition.TopLeft, TopLeftGrip),
               new WindowBorder(BorderPosition.Top, TopGrip),
               new WindowBorder(BorderPosition.TopRight, TopRightGrip),
               new WindowBorder(BorderPosition.Right, RightGrip),
               new WindowBorder(BorderPosition.BottomRight, BottomRightGrip),
               new WindowBorder(BorderPosition.Bottom, BottomGrip),
               new WindowBorder(BorderPosition.BottomLeft, BottomLeftGrip),
               new WindowBorder(BorderPosition.Left, LeftGrip));

        }
        void window_SourceInitialized(object sender, EventArgs e)
        {
            Initialize(sender);
        }

        /// <summary>
        /// The <see cref="TopLeftGrip" /> dependency property's name.
        /// </summary>
        public const string TopLeftGripPropertyName = "TopLeftGrip";

        /// <summary>
        /// Gets or sets the value of the <see cref="TopLeftGrip" />
        /// property. This is a dependency property.
        /// </summary>
        public FrameworkElement TopLeftGrip
        {
            get
            {
                return (FrameworkElement)GetValue(TopLeftGripProperty);
            }
            set
            {
                SetValue(TopLeftGripProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="TopLeftGrip" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty TopLeftGripProperty = DependencyProperty.Register(
            TopLeftGripPropertyName,
            typeof(FrameworkElement),
            typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null));

        /// <summary>
        /// The <see cref="TopGrip" /> dependency property's name.
        /// </summary>
        public const string TopGripPropertyName = "TopGrip";

        /// <summary>
        /// Gets or sets the value of the <see cref="TopGrip" />
        /// property. This is a dependency property.
        /// </summary>
        public FrameworkElement TopGrip
        {
            get
            {
                return (FrameworkElement)GetValue(TopGripProperty);
            }
            set
            {
                SetValue(TopGripProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="TopGrip" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty TopGripProperty = DependencyProperty.Register(
            TopGripPropertyName,
            typeof(FrameworkElement),
            typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null));

        /// <summary>
        /// The <see cref="TopRightGrip" /> dependency property's name.
        /// </summary>
        public const string TopRightGripPropertyName = "TopRightGrip";

        /// <summary>
        /// Gets or sets the value of the <see cref="TopRightGrip" />
        /// property. This is a dependency property.
        /// </summary>
        public FrameworkElement TopRightGrip
        {
            get
            {
                return (FrameworkElement)GetValue(TopRightGripProperty);
            }
            set
            {
                SetValue(TopRightGripProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="TopRightGrip" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty TopRightGripProperty = DependencyProperty.Register(
            TopRightGripPropertyName,
            typeof(FrameworkElement),
            typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null));


        /// <summary>
        /// The <see cref="RightGrip" /> dependency property's name.
        /// </summary>
        public const string RightGripPropertyName = "RightGrip";

        /// <summary>
        /// Gets or sets the value of the <see cref="RightGrip" />
        /// property. This is a dependency property.
        /// </summary>
        public FrameworkElement RightGrip
        {
            get
            {
                return (FrameworkElement)GetValue(RightGripProperty);
            }
            set
            {
                SetValue(RightGripProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="RightGrip" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty RightGripProperty = DependencyProperty.Register(
            RightGripPropertyName,
            typeof(FrameworkElement),
            typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null));

        /// <summary>
        /// The <see cref="BottomRightGrip" /> dependency property's name.
        /// </summary>
        public const string BottomRightGripPropertyName = "BottomRightGrip";

        /// <summary>
        /// Gets or sets the value of the <see cref="BottomRightGrip" />
        /// property. This is a dependency property.
        /// </summary>
        public FrameworkElement BottomRightGrip
        {
            get
            {
                return (FrameworkElement)GetValue(BottomRightGripProperty);
            }
            set
            {
                SetValue(BottomRightGripProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="BottomRightGrip" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty BottomRightGripProperty = DependencyProperty.Register(
            BottomRightGripPropertyName,
            typeof(FrameworkElement),
            typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null));

        /// <summary>
        /// The <see cref="BottomGrip" /> dependency property's name.
        /// </summary>
        public const string BottomGripPropertyName = "BottomGrip";

        /// <summary>
        /// Gets or sets the value of the <see cref="BottomGrip" />
        /// property. This is a dependency property.
        /// </summary>
        public FrameworkElement BottomGrip
        {
            get
            {
                return (FrameworkElement)GetValue(BottomGripProperty);
            }
            set
            {
                SetValue(BottomGripProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="BottomGrip" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty BottomGripProperty = DependencyProperty.Register(
            BottomGripPropertyName,
            typeof(FrameworkElement),
            typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null));

        /// <summary>
        /// The <see cref="BottomLeftGrip" /> dependency property's name.
        /// </summary>
        public const string BottomLeftGripPropertyName = "BottomLeftGrip";

        /// <summary>
        /// Gets or sets the value of the <see cref="BottomLeftGrip" />
        /// property. This is a dependency property.
        /// </summary>
        public FrameworkElement BottomLeftGrip
        {
            get
            {
                return (FrameworkElement)GetValue(BottomLeftGripProperty);
            }
            set
            {
                SetValue(BottomLeftGripProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="BottomLeftGrip" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty BottomLeftGripProperty = DependencyProperty.Register(
            BottomLeftGripPropertyName,
            typeof(FrameworkElement),
            typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null));

        /// <summary>
        /// The <see cref="LeftGrip" /> dependency property's name.
        /// </summary>
        public const string LeftGripPropertyName = "LeftGrip";

        /// <summary>
        /// Gets or sets the value of the <see cref="LeftGrip" />
        /// property. This is a dependency property.
        /// </summary>
        public FrameworkElement LeftGrip
        {
            get
            {
                return (FrameworkElement)GetValue(LeftGripProperty);
            }
            set
            {
                SetValue(LeftGripProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="LeftGrip" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty LeftGripProperty = DependencyProperty.Register(
            LeftGripPropertyName,
            typeof(FrameworkElement),
            typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null));



    }

}
