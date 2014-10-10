using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UB.Controls
{
	/// <summary>
	/// Interaction logic for RollingFadingText.xaml
	/// </summary>
	public partial class RollingFadingText : UserControl
	{
		public RollingFadingText()
		{
			this.InitializeComponent();
		}

        /// <summary>
        /// The <see cref="Title" /> dependency property's name.
        /// </summary>
        public const string TitlePropertyName = "Title";

        /// <summary>
        /// Gets or sets the value of the <see cref="Title" />
        /// property. This is a dependency property.
        /// </summary>
        public string Title
        {
            get
            {
                return (string)GetValue(TitleProperty);
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Title" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            TitlePropertyName,
            typeof(string),
            typeof(RollingFadingText),
            new UIPropertyMetadata("Thank you for following me!"));

        /// <summary>
        /// The <see cref="Message" /> dependency property's name.
        /// </summary>
        public const string MessagePropertyName = "Message";

        /// <summary>
        /// Gets or sets the value of the <see cref="Message" />
        /// property. This is a dependency property.
        /// </summary>
        public string Message
        {
            get
            {
                return (string)GetValue(MessageProperty);
            }
            set
            {
                SetValue(MessageProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Message" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            MessagePropertyName,
            typeof(string),
            typeof(RollingFadingText),
            new UIPropertyMetadata("Username is coming here"));


	}
}