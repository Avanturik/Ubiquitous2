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
    public class WindowPersistentLocationBehavior : Behavior<Window>
    {
        private Window window;
        private WindowSettings currentSettings;
        protected override void OnAttached()
        {            
            window = AssociatedObject;
            window.WindowStartupLocation = WindowStartupLocation.Manual;

            currentSettings = Ubiquitous.Default.Config.WindowConfigs.FirstOrDefault( config => config.WindowName.Equals(WindowName,StringComparison.InvariantCultureIgnoreCase));
            if (currentSettings == null)
            {
                currentSettings = window.GetPlacement(WindowName);
                Ubiquitous.Default.Config.WindowConfigs.Add(currentSettings);
            }
            else
            {
                window.Initialized += (obj,e) =>
                {
                    var win = obj as Window;
                    win.Top = currentSettings.Top;
                    win.Left = currentSettings.Left;
                    win.Width = currentSettings.Right - currentSettings.Left;
                    win.Height = currentSettings.Bottom - currentSettings.Top;                    
                    win.SetPlacement(currentSettings);
                };
            }

            window.SizeChanged += window_SizeChanged;
            window.LocationChanged += window_LocationChanged;
            window.StateChanged +=window_StateChanged;
        }

        void window_StateChanged(object sender, EventArgs e)
        {
            if (currentSettings != null)
                currentSettings.CopyStateFrom(window.GetPlacement());
        }

        void window_LocationChanged(object sender, EventArgs e)
        {
            if (currentSettings != null)
            {
                currentSettings.CopyLocationFrom(window.GetPlacement());
            }
        }

        void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (currentSettings != null)
            {
                currentSettings.CopyLocationFrom(window.GetPlacement());
            }            
        }

        protected override void OnDetaching()
        {
            
        }

        /// <summary>
        /// The <see cref="WindowName" /> dependency property's name.
        /// </summary>
        public const string WindowNamePropertyName = "WindowName";

        /// <summary>
        /// Gets or sets the value of the <see cref="WindowName" />
        /// property. This is a dependency property.
        /// </summary>
        public string WindowName
        {
            get
            {
                return (string)GetValue(WindowNameProperty);
            }
            set
            {
                SetValue(WindowNameProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="WindowName" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty WindowNameProperty = DependencyProperty.Register(
            WindowNamePropertyName,
            typeof(string),
            typeof(WindowPersistentLocationBehavior),
            new UIPropertyMetadata(null));
    }
}
