using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace UB.Model
{
    [Serializable]
    [DataContract]
    public class ConfigField :NotifyPropertyChangeBase
    {
        public ConfigField()
        {

        }
        public ConfigField(string name, string label, string dataType, bool isVisible, object value  )
        {
            Name = name;
            Label = label;
            DataType = dataType;
            IsVisible = isVisible;
            Value = value;
        }
        /// <summary>
        /// The <see cref="IsVisible" /> property's name.
        /// </summary>
        public const string IsVisiblePropertyName = "IsVisible";

        private bool _isVisible = false;

        /// <summary>
        /// Sets and gets the IsVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }

            set
            {
                if (_isVisible == value)
                {
                    return;
                }

                RaisePropertyChanging(IsVisiblePropertyName);
                _isVisible = value;
                RaisePropertyChanged(IsVisiblePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="DataType" /> property's name.
        /// </summary>
        public const string DataTypePropertyName = "DataType";

        private string _dataType = "";

        /// <summary>
        /// Sets and gets the DataType property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public string DataType
        {
            get
            {
                return _dataType;
            }

            set
            {
                if (_dataType == value)
                {
                    return;
                }

                RaisePropertyChanging(DataTypePropertyName);
                _dataType = value;
                RaisePropertyChanged(DataTypePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Name" /> property's name.
        /// </summary>
        public const string NamePropertyName = "Name";

        private string _name = "";

        /// <summary>
        /// Sets and gets the Name property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (_name == value)
                {
                    return;
                }

                RaisePropertyChanging(NamePropertyName);
                _name = value;
                RaisePropertyChanged(NamePropertyName);
            }
        }
        /// <summary>
        /// The <see cref="Label" /> property's name.
        /// </summary>
        public const string LabelPropertyName = "Label";

        private string _label = "";

        /// <summary>
        /// Sets and gets the Label property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public string Label
        {
            get
            {
                return _label;
            }

            set
            {
                if (_label == value)
                {
                    return;
                }

                RaisePropertyChanging(LabelPropertyName);
                _label = value;
                RaisePropertyChanged(LabelPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Value" /> property's name.
        /// </summary>
        public const string ValuePropertyName = "Value";

        private object _value = null;

        /// <summary>
        /// Sets and gets the Value property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlElement]
        public object Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (_value == value)
                {
                    return;
                }

                RaisePropertyChanging(ValuePropertyName);
                _value = value;
                RaisePropertyChanged(ValuePropertyName);
            }
        }
    }
}
