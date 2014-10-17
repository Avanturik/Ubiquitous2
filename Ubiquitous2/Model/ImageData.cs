using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace UB.Model
{
    [DataContract]
    public class ImageData
    {
        [DataMember]
        public byte[] Pixels { get; set; }
        [DataMember]
        public Size Size { get; set; }
    }
}
