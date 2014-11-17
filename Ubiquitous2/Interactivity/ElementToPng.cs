using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using UB.Model;
using UB.Utils;
using System.Windows.Controls;

namespace UB.Interactivity
{
    public class ElementToPng : BehaviorBase
    {
        private UIElement visual;
        private Timer saveTimer;
        private VisualBrush visualBrush = new VisualBrush();     
        private object lockSave = new object();
        private static bool imageChanged = false;

        public ElementToPng()
        {
            saveTimer = new Timer((obj) => {
                SaveVisualToPng();
            }, null, Timeout.Infinite, Timeout.Infinite);
        }
        protected override void Cleanup()
        {
            saveTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        protected override void Attach()
        {
            visual = AssociatedObject as UIElement;
            visual.LayoutUpdated += visual_LayoutUpdated;
            visualBrush.Visual = visual;            
        }

        void visual_LayoutUpdated(object sender, EventArgs e)
        {
            imageChanged = true;
        }

        private void SaveVisualToPng()
        {

            if (!imageChanged )
                return;

            imageChanged = false;

            dynamic saveResult = null;

            lock(lockSave)
            {
                if (visual == null || !visual.IsArrangeValid)
                    return;


                saveResult = UI.DispatchFunc( new Func<dynamic>(() =>
                {
                    if (String.IsNullOrWhiteSpace(FileName))
                        return null;

                    var width = visual.RenderSize.Width + SystemParameters.VerticalScrollBarWidth;
                    var height = visual.RenderSize.Height;

                    if (width == 0 || height == 0)
                        return null;

                    RenderTargetBitmap rtb = new RenderTargetBitmap((Int32)width, (Int32)height, DeviceHelper.PixelsPerInch(Orientation.Horizontal), DeviceHelper.PixelsPerInch(Orientation.Vertical), PixelFormats.Pbgra32);
                    rtb.Render(visual);
                    var frame = BitmapFrame.Create(rtb);

                    if (frame == null)
                        return null;

                    PngBitmapEncoder png = new PngBitmapEncoder();
                    png.Frames.Add(frame);

                    byte[] pngBytes = null;
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        png.Save(memoryStream);
                        memoryStream.Flush();
                        pngBytes = memoryStream.ToArray();
                        memoryStream.Close();
                    }
                    return new { PngBytes = pngBytes, FileName = FileName };
                }));


            }
            try
            {
                if (saveResult.PngBytes != null && saveResult.PngBytes.Length > 0)
                    File.WriteAllBytes(saveResult.FileName, saveResult.PngBytes);
            }
            catch { }
        }

        /// <summary>
        /// The <see cref="DelayBefore" /> dependency property's name.
        /// </summary>
        public const string DelayBeforePropertyName = "DelayBefore";

        /// <summary>
        /// Gets or sets the value of the <see cref="DelayBefore" />
        /// property. This is a dependency property.
        /// </summary>
        public int DelayBefore
        {
            get
            {
                return (int)GetValue(DelayBeforeProperty);
            }
            set
            {
                SetValue(DelayBeforeProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="DelayBefore" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty DelayBeforeProperty = DependencyProperty.Register(
            DelayBeforePropertyName,
            typeof(int),
            typeof(ElementToPng),
            new UIPropertyMetadata(100));
        /// <summary>
        /// The <see cref="SaveFlag" /> dependency property's name.
        /// </summary>
        public const string SaveFlagPropertyName = "SaveFlag";

        /// <summary>
        /// Gets or sets the value of the <see cref="SaveFlag" />
        /// property. This is a dependency property.
        /// </summary>
        public bool SaveFlag
        {
            get
            {
                return (bool)GetValue(SaveFlagProperty);
            }
            set
            {
                SetValue(SaveFlagProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="SaveFlag" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SaveFlagProperty = DependencyProperty.Register(
            SaveFlagPropertyName,
            typeof(bool),
            typeof(ElementToPng),
            new UIPropertyMetadata(false, (o, e) => {
                var isNeedSaving = (bool)e.NewValue;
                var behavior = o as ElementToPng;
                if( isNeedSaving )
                {
                    behavior.saveTimer.Change(behavior.DelayBefore, 1000);
                }
            }));

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
            typeof(ElementToPng),
            new UIPropertyMetadata(1500, (o, e) => {
                var interval = (int)e.NewValue;
                var element = o as ElementToPng;
                if (element.SaveOnDemand)
                    return;

                if (interval > 0)
                    element.saveTimer.Change(interval, interval);
                else
                    element.saveTimer.Change(Timeout.Infinite, Timeout.Infinite);

            }));

        /// <summary>
        /// The <see cref="SaveOnDemand" /> dependency property's name.
        /// </summary>
        public const string SaveOnDemandPropertyName = "SaveOnDemand";

        /// <summary>
        /// Gets or sets the value of the <see cref="SaveOnDemand" />
        /// property. This is a dependency property.
        /// </summary>
        public bool SaveOnDemand
        {
            get
            {
                return (bool)GetValue(SaveOnDemandProperty);
            }
            set
            {
                SetValue(SaveOnDemandProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="SaveOnDemand" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty SaveOnDemandProperty = DependencyProperty.Register(
            SaveOnDemandPropertyName,
            typeof(bool),
            typeof(ElementToPng),
            new UIPropertyMetadata(true));

        /// <summary>
        /// The <see cref="FileName" /> dependency property's name.
        /// </summary>
        public const string FileNamePropertyName = "FileName";

        /// <summary>
        /// Gets or sets the value of the <see cref="FileName" />
        /// property. This is a dependency property.
        /// </summary>
        public string FileName
        {
            get
            {
                return (string)GetValue(FileNameProperty);
            }
            set
            {
                SetValue(FileNameProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="FileName" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty FileNameProperty = DependencyProperty.Register(
            FileNamePropertyName,
            typeof(string),
            typeof(ElementToPng),
            new UIPropertyMetadata(null));
    }
}
