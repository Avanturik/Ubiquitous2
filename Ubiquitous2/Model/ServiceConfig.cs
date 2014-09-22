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
    [XmlRoot(ElementName="ServiceConfig")]
    public class ServiceConfig : NotifyPropertyChangeBase
    {
        /// <summary>
        /// The <see cref="ServiceName" /> property's name.
        /// </summary>
        public const string ServiceNamePropertyName = "ServiceName";

        private string _serviceName = "";

        /// <summary>
        /// Sets and gets the ServiceName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public string ServiceName
        {
            get
            {
                return _serviceName;
            }

            set
            {
                if (_serviceName == value)
                {
                    return;
                }

                RaisePropertyChanging(ServiceNamePropertyName);
                _serviceName = value;
                RaisePropertyChanged(ServiceNamePropertyName);
            }
        }
        /// <summary>
        /// The <see cref="IconURL" /> property's name.
        /// </summary>
        public const string IconURLPropertyName = "IconURL";

        private string _iconURL = "";

        /// <summary>
        /// Sets and gets the IconURL property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public string IconURL
        {
            get
            {
                return _iconURL;
            }

            set
            {
                if (_iconURL == value)
                {
                    return;
                }

                RaisePropertyChanging(IconURLPropertyName);
                _iconURL = value;
                RaisePropertyChanged(IconURLPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Enabled" /> property's name.
        /// </summary>
        public const string EnabledPropertyName = "Enabled";

        private bool _enabled = false;

        /// <summary>
        /// Sets and gets the Enabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public bool Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                if (_enabled == value)
                {
                    return;
                }

                RaisePropertyChanging(EnabledPropertyName);
                _enabled = value;
                RaisePropertyChanged(EnabledPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Parameters" /> property's name.
        /// </summary>
        public const string ParametersPropertyName = "Parameters";

        private List<ConfigField> _parameters = new List<ConfigField>();

        /// <summary>
        /// Sets and gets the Parameters property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlArray]
        public List<ConfigField> Parameters
        {
            get
            {
                return _parameters;
            }

            set
            {
                if (_parameters == value)
                {
                    return;
                }

                RaisePropertyChanging(ParametersPropertyName);
                _parameters = value;
                RaisePropertyChanged(ParametersPropertyName);
            }
        }

        public object GetParameterValue(string name)
        {
            var searchParameter = Parameters.FirstOrDefault(parameter => parameter.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return searchParameter.Value;
        }
        
        public void SetParameterValue(string name, object value)
        {
            var searchParameter = Parameters.FirstOrDefault(parameter => parameter.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (searchParameter != null)
                searchParameter.Value = value;

        }
        public override bool Equals(object o)
        {
            bool result = false;
            var obj = o as ServiceConfig;

            if (obj == null)
                return false;

            if( this.ServiceName == obj.ServiceName &&
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
                ChatName = this.ServiceName,
                Enabled = this.Enabled,
                IconURL = this.IconURL,
                Parameters = new List<ConfigField>()
            };
            clone.Parameters = this.Parameters.ToList();
            return clone;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
