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
        /// The <see cref="ForceAutoScroll" /> property's name.
        /// </summary>
        public const string ForceAutoScrollPropertyName = "ForceAutoScroll";

        private bool _forceAutoScroll = false;

        /// <summary>
        /// Sets and gets the ForceAutoScroll property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool ForceAutoScroll
        {
            get
            {
                return _forceAutoScroll;
            }

            set
            {
                if (_forceAutoScroll == value)
                {
                    return;
                }

                _forceAutoScroll = value;
                RaisePropertyChanged(ForceAutoScrollPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="FontSizeTimestamp" /> property's name.
        /// </summary>
        public const string FontSizeTimestampPropertyName = "FontSizeTimestamp";

        private double _fontSizeTimestamp = 11;

        /// <summary>
        /// Sets and gets the FontSizeTimestamp property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double FontSizeTimestamp
        {
            get
            {
                return _fontSizeTimestamp;
            }

            set
            {
                if (_fontSizeTimestamp == value)
                {
                    return;
                }

                RaisePropertyChanging(FontSizeTimestampPropertyName);
                _fontSizeTimestamp = value;
                RaisePropertyChanged(FontSizeTimestampPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="FontSizeNickName" /> property's name.
        /// </summary>
        public const string FontSizeNickNamePropertyName = "FontSizeNickName";

        private double _fontSizeNickName = 11;

        /// <summary>
        /// Sets and gets the FontSizeNickName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double FontSizeNickName
        {
            get
            {
                return _fontSizeNickName;
            }

            set
            {
                if (_fontSizeNickName == value)
                {
                    return;
                }

                RaisePropertyChanging(FontSizeNickNamePropertyName);
                _fontSizeNickName = value;
                RaisePropertyChanged(FontSizeNickNamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="FontSizeChannel" /> property's name.
        /// </summary>
        public const string FontSizeChannelPropertyName = "FontSizeChannel";

        private double _fontSizeChannel = 11;

        /// <summary>
        /// Sets and gets the FontSizeChannel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double FontSizeChannel
        {
            get
            {
                return _fontSizeChannel;
            }

            set
            {
                if (_fontSizeChannel == value)
                {
                    return;
                }

                RaisePropertyChanging(FontSizeChannelPropertyName);
                _fontSizeChannel = value;
                RaisePropertyChanged(FontSizeChannelPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="FontSizeMessage" /> property's name.
        /// </summary>
        public const string FontSizeMessagePropertyName = "FontSizeMessage";

        private double _fontSizeMessage = 16;

        /// <summary>
        /// Sets and gets the FontSizeMessage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double FontSizeMessage
        {
            get
            {
                return _fontSizeMessage;
            }

            set
            {
                if (_fontSizeMessage == value)
                {
                    return;
                }

                RaisePropertyChanging(FontSizeMessagePropertyName);
                _fontSizeMessage = value;
                RaisePropertyChanged(FontSizeMessagePropertyName);
            }
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
        /// The <see cref="IndividualMessageBackgroundOpacity" /> property's name.
        /// </summary>
        public const string IndividualMessageBackgroundOpacityPropertyName = "IndividualMessageBackgroundOpacity";

        private double _individualMessageBackgroundOpacity = 0.8;

        /// <summary>
        /// Sets and gets the IndividualMessageBackgroundOpacity property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public double IndividualMessageBackgroundOpacity
        {
            get
            {
                return _individualMessageBackgroundOpacity;
            }

            set
            {
                if (_individualMessageBackgroundOpacity == value)
                {
                    return;
                }

                RaisePropertyChanging(IndividualMessageBackgroundOpacityPropertyName);
                _individualMessageBackgroundOpacity = value;
                RaisePropertyChanged(IndividualMessageBackgroundOpacityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="MusicTickerBackgroundOpacity" /> property's name.
        /// </summary>
        public const string MusicTickerBackgroundOpacityPropertyName = "MusicTickerBackgroundOpacity";

        private double _musicTickerBackgroundOpacity = 0.8;

        /// <summary>
        /// Sets and gets the MusicTickerBackgroundOpacity property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public double MusicTickerBackgroundOpacity
        {
            get
            {
                return _musicTickerBackgroundOpacity;
            }

            set
            {
                if (_musicTickerBackgroundOpacity == value)
                {
                    return;
                }

                RaisePropertyChanging(MusicTickerBackgroundOpacityPropertyName);
                _musicTickerBackgroundOpacity = value;
                RaisePropertyChanged(MusicTickerBackgroundOpacityPropertyName);
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
