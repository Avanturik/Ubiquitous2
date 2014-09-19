using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace UB.Model
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        public RECT normalPosition;
    }

    public static class WindowPlacement
    {
        private static Encoding encoding = new UTF8Encoding();
        private static XmlSerializer serializer = new XmlSerializer(typeof(WINDOWPLACEMENT));

        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;

        public static void SetPlacement(IntPtr windowHandle, WindowSettings settings)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.maxPosition.X = settings.MaxPositionX;
            placement.maxPosition.Y = settings.MaxPositionY;
            placement.minPosition.X = settings.MinPositionX;
            placement.minPosition.Y = settings.MinPositionY;
            placement.normalPosition.Bottom = settings.Bottom;
            placement.normalPosition.Left = settings.Left;
            placement.normalPosition.Right = settings.Right;
            placement.normalPosition.Top = settings.Top;

            placement.showCmd = (settings.WindowState == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : settings.WindowState);
            placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            placement.flags = 0;

            if( SetWindowPlacement(windowHandle, ref placement) )
            {
                Log.WriteInfo("Window {0} position set", settings.WindowName);
            }
        }

        public static WindowSettings GetPlacement(IntPtr windowHandle)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            GetWindowPlacement(windowHandle, out placement);

            return new WindowSettings(placement);
        }
    }


    [Serializable]
    [DataContract]
    public class WindowSettings : NotifyPropertyChangeBase
    {
        public WindowSettings()
        {

        }
        public WindowSettings(WINDOWPLACEMENT placement)
        {
            WindowState = placement.showCmd;
            Left = placement.normalPosition.Left;
            Top = placement.normalPosition.Top;
            Right = placement.normalPosition.Right;
            Bottom = placement.normalPosition.Bottom;
            MaxPositionX = placement.maxPosition.X;
            MaxPositionY = placement.maxPosition.Y;
            MinPositionX = placement.minPosition.X;
            MinPositionY = placement.minPosition.Y;
        }
        public void CopyAllFrom(WindowSettings settings)
        {
            CopyLocationFrom(settings);
            CopyStateFrom(settings);
            CopyLimitsFrom(settings);
        }
        public void CopyLocationFrom( WindowSettings settings )
        {
            Left = settings.Left;
            Top = settings.Top;
            Right = settings.Right;
            Bottom = settings.Bottom;
        }
        public void CopyStateFrom( WindowSettings settings)
        {
            WindowState = settings.WindowState;
        }
        public void CopyLimitsFrom( WindowSettings settings)
        {
            MaxPositionX = settings.MaxPositionX;
            MaxPositionY = settings.MaxPositionY;
            MinPositionX = settings.MaxPositionX;
            MinPositionY = settings.MinPositionY;
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
        /// The <see cref="Bottom" /> property's name.
        /// </summary>
        public const string BottomPropertyName = "Bottom";

        private int _bottom = 0;

        /// <summary>
        /// Sets and gets the Bottom property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public int Bottom
        {
            get
            {
                return _bottom;
            }

            set
            {
                if (_bottom == value)
                {
                    return;
                }

                RaisePropertyChanging(BottomPropertyName);
                _bottom = value;
                RaisePropertyChanged(BottomPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Right" /> property's name.
        /// </summary>
        public const string RightPropertyName = "Right";

        private int _right = 0;

        /// <summary>
        /// Sets and gets the Right property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public int Right
        {
            get
            {
                return _right;
            }

            set
            {
                if (_right == value)
                {
                    return;
                }

                RaisePropertyChanging(RightPropertyName);
                _right = value;
                RaisePropertyChanged(RightPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Top" /> property's name.
        /// </summary>
        public const string TopPropertyName = "Top";

        private int _top = 0;

        /// <summary>
        /// Sets and gets the Top property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public int Top
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

        private int _left = 0;

        /// <summary>
        /// Sets and gets the Left property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public int Left
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
        /// The <see cref="MinPositionY" /> property's name.
        /// </summary>
        public const string MinPositionYPropertyName = "MinPositionY";

        private int _minPositionY = -32000;

        /// <summary>
        /// Sets and gets the MinPositionY property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public int MinPositionY
        {
            get
            {
                return _minPositionY;
            }

            set
            {
                if (_minPositionY == value)
                {
                    return;
                }

                RaisePropertyChanging(MinPositionYPropertyName);
                _minPositionY = value;
                RaisePropertyChanged(MinPositionYPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="MinPositionX" /> property's name.
        /// </summary>
        public const string MinPositionXPropertyName = "MinPositionX";

        private int _minPositionX = -32000;

        /// <summary>
        /// Sets and gets the MinPositionX property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public int MinPositionX
        {
            get
            {
                return _minPositionX;
            }

            set
            {
                if (_minPositionX == value)
                {
                    return;
                }

                RaisePropertyChanging(MinPositionXPropertyName);
                _minPositionX = value;
                RaisePropertyChanged(MinPositionXPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="MaxPositionY" /> property's name.
        /// </summary>
        public const string MaxPositionYPropertyName = "MaxPositionY";

        private int _maxPositionY = 32000;

        /// <summary>
        /// Sets and gets the MaxPositionY property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public int MaxPositionY
        {
            get
            {
                return _maxPositionY;
            }

            set
            {
                if (_maxPositionY == value)
                {
                    return;
                }

                RaisePropertyChanging(MaxPositionYPropertyName);
                _maxPositionY = value;
                RaisePropertyChanged(MaxPositionYPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="MaxPositionX" /> property's name.
        /// </summary>
        public const string MaxPositionXPropertyName = "MaxPositionX";

        private int _maxPositionX = 32000;

        /// <summary>
        /// Sets and gets the MaxPositionX property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public int MaxPositionX
        {
            get
            {
                return _maxPositionX;
            }

            set
            {
                if (_maxPositionX == value)
                {
                    return;
                }

                RaisePropertyChanging(MaxPositionXPropertyName);
                _maxPositionX = value;
                RaisePropertyChanged(MaxPositionXPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="WindowState" /> property's name.
        /// </summary>
        public const string WindowStatePropertyName = "WindowState";

        private int _state = 1;

        /// <summary>
        /// Sets and gets the WindowState property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public int WindowState
        {
            get
            {
                return _state;
            }

            set
            {
                if (_state == value)
                {
                    return;
                }

                RaisePropertyChanging(WindowStatePropertyName);
                _state = value;
                RaisePropertyChanged(WindowStatePropertyName);
            }
        }
    }
}
