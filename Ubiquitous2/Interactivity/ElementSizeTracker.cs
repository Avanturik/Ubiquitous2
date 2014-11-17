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
    public class ElementSizeTracker : BehaviorBase
    {
        private FrameworkElement element;

        protected override void Attach()
        {
            element = AssociatedObject;
            element.SizeChanged += element_SizeChanged;            
        }
        protected override void Cleanup()
        {
            element.SizeChanged -= element_SizeChanged;
        }

        void element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (element == null)
                return;

            Height = element.ActualHeight;
            Width = element.ActualWidth;
        }

        /// <summary>
        /// The <see cref="Height" /> dependency property's name.
        /// </summary>
        public const string HeightPropertyName = "Height";

        /// <summary>
        /// Gets or sets the value of the <see cref="Height" />
        /// property. This is a dependency property.
        /// </summary>
        public double Height
        {
            get
            {
                return (double)GetValue(HeightProperty);
            }
            set
            {
                SetValue(HeightProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Height" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
            HeightPropertyName,
            typeof(double),
            typeof(ElementSizeTracker),
            new UIPropertyMetadata(-1.0d));

        /// <summary>
        /// The <see cref="Width" /> dependency property's name.
        /// </summary>
        public const string WidthPropertyName = "Width";

        /// <summary>
        /// Gets or sets the value of the <see cref="Width" />
        /// property. This is a dependency property.
        /// </summary>
        public double Width
        {
            get
            {
                return (double)GetValue(WidthProperty);
            }
            set
            {
                SetValue(WidthProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Width" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            WidthPropertyName,
            typeof(double),
            typeof(ElementSizeTracker),
            new UIPropertyMetadata(-1.0d));
    }
}
