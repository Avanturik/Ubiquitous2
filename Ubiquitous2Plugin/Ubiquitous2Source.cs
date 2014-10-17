using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLROBS;
using System.ServiceModel;
using UB.Model;
using System.Diagnostics;

namespace Ubiquitous2Plugin
{
    internal class Ubiquitous2Source : AbstractImageSource
    {
        private ChannelFactory<IOBSPluginService> pipeFactory;
        private Texture texture;
        private IOBSPluginService pipeProxy;
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

            texture = GS.CreateTexture(100, 100, GSColorFormat.GS_BGRA, null, false, false);

        }

        public override void Render(float x, float y, float width, float height)
        {
            byte[] bytes = null;
            try
            {
                bytes = pipeProxy.GetImage();
            }
            catch(Exception e)
            {
                Debug.Print("OBSPlugin GetImage exception: {0}", e.Message);
            }
            if (bytes == null)
            {
                Debug.Print("OBSPlugin got null image");
            }
            else
            {
                if (texture == null)
                    Debug.Print("OBSPlugin: failed to create texture");
                else
                {
                    texture.SetImage(bytes, GSImageFormat.GS_IMAGEFORMAT_BGRA, (UInt32)(100 * 4));
                }

                if (texture != null)
                    GS.DrawSprite(texture, 0xFFFFFFFF, x,y, width,height);
                else
                    Debug.Print("OBSPlugin: null texture");


            }
        }

        public override void UpdateSettings()
        {
            Size.X = 100;
            Size.Y = 100;
        }
    }
}
