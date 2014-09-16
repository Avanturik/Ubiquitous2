using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace UB.Model
{
    public class ChatStatusBase : NotifyPropertyChangeBase, IChatStatus
    {
        public ChatStatusBase()
        {
            ResetToDefault();
        }
        public void ResetToDefault()
        {
            IsGotAuthenticationInfo = false;
            IsLoginFailed = false;
            IsLoggedIn = false;
            IsConnected = false;
            IsJoined = false;
            IsStopping = false;
            IsStarting = false;
            IsConnecting = false;
        }

        /// <summary>
        /// The <see cref="ViewersCount" /> property's name.
        /// </summary>
        public const string ViewersCountPropertyName = "ViewersCount";

        private int _viewersCount = 0;

        /// <summary>
        /// Sets and gets the ViewersCount property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int ViewersCount
        {
            get
            {
                return _viewersCount;
            }

            set
            {
                if (_viewersCount == value)
                {
                    return;
                }

                RaisePropertyChanging(ViewersCountPropertyName);
                _viewersCount = value;
                RaisePropertyChanged(ViewersCountPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="IsConnected" /> property's name.
        /// </summary>
        public const string IsConnectedPropertyName = "IsConnected";

        private bool _isConnected = false;

        /// <summary>
        /// Sets and gets the IsConnected property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }

            set
            {
                if (_isConnected == value)
                {
                    return;
                }

                RaisePropertyChanging(IsConnectedPropertyName);
                _isConnected = value;
                RaisePropertyChanged(IsConnectedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsJoined" /> property's name.
        /// </summary>
        public const string IsJoinedPropertyName = "IsJoined";

        private bool _isJoined = false;

        /// <summary>
        /// Sets and gets the IsJoined property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsJoined
        {
            get
            {
                return _isJoined;
            }

            set
            {
                if (_isJoined == value)
                {
                    return;
                }

                RaisePropertyChanging(IsJoinedPropertyName);
                _isJoined = value;
                RaisePropertyChanged(IsJoinedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsGotAuthenticationInfo" /> property's name.
        /// </summary>
        public const string IsGotAuthenticationInfoPropertyName = "IsGotAuthenticationInfo";

        private bool _isGotAuthenticationInfo = false;

        /// <summary>
        /// Sets and gets the IsGotAuthenticationInfo property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsGotAuthenticationInfo
        {
            get
            {
                return _isGotAuthenticationInfo;
            }

            set
            {
                if (_isGotAuthenticationInfo == value)
                {
                    return;
                }

                RaisePropertyChanging(IsGotAuthenticationInfoPropertyName);
                _isGotAuthenticationInfo = value;
                RaisePropertyChanged(IsGotAuthenticationInfoPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsLoginFailed" /> property's name.
        /// </summary>
        public const string IsLoginFailedPropertyName = "IsLoginFailed";

        private bool _isLoginFailed = false;

        /// <summary>
        /// Sets and gets the IsLoginFailed property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsLoginFailed
        {
            get
            {
                return _isLoginFailed;
            }

            set
            {
                if (_isLoginFailed == value)
                {
                    return;
                }

                RaisePropertyChanging(IsLoginFailedPropertyName);
                _isLoginFailed = value;
                RaisePropertyChanged(IsLoginFailedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsLoggedIn" /> property's name.
        /// </summary>
        public const string IsLoggedInPropertyName = "IsLoggedIn";

        private bool _isLoggedIn = false;

        /// <summary>
        /// Sets and gets the IsLoggedIn property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsLoggedIn
        {
            get
            {
                return _isLoggedIn;
            }

            set
            {
                if (_isLoggedIn == value)
                {
                    return;
                }

                RaisePropertyChanging(IsLoggedInPropertyName);
                _isLoggedIn = value;
                RaisePropertyChanged(IsLoggedInPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsStopping" /> property's name.
        /// </summary>
        public const string IsStoppingPropertyName = "IsStopping";

        private bool _isStopping = false;

        /// <summary>
        /// Sets and gets the IsStopping property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsStopping
        {
            get
            {
                return _isStopping;
            }

            set
            {
                if (_isStopping == value)
                {
                    return;
                }

                RaisePropertyChanging(IsStoppingPropertyName);
                _isStopping = value;
                RaisePropertyChanged(IsStoppingPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsStarting" /> property's name.
        /// </summary>
        public const string IsStartingPropertyName = "IsStarting";

        private bool _isStarting = false;

        /// <summary>
        /// Sets and gets the IsStarting property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsStarting
        {
            get
            {
                return _isStarting;
            }

            set
            {
                if (_isStarting == value)
                {
                    return;
                }

                RaisePropertyChanging(IsStartingPropertyName);
                _isStarting = value;
                RaisePropertyChanged(IsStartingPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsConnecting" /> property's name.
        /// </summary>
        public const string IsConnectingPropertyName = "IsConnecting";

        private bool _isConnecting = false;

        /// <summary>
        /// Sets and gets the IsConnecting property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsConnecting
        {
            get
            {
                return _isConnecting;
            }

            set
            {
                if (_isConnecting == value)
                {
                    return;
                }

                RaisePropertyChanging(IsConnectingPropertyName);
                _isConnecting = value;
                RaisePropertyChanged(IsConnectingPropertyName);
            }
        }



        /// <summary>
        /// The <see cref="ToolTips" /> property's name.
        /// </summary>
        public const string ToolTipsPropertyName = "ToolTips";

        private ObservableCollection<ToolTip> _toolTips = new ObservableCollection<ToolTip>();

        /// <summary>
        /// Sets and gets the ToolTips property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ToolTip> ToolTips
        {
            get
            {
                return _toolTips;
            }

            set
            {
                if (_toolTips == value)
                {
                    return;
                }

                RaisePropertyChanging(ToolTipsPropertyName);
                _toolTips = value;
                RaisePropertyChanged(ToolTipsPropertyName);
            }
        }
    }
}
