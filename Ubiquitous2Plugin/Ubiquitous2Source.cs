using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLROBS;

namespace Ubiquitous2Plugin
{
    internal class Ubiquitous2Source : AbstractImageSource
    {
        public Ubiquitous2Source(XElement configElement)
        {

        }

        public override void Render(float x, float y, float width, float height)
        {
            GS.DrawSprite(GS.CreateTexture(100, 100, GSColorFormat.GS_BGRA, null, false, false), 0x80FFFFFF, 1, 1, 100, 100);
        }

        public override void UpdateSettings()
        {
            
        }
    }
}
