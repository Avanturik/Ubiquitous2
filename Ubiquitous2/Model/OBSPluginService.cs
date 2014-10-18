using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using UB.Utils;

namespace UB.Model
{
    public class OBSPluginService : IOBSPluginService   
    {
        private ServiceHost serviceHost;
        private object lockSave = new object();
        private static ImageData imageData = new ImageData();
        private static Size currentSize = new Size();
        private static bool imageChanged = false;

        [DataMember]
        private static RenderTargetBitmap renderTarget;
        
        public ImageData GetImage()
        {
            lock (lockSave)
            {
                if ( imageChanged && RenderTarget != null)
                {
                    WriteableBitmap wb = new WriteableBitmap(RenderTarget);
                    currentSize.Width = wb.PixelWidth;
                    currentSize.Height = wb.PixelHeight;
                    imageData.Size = currentSize;
                    imageData.Pixels = wb.ConvertToByteArray();
                    return imageData;
                }
                else
                {
                    return null;
                }

            }
        }
        [DataMember]
        public RenderTargetBitmap RenderTarget {
            get {
                lock (lockSave)
                {
                    imageChanged = false;
                    return renderTarget;             
                }
            }
            set
            {
                lock (lockSave)
                {
                    renderTarget = value;
                    imageChanged = true;
                }
            }
        
        }
        public void Start()
        {
            serviceHost = new ServiceHost(
              typeof(OBSPluginService),
              new Uri[]{
                new Uri("net.pipe://localhost")
              });

            serviceHost.AddServiceEndpoint(typeof(IOBSPluginService),
              new NetNamedPipeBinding(),
              "ImageSource");

            serviceHost.Open();
        }

        public void Stop()
        {
            serviceHost.Close();
        }
    }


}
