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

namespace UB.Interactivity
{
    public class ElementToPng : Behavior<UIElement>
    {
        private UIElement visual;
        private Timer saveTimer;
        private VisualBrush visualBrush = new VisualBrush();
        private int sureSaveSteps = 2;
        private object lockSave = new object();
        public ElementToPng()
        {
            saveTimer = new Timer((obj) => {
                UI.Dispatch(() =>
                {
                    SaveVisualToPng();
                    if( SaveOnDemand )
                    {
                        sureSaveSteps--;
                        if (sureSaveSteps <= 0)
                        {
                            saveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                            sureSaveSteps = 2;
                        }
                    }
                });

            }, null, Timeout.Infinite, Timeout.Infinite);
        }
        protected override void OnDetaching()
        {
            saveTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        protected override void OnAttached()
        {
            visual = AssociatedObject as UIElement;
            visualBrush.Visual = visual;
        }

        public void SaveVisualToPng()
        {
            if (String.IsNullOrWhiteSpace(FileName))
                return;

            lock(lockSave)
            {
                if (visual == null || !visual.IsArrangeValid)
                    return;

                var width = visual.RenderSize.Width;
                var height = visual.RenderSize.Height;

                if( width == 0 || height == 0 )
                    return;

                var stopWatch = Stopwatch.StartNew();

                RenderTargetBitmap rtb = new RenderTargetBitmap((Int32)width, (Int32)height, 96, 96, PixelFormats.Pbgra32);
                
                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    drawingContext.DrawRectangle(visualBrush, null, new Rect(new Point(), new Size(width, height)));
                }

                rtb.Render(drawingVisual);
                
                PngBitmapEncoder png = new PngBitmapEncoder();

                var frame = BitmapFrame.Create(rtb);

                if (frame == null)
                    return;

                stopWatch.Stop();
                var ms = stopWatch.ElapsedMilliseconds;
                Log.WriteInfo("Frame captured in {0}ms", ms);
                png.Frames.Add(frame);

                if (String.IsNullOrWhiteSpace(FileName))
                    return;

                try
                {
                    FileStream file;

                    if( File.Exists(FileName) )
                        file = File.Open(FileName, FileMode.Truncate, FileAccess.ReadWrite, FileShare.Delete);
                    else
                        file = File.Open(FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Delete);

                    using (Stream stm = file )
                    {
                        png.Save(stm);
                    }
                }
                catch { }

            }
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
