using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace UB.Model
{

    [Serializable]
    [DataContract]
    public class WindowSettings : NotifyPropertyChangeBase
    {
        public WindowSettings()
        {

        }
        /// <summary>
        /// The <see cref="WindowName" /> property's name.
        /// </summary>
        public const string WindowNamePropertyName = "WindowName";

        private string _windowName = null;

        /// <summary>
        /// Sets and gets the WindowName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public string WindowName
        {
            get
            {
                return _windowName;
            }

            set
            {
                if (_windowName == value)
                {
                    return;
                }

                RaisePropertyChanging(WindowNamePropertyName);
                _windowName = value;
                RaisePropertyChanged(WindowNamePropertyName);
            }
        }


        /// <summary>
        /// The <see cref="Top" /> property's name.
        /// </summary>
        public const string TopPropertyName = "Top";

        private double _top = 0;

        /// <summary>
        /// Sets and gets the Top property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public double Top
        {
            get
            {
                return _top;
            }

            set
            {
                if (_top == value)
                {
                    return;
                }

                RaisePropertyChanging(TopPropertyName);
                _top = value;
                RaisePropertyChanged(TopPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Left" /> property's name.
        /// </summary>
        public const string LeftPropertyName = "Left";

        private double _left = 0;

        /// <summary>
        /// Sets and gets the Left property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public double Left
        {
            get
            {
                return _left;
            }

            set
            {
                if (_left == value)
                {
                    return;
                }

                RaisePropertyChanging(LeftPropertyName);
                _left = value;
                RaisePropertyChanged(LeftPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="Width" /> property's name.
        /// </summary>
        public const string WidthPropertyName = "Width";

        private double _width = 0;

        /// <summary>
        /// Sets and gets the Width property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public double Width
        {
            get
            {
                return _width;
            }

            set
            {
                if (_width == value)
                {
                    return;
                }

                RaisePropertyChanging(WidthPropertyName);
                _width = value;
                RaisePropertyChanged(WidthPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Height" /> property's name.
        /// </summary>
        public const string HeightPropertyName = "Height";

        private double _height = 0;

        /// <summary>
        /// Sets and gets the Height property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Height
        {
            get
            {
                return _height;
            }

            set
            {
                if (_height == value)
                {
                    return;
                }

                RaisePropertyChanging(HeightPropertyName);
                _height = value;
                RaisePropertyChanged(HeightPropertyName);
            }
        }
        
    }
}
