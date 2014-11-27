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
using System.Threading;

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
        private bool isConnecting = true;
        public Ubiquitous2Source(XElement configElement)
        {
            UpdateSettings();
            Task.Run(() => ConnectWCF());
        }
        public void ConnectWCF()
        {
            isConnecting = true;
            
            lock(imageLock)
                imageData = null;

            while( imageData == null )
            {
                Debug.Print("OBS plugin is trying to reconnect...");
                try
                {
                    if( pipeFactory == null )
                    {
                        pipeFactory =
                            new ChannelFactory<IOBSPluginService>(
                            new NetNamedPipeBinding() { MaxReceivedMessageSize = 8294400 * 2, MaxBufferSize = 8294400 * 2 },
                            new EndpointAddress(
                            "net.pipe://localhost/ImageSource"));

                      
                    }

                    if (pipeProxy == null)
                    {
                        pipeProxy = pipeFactory.CreateChannel();
                    }  

                    if (pipeProxy != null)
                    {
                        Debug.Print("OBSPlugin: trying to get first image");
                        lock(imageLock )
                            imageData = pipeProxy.GetFirstImage();

                        if (imageData == null)
                        {
                            Debug.Print("OBSPlugin: got null image data");
                            lock( imageLock )
                                texture = GS.CreateTexture(100, 100, GSColorFormat.GS_BGRA, null, false, false);
                            
                            Size.X = 100;
                            Size.Y = 100;
                        }
                        else
                        {
                            UpdateSettings();
                            break;
                        }
                        
                    }     
                    else
                        Debug.Print("OBSPlugin: pipe proxy is null");
                }
                catch
                {
                    pipeProxy = null;         
                }

                Thread.Sleep(1000);
            }
            isConnecting = false;

        }
        public override void Render(float x, float y, float width, float height)
        {
            if (isConnecting)
                return;
            try
            {
                lock(imageLock)
                    imageData = pipeProxy.GetImage();
            }
            catch
            {
                isConnecting = true;
                lock( imageLock )
                {
                    texture = GS.CreateTexture((uint)Size.X, (uint)Size.Y, GSColorFormat.GS_BGRA, null, false, false);
                    GS.DrawSprite(texture, 0xFFFFFFFF, x, y, x + width, y + height);
                }

                Task.Run(() => ConnectWCF());
            }

            if (imageData!= null && texture != null)
            {

                UpdateSettings();
                lock(imageLock)
                    texture.SetImage(imageData.Pixels, GSImageFormat.GS_IMAGEFORMAT_BGRA, (UInt32)(imageData.Size.Width * 4));
            }
            if( texture != null )
                lock( imageLock )
                    GS.DrawSprite(texture, 0xFFFFFFFF, x, y, x + width, y + height);
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
                lock (imageLock)
                    texture = GS.CreateTexture((uint)imageData.Size.Width, (uint)imageData.Size.Height, GSColorFormat.GS_BGRA, null, false, false);
            }
        }
    }
}
