using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UB.Model
{
    public class AppConfig :NotifyPropertyChangeBase
    {
        public AppConfig()
        {
            Parameters = new List<ConfigField>();
        }

        /// <summary>
        /// The <see cref="MouseTransparency" /> property's name.
        /// </summary>
        public const string MouseTransparencyPropertyName = "MouseTransparency";

        private bool _mouseTransparency = false;

        /// <summary>
        /// Sets and gets the MouseTransparency property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public bool MouseTransparency
        {
            get
            {
                return _mouseTransparency;
            }

            set
            {
                if (_mouseTransparency == value)
                {
                    return;
                }

                RaisePropertyChanging(MouseTransparencyPropertyName);
                _mouseTransparency = value;
                RaisePropertyChanged(MouseTransparencyPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="MessageBackgroundOpacity" /> property's name.
        /// </summary>
        public const string MessageBackgroundOpacityPropertyName = "MessageBackgroundOpacity";

        private double _messageBackgroundOpacity = 0.8;

        /// <summary>
        /// Sets and gets the MessageBackgroundOpacity property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public double MessageBackgroundOpacity
        {
            get
            {
                return _messageBackgroundOpacity;
            }

            set
            {
                if (_messageBackgroundOpacity == value)
                {
                    return;
                }

                RaisePropertyChanging(MessageBackgroundOpacityPropertyName);
                _messageBackgroundOpacity = value;
                RaisePropertyChanged(MessageBackgroundOpacityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="BackgroundOpacity" /> property's name.
        /// </summary>
        public const string BackgroundOpacityPropertyName = "BackgroundOpacity";

        private double _backgroundOpacity = 0.8;

        /// <summary>
        /// Sets and gets the BackgroundOpacity property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public double BackgroundOpacity
        {
            get
            {
                return _backgroundOpacity;
            }

            set
            {
                if (_backgroundOpacity == value)
                {
                    return;
                }

                RaisePropertyChanging(BackgroundOpacityPropertyName);
                _backgroundOpacity = value;
                RaisePropertyChanged(BackgroundOpacityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="EnableTransparency" /> property's name.
        /// </summary>
        public const string EnableTransparencyPropertyName = "EnableTransparency";

        private bool _enableTransparency = false;

        /// <summary>
        /// Sets and gets the EnableTransparency property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public bool EnableTransparency
        {
            get
            {
                return _enableTransparency;
            }

            set
            {
                if (_enableTransparency == value)
                {
                    return;
                }

                RaisePropertyChanging(EnableTransparencyPropertyName);
                _enableTransparency = value;
                RaisePropertyChanged(EnableTransparencyPropertyName);
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

        /// <summary>
        /// The <see cref="ThemeName" /> property's name.
        /// </summary>
        public const string ThemeNamePropertyName = "ThemeName";

        private string _themeName = null;

        /// <summary>
        /// Sets and gets the ThemeName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public string ThemeName
        {
            get
            {
                return _themeName;
            }

            set
            {
                if (_themeName == value)
                {
                    return;
                }

                RaisePropertyChanging(ThemeNamePropertyName);
                _themeName = value;
                RaisePropertyChanged(ThemeNamePropertyName);
            }
        }


    }
}
