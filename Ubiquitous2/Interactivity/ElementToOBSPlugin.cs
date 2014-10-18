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


namespace UB.Interactivity
{
    public class ElementToOBSPlugin : Behavior<UIElement>
    {
        private UIElement visual;
        private Timer saveTimer;      
        private object lockSave = new object();
        private bool isChanged = false;
        private OBSPluginService obsPluginService = new OBSPluginService();
        
        public ElementToOBSPlugin()
        {
            saveTimer = new Timer((obj) =>
            {
                UI.Dispatch(() =>
                {
                    CaptureImage();
                });

            }, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected override void OnDetaching()
        {
            saveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            obsPluginService.Stop();
        }
        protected override void OnAttached()
        {
            visual = AssociatedObject as UIElement;
            visual.LayoutUpdated += visual_LayoutUpdated;
            try
            {
                obsPluginService.Start();
            }
            catch
            {

            }
        }

        void visual_LayoutUpdated(object sender, EventArgs e)
        {
            isChanged = true;
        }

        public void CaptureImage()
        {
            if (!isChanged)
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

                RenderTargetBitmap renderTarget = new RenderTargetBitmap((Int32)width, (Int32)height, 96, 96, PixelFormats.Pbgra32);

                renderTarget.Render(visual);

                obsPluginService.RenderTarget = renderTarget;

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
