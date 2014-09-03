using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace UB.Model
{
    [Serializable]
    [DataContract]
    public class ConfigField
    {
        [XmlAttribute]
        public bool IsVisible { get; set; }

        [XmlAttribute]
        public String DataType { get; set; }

        [XmlAttribute]
        public String Name { get; set; }

        [XmlAttribute]
        public String Label { get; set; }

        [XmlElement]
        public object Value { get; set; }

        [XmlIgnore]
        public Action<object> Update { get;set; }
    }
}
