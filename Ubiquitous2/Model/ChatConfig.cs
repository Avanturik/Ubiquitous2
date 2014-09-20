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
    [XmlRoot(ElementName="ChatConfig")]
    public class ChatConfig : NotifyPropertyChangeBase
    {
        [XmlAttribute]
        public String ChatName { get; set; }
        [XmlAttribute]
        public String IconURL { get; set; }
        [XmlAttribute]
        public bool Enabled { get; set; }
        [XmlAttribute]
        /// <summary>
        /// The <see cref="HideViewersCounter" /> property's name.
        /// </summary>
        public const string HideViewersCounterPropertyName = "HideViewersCounter";

        private bool _hideViewersCounter = false;

        /// <summary>
        /// Sets and gets the HideViewersCounter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool HideViewersCounter
        {
            get
            {
                return _hideViewersCounter;
            }

            set
            {
                if (_hideViewersCounter == value)
                {
                    return;
                }

                RaisePropertyChanging(HideViewersCounterPropertyName);
                _hideViewersCounter = value;
                RaisePropertyChanged(HideViewersCounterPropertyName);
            }
        }
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
        public object GetParameterValue(string name)
        {
            var searchParameter = Parameters.FirstOrDefault(parameter => parameter.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (searchParameter == null)
                return null;

            return searchParameter.Value;
        }
        public void SetParameterValue(string name, object value)
        {            
            var searchParameter = Parameters.FirstOrDefault(parameter => parameter.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (searchParameter != null)
                searchParameter.Value = value;

        }
        public ChatConfig Clone()
        {
            var clone = new ChatConfig()
            {
                ChatName = this.ChatName,
                Enabled = this.Enabled,
                IconURL = this.IconURL,
                HideViewersCounter = this.HideViewersCounter,
                Parameters = new List<ConfigField>()
            };
            foreach( var param in this.Parameters.ToList() )
            {
                clone.Parameters.Add( param );
            }

            return clone;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
