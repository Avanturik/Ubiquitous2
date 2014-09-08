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

        public override bool Equals(object o)
        {
            bool result = false;
            var obj = o as ChatConfig;

            if (obj == null)
                return false;

            if( this.ChatName == obj.ChatName &&
                this.IconURL == obj.IconURL &&
                this.Enabled == obj.Enabled )
            {
                if (obj.Parameters == null && this.Parameters == null)
                    return true;

                result = obj.Parameters.Except(this.Parameters).Count() == 0 && 
                    this.Parameters.Except(obj.Parameters).Count() == 0;
            }

            return result;
        }

        public ChatConfig Clone()
        {
            var clone = new ChatConfig()
            {
                ChatName = this.ChatName,
                Enabled = this.Enabled,
                IconURL = this.IconURL,
                Parameters = new List<ConfigField>()
            };
            foreach( var param in this.Parameters.ToList() )
            {
                clone.Parameters.Add( param );
            }

            return clone;
        }
    }
}
