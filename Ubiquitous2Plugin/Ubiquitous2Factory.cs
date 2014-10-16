using CLROBS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ubiquitous2Plugin
{
    internal class Ubiquitous2Factory : AbstractImageSourceFactory
    {
        public Ubiquitous2Factory()
        {
            ClassName = "Ubiquitous2";
            DisplayName = "Ubiquitous2 chat";
        }

        public override bool ShowConfiguration(XElement data)
        {
            return true;
        }

        public override ImageSource Create(XElement data)
        {
            return new Ubiquitous2Source(data);
        }
    }
}
