using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace UB.Model
{
    [Serializable]
    [DataContract]
    public class ConfigField
    {
        [XmlElement]
        public bool IsVisible { get; set; }

        [XmlElement]
        public String DataType { get; set; }

        [XmlElement]
        public String Name { get; set; }

        [XmlElement]
        public String Label { get; set; }

        [XmlElement]
        public object Value { get; set; }

        //public override bool Equals(object o)
        //{
        //    var obj = o as ConfigField;
        //    if (this.IsVisible == obj.IsVisible &&
        //        this.DataType == obj.DataType &&
        //        this.Name == obj.Name &&
        //        this.Label == obj.Label &&
        //        this.Value == obj.Value)
        //        return true;

        //    return false;
        //}
        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}

    }
}
