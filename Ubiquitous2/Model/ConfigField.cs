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

        public override bool Equals(object o)
        {
            var obj = o as ConfigField;
            if (this.IsVisible == obj.IsVisible &&
                this.DataType == obj.DataType &&
                this.Name == obj.Name &&
                this.Label == obj.Label &&
                this.Value == obj.Value)
                return true;

            return false;
        }

    }
}
