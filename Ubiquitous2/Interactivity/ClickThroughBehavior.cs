using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Interop;
using UB.Model;

namespace UB.Interactivity
{
    public class ClickThroughBehavior :Behavior<Window>
    {
        const int WS_EX_TRANSPARENT = 0x00080020;
        const int NOT_WS_EX_TRANSPARENT = 0x7FF7FFDF;
        
        const int GWL_EXSTYLE = (-20);


        
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
            var extendedStyle = NativeMethods.GetWindowLong(hwnd, GWL_EXSTYLE);
            if (isTransparent)
            {
                Log.WriteInfo("Clickthrough enabled");
                NativeMethods.SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
                window.IsHitTestVisible = false;
            }
            else
            {
                Log.WriteInfo("Clickthrough disabled");
                NativeMethods.SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle & NOT_WS_EX_TRANSPARENT);
                window.IsHitTestVisible = true;
            }
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
