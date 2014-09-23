using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Interop;

namespace UB.Interactivity
{
    public class ClickThroughBehavior :Behavior<Window>
    {
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int NOT_WS_EX_TRANSPARENT = 0x7FFFFFDF;
        const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        
        protected override void OnAttached()
        {
            Window window = AssociatedObject as Window;
            if (window == null)
                return;

            window.SourceInitialized += window_SourceInitialized;
        }

        void window_SourceInitialized(object sender, EventArgs e)
        {

            var window = sender as Window;
            if (window == null)
                return;

            SwitchTransparency(window, EnableMouseTransparency);
        }

        public static void SwitchTransparency(Window window, bool isTransparent)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            if( isTransparent )
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            else
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle & NOT_WS_EX_TRANSPARENT);
        }

        /// <summary>
        /// The <see cref="EnableMouseTransparency" /> dependency property's name.
        /// </summary>
        public const string EnableMouseTransparencyPropertyName = "EnableMouseTransparency";

        /// <summary>
        /// Gets or sets the value of the <see cref="EnableMouseTransparency" />
        /// property. This is a dependency property.
        /// </summary>
        public bool EnableMouseTransparency
        {
            get
            {
                return (bool)GetValue(EnableMouseTransparencyProperty);
            }
            set
            {
                SetValue(EnableMouseTransparencyProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="EnableMouseTransparency" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableMouseTransparencyProperty = DependencyProperty.Register(
            EnableMouseTransparencyPropertyName,
            typeof(bool),
            typeof(ClickThroughBehavior),
            new UIPropertyMetadata(true, (obj, args) => {
                var window = (obj as ClickThroughBehavior).AssociatedObject as Window;
                bool isTransparent = (bool)args.NewValue;
                if (window != null)
                    SwitchTransparency(window, isTransparent);
            }));
    }
}
