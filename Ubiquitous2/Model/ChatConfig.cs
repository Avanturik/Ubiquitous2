using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Practices.ServiceLocation;

namespace UB.Model
{
    [Serializable]
    [XmlRoot(ElementName="ChatConfigs")]
    public class ChatConfig
    {
        [XmlAttribute]
        public String ChatName { get; set; }
        [XmlAttribute]
        public String IconURL { get; set; }
        [XmlAttribute]
        public bool Enabled { get; set; }
        [XmlArray]
        public List<ConfigField> Parameters { get; set; }
    }
}
