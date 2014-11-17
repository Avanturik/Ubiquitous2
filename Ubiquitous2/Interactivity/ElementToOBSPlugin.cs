using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using UB.Model;
using UB.Utils;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Windows.Media.Animation;
using System.Windows.Controls;


namespace UB.Interactivity
{
    public class ElementToOBSPlugin : BehaviorBase
    {
        private UIElement visual;
        private Timer saveTimer;
        private object lockSave = new object();
        private bool isChanged = false;
        private double horizontalDpi;
        private double verticalDpi;
        private OBSPluginService obsPluginService = new OBSPluginService();
        
        public ElementToOBSPlugin()
        {
            saveTimer = new Timer((obj) =>
            {
               CaptureImage();

            }, null, Timeout.Infinite, Timeout.Infinite);
        }
        protected override void Attach()
        {
            visual = AssociatedObject as UIElement;
            visual.LayoutUpdated += visual_LayoutUpdated;
            horizontalDpi = (Double)DeviceHelper.PixelsPerInch(Orientation.Horizontal);
            verticalDpi = (Double)DeviceHelper.PixelsPerInch(Orientation.Vertical);

            try
            {
                obsPluginService.Start();
            }
            catch
            {

            }            
        }
        protected override void Cleanup()
        {
            saveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            obsPluginService.Stop();            
        }

        void visual_LayoutUpdated(object sender, EventArgs e)
        {
            isChanged = true;
        }

        public void CaptureImage()
        {
            if (!isChanged || !obsPluginService.IsConnected )
                return;


            lock (lockSave)
            {
                isChanged = false;

                if (visual == null || !visual.IsArrangeValid)
                    return;

                var width = visual.RenderSize.Width;
                var height = visual.RenderSize.Height;

                if (width == 0 || height == 0)
                    return;

                UI.Dispatch( () => {
                    RenderTargetBitmap renderTarget = new RenderTargetBitmap((Int32)Math.Ceiling(width), 
                        (Int32)Math.Ceiling(height),
                        horizontalDpi, 
                        verticalDpi, 
                        PixelFormats.Pbgra32);
                    
                    renderTarget.Render(visual);
                    obsPluginService.RenderTarget = renderTarget;
                });

            }
        }

        /// <summary>
        /// The <see cref="SaveInterval" /> dependency property's name.
        /// </summary>
        public const string SaveIntervalPropertyName = "SaveInterval";

        /// <summary>
        /// Gets or sets the value of the <see cref="SaveInterval" />
        /// property. This is a dependency property.
        /// </summary>
        public int SaveInterval
        {
            get
            {
                return (int)GetValue(SaveIntervalProperty);
            }
            set
            {
                SetValue(SaveIntervalProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="SaveInterval" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SaveIntervalProperty = DependencyProperty.Register(
            SaveIntervalPropertyName,
            typeof(int),
            typeof(ElementToOBSPlugin),
            new UIPropertyMetadata(1500, (o, e) =>
            {
                var interval = (int)e.NewValue;
                var element = o as ElementToOBSPlugin;

                if (interval > 0)
                    element.saveTimer.Change(0, interval);
                else
                    element.saveTimer.Change(Timeout.Infinite, Timeout.Infinite);

               

            }));
    }
}
