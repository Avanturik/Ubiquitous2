using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLROBS;
using System.ServiceModel;
using UB.Model;
using System.Diagnostics;
using System.Drawing;

namespace Ubiquitous2Plugin
{
    internal class Ubiquitous2Source : AbstractImageSource
    {
        private ChannelFactory<IOBSPluginService> pipeFactory;
        private Texture texture;
        private Size currentSize = new Size(100, 100);
        private IOBSPluginService pipeProxy;
        private ImageData imageData;
        private object imageLock = new object();
        public Ubiquitous2Source(XElement configElement)
        {
            UpdateSettings();

            pipeFactory =
                new ChannelFactory<IOBSPluginService>(
                new NetNamedPipeBinding() { MaxReceivedMessageSize = 8294400*2, MaxBufferSize = 8294400*2 },
                new EndpointAddress(
                "net.pipe://localhost/ImageSource"));
            if (pipeFactory == null)
            {
                Debug.Print("OBSPlugin: can't create IOBSPluginService channel");
            }
            else
            {

                pipeProxy = pipeFactory.CreateChannel();
            }

            if (pipeProxy == null)
            {
                Debug.Print("OBSPlugin: pipe proxy is null");
            }
            imageData = pipeProxy.GetImage();
            if( imageData == null )
            {
                texture = GS.CreateTexture(100, 100, GSColorFormat.GS_BGRA, null, false, false);
                Size.X = 100;
                Size.Y = 100;
            }
            else
            {
                UpdateSettings();
            }

        }

        public override void Render(float x, float y, float width, float height)
        {
            try
            {
                lock(imageLock)
                {
                    imageData = pipeProxy.GetImage();
                    if (imageData == null)
                        return;
                }
            }
            catch(Exception e)
            {
                Debug.Print("OBSPlugin GetImage exception: {0}", e.Message);
            }
            
            if (texture == null)
                Debug.Print("OBSPlugin: failed to create texture");
            else
            {
                lock(imageLock)
                {
                    UpdateSettings();
                    texture.SetImage(imageData.Pixels, GSImageFormat.GS_IMAGEFORMAT_BGRA, (UInt32)(imageData.Size.Width * 4));
                }
            }

            if (texture != null)
                GS.DrawSprite(texture, 0xFFFFFFFF, x, y, x + width, y + height);
            else
                Debug.Print("OBSPlugin: null texture");

        }

        public override void UpdateSettings()
        {
            if( imageData == null )
                return;

            if (currentSize.Width != imageData.Size.Width ||
            currentSize.Height != imageData.Size.Height)
            {
                currentSize.Height = imageData.Size.Height;
                currentSize.Width = imageData.Size.Width;
                Size.X = currentSize.Width;
                Size.Y = currentSize.Height;
                texture = GS.CreateTexture((uint)imageData.Size.Width, (uint)imageData.Size.Height, GSColorFormat.GS_BGRA, null, false, false);
            }
        }
    }
}
