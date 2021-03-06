﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using System.Linq;
using UB.Model;
using System.Windows.Threading;
using System.Threading;
using GalaSoft.MvvmLight.Threading;
using UB.Utils;
using UB.View;
using System.Windows;
using System.Web;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UB.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IChatDataService _dataService;
        private readonly IGeneralDataService _generalDataService;
        private readonly ISettingsDataService _settingsDataService;
        private SteamGuardWindow steamGuardWindow;
        private KeyboardListener keyboardListener;
        
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        [PreferredConstructor]
        public MainViewModel(IChatDataService dataService, IGeneralDataService generalDataService, ISettingsDataService settingsDataService)
        {            
            if( !IsWindowReopen )
            {
                _dataService = dataService;
                _generalDataService = generalDataService;
                _settingsDataService = settingsDataService;

                _settingsDataService.GetAppSettings((config) => {
                    AppConfig = config;
                });                
                steamGuardWindow = new SteamGuardWindow();
                Initialize();
            }
        }
        public void Initialize()
        {
            //watch Ctrl down to switch click-through
#if !DEBUG
            keyboardListener = new KeyboardListener();
            keyboardListener.KeyDown += keyboardListener_KeyDown;
            keyboardListener.KeyUp += keyboardListener_KeyUp;
#endif
            if( AppConfig.IsReplyBoxPersistent )
            {
                IsOverlayVisible = true;
                SendTextEditMode = true;
            }

            
            AppConfig.PropertyChanged += AppConfig_PropertyChanged;

            Win.ShowStatus();

            EnableMouseTransparency = AppConfig.MouseTransparency && Keyboard.Modifiers != ModifierKeys.Control;

            _generalDataService.Start();

            ChannelList = _dataService.ChatChannels;
            SelectedChatChannel = ChannelList[0];


            MessengerInstance.Register<bool>(this, "ReopenMainWindow", (message) => { 
                if( message )
                {
                    IsWindowReopen = true;
                }
            });

            MessengerInstance.Register<ChatMessage>(this, "SetChannel", (message) =>
            {
                SelectedChatChannel = ChannelList.FirstOrDefault(channel =>
                    channel.ChatName == message.ChatName &&
                    channel.ChannelName == message.Channel) ?? ChannelList[0];

            });

            var steamChat = _dataService.GetChat(SettingsRegistry.ChatTitleSteam);
            if( steamChat != null )
            {
                steamChat.RequestData = (what) => {
                    if (what.Equals("SteamGuardCode", StringComparison.InvariantCultureIgnoreCase))
                    {
                        UI.Dispatch( () =>
                        {                                
                                steamGuardWindow.Show();
                        });
                    }
                    return null;
                };
            }

        }

        void AppConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if( e.PropertyName.Equals(AppConfig.MouseTransparencyPropertyName) )
            {
                EnableMouseTransparency = AppConfig.MouseTransparency;
            }
            else if( e.PropertyName.Equals(AppConfig.IsReplyBoxPersistent ))
            {
                SwitchOverlay();
            }
        }

        void keyboardListener_KeyUp(object sender, RawKeyEventArgs args)
        {
            if (AppConfig.MouseTransparency && ( args.Key == Key.RightCtrl || args.Key == Key.LeftCtrl))
            {
                UI.Dispatch(() => EnableMouseTransparency = AppConfig.MouseTransparency);
            }

        }

        void keyboardListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            if (args.Key == Key.RightCtrl || args.Key == Key.LeftCtrl)
            {
                UI.Dispatch(() => EnableMouseTransparency = false);
            }
        }

        /// <summary>
        /// The <see cref="IsWindowReopen" /> property's name.
        /// </summary>
        public const string IsWindowReopenPropertyName = "IsWindowReopen";

        private bool _isWindowReopen = false;

        /// <summary>
        /// Sets and gets the IsWindowReopen property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsWindowReopen
        {
            get
            {
                return _isWindowReopen;
            }

            set
            {
                if (_isWindowReopen == value)
                {
                    return;
                }

                _isWindowReopen = value;
                RaisePropertyChanged(IsWindowReopenPropertyName);
            }
        }

        private RelayCommand _changeState;


        /// <summary>
        /// Gets the ChangeState.
        /// </summary>
        public RelayCommand ChangeState
        {
            get
            {
                return _changeState
                    ?? (_changeState = new RelayCommand(
                                          () =>
                                          {

                                              if( Application.Current.MainWindow.WindowState == WindowState.Normal ||
                                                  Application.Current.MainWindow.WindowState == WindowState.Minimized)
                                              {
                                                  Win.SetGlobalStatus(Application.Current.MainWindow.WindowState);
                                              }
                                          }));
            }
        }

        private RelayCommand _minimize;

        /// <summary>
        /// Gets the Minimize.
        /// </summary>
        public RelayCommand Minimize
        {
            get
            {
                return _minimize
                    ?? (_minimize = new RelayCommand(
                                          () =>
                                          {
                                              Application.Current.MainWindow.WindowState = WindowState.Minimized;
                                          }));
            }
        }

        private RelayCommand _showSettings;

        /// <summary>
        /// Gets the ShowSettings.
        /// </summary>
        public RelayCommand ShowSettings
        {
            get
            {
                return _showSettings
                    ?? (_showSettings = new RelayCommand(
                                          () =>
                                          {
                                              Win.ShowSettings();
                                          }));
            }
        }

        private RelayCommand _exitApplication;

        /// <summary>
        /// Gets the ExitApplication.
        /// </summary>
        public RelayCommand ExitApplication
        {
            get
            {
                return _exitApplication
                    ?? (_exitApplication = new RelayCommand(
                                          () =>
                                          {
                                              if( !IsWindowReopen )
                                              {
                                                  Properties.Ubiquitous.Default.Save();
                                                  _dataService.Stop();
                                                  
                                                  Application.Current.Shutdown();
                                              }
                                              IsWindowReopen = false;
                                          }));
            }
        }

        /// <summary>
        /// The <see cref="IsOverlayVisible" /> property's name.
        /// </summary>
        public const string IsOverlayVisiblePropertyName = "IsOverlayVisible";

        private bool _isOverlayVisible = false;

        /// <summary>
        /// Sets and gets the IsOverlayVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsOverlayVisible
        {
            get
            {
                return _isOverlayVisible;
            }

            set
            {
                if (_isOverlayVisible == value)
                {
                    return;
                }

                _isOverlayVisible = value;
                MessengerInstance.Send<bool>(!_isOverlayVisible, "EnableAutoScroll");
                RaisePropertyChanged(IsOverlayVisiblePropertyName);
            }
        }
        /// <summary>
        /// The <see cref="IsMouseOver" /> property's name.
        /// </summary>
        public const string IsMouseOverPropertyName = "IsMouseOver";

        private bool _isMouseOver = false;

        /// <summary>
        /// Sets and gets the IsMouseOver property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsMouseOver
        {
            get
            {
                return _isMouseOver;
            }

            set
            {
                if (_isMouseOver == value)
                {
                    return;
                }
                _isMouseOver = value;
                RaisePropertyChanged(IsMouseOverPropertyName);
            }
        }

        private RelayCommand _hideOverlay;

        /// <summary>
        /// Gets the HideOverlay.
        /// </summary>
        public RelayCommand HideOverlay
        {
            get
            {
                return _hideOverlay
                    ?? (_hideOverlay = new RelayCommand(
                                          () =>
                                          {
                                              if (AppConfig.IsReplyBoxPersistent)
                                                  return;

                                              IsMouseOver = false;
                                              SwitchOverlay();
                                          }));
            }
        }

        private RelayCommand _showOverlay;

        /// <summary>
        /// Gets the ShowOverlay.
        /// </summary>
        public RelayCommand ShowOverlay
        {
            get
            {
                return _showOverlay
                    ?? (_showOverlay = new RelayCommand(
                                          () =>
                                          {
                                                IsMouseOver = true;
                                                SwitchOverlay();
                                          }));
            }
        }


        /// <summary>
        /// The <see cref="SendText" /> property's name.
        /// </summary>
        public const string SendTextPropertyName = "SendText";

        private string _sendText;

        /// <summary>
        /// Sets and gets the SendText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SendText
        {
            get
            {
                return _sendText;
            }

            set
            {
                if (_sendText == value)
                {
                    return;
                }
                _sendText = value;
                RaisePropertyChanged(SendTextPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsFocused" /> property's name.
        /// </summary>
        public const string IsFocusedPropertyName = "IsFocused";

        private bool _isFocused = true;

        /// <summary>
        /// Sets and gets the IsFocused property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsFocused
        {
            get
            {
                return _isFocused;
            }

            set
            {
                if (_isFocused == value)
                {
                    return;
                }
                _isFocused = value;
                RaisePropertyChanged(IsFocusedPropertyName);
            }
        }

        private RelayCommand _setFocused;

        /// <summary>
        /// Gets the SetFocused.
        /// </summary>
        public RelayCommand SetFocused
        {
            get
            {
                return _setFocused
                    ?? (_setFocused = new RelayCommand(
                                          () =>
                                          {
                                              IsFocused = true;
                                              SwitchOverlay();
                                          }));
            }
        }

        private RelayCommand _setUnFocused;

        /// <summary>
        /// Gets the SetUnFocused.
        /// </summary>
        public RelayCommand SetUnFocused
        {
            get
            {
                return _setUnFocused
                    ?? (_setUnFocused = new RelayCommand(
                                          () =>
                                          {
                                              IsFocused = false;
                                              SwitchOverlay();
                                          }));
            }
        }

        /// <summary>
        /// The <see cref="ChannelList" /> property's name.
        /// </summary>
        public const string ChatListPropertyName = "ChannelList";

        private ObservableCollection<dynamic> _chatList = new ObservableCollection<dynamic>();

        /// <summary>
        /// Sets and gets the ChatList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<dynamic> ChannelList
        {
            get
            {
                return _chatList;
            }

            set
            {
                if (_chatList == value)
                {
                    return;
                }
                _chatList = value;
                RaisePropertyChanged(ChatListPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectedChatChannel" /> property's name.
        /// </summary>
        public const string SelectedChatChannelPropertyName = "SelectedChatChannel";

        private dynamic _selectedChat;// = new { ChatName = "Ubiquitous 2.0", ChannelName = "#ubiquitous", ChatIconURL = @"/Ubiquitous2;component/Resources/ubiquitous smile.ico" };

        /// <summary>
        /// Sets and gets the SelectedChatChannel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public dynamic SelectedChatChannel
        {
            get
            {
                return _selectedChat;
            }

            set
            {
                if (value == null)
                    return;

                if ( _selectedChat != null &&
                    _selectedChat.ChatName == value.ChatName &&
                    _selectedChat.ChannelName == value.ChannelName &&
                    _selectedChat.ChatIconURL == value.ChatIconURL)
                {
                    return;
                }
                SelectedChannelName = value.ChannelName;
                _selectedChat = value;
                RaisePropertyChanged(SelectedChatChannelPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectedChannelName" /> property's name.
        /// </summary>
        public const string SelectedChannelNamePropertyName = "SelectedChannelName";

        private string _selectedChannelName = "#xedoc";

        /// <summary>
        /// Sets and gets the SelectedChannelName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectedChannelName
        {
            get
            {
                return _selectedChannelName;
            }

            set
            {
                if (_selectedChannelName == value)
                {
                    return;
                }
                _selectedChannelName = value;
                RaisePropertyChanged(SelectedChannelNamePropertyName);
            }
        }
        private void SwitchOverlay()
        {
            if ((IsMouseOver && IsFocused) || AppConfig.IsReplyBoxPersistent)
            {
                IsOverlayVisible = true;
                SendTextEditMode = true;
            }
            else if( !AppConfig.IsReplyBoxPersistent && !IsMouseOver && !IsFocused)
            {
                IsOverlayVisible = false;
                SendTextEditMode = false;
            }
            if( AppConfig.IsReplyBoxPersistent )
                MessengerInstance.Send<bool>(!(IsMouseOver && IsFocused), "EnableAutoScroll");
        }

        private RelayCommand _enterCommand;

        /// <summary>
        /// Gets the EnterCommand.
        /// </summary>
        public RelayCommand EnterCommand
        {
            get
            {
                return _enterCommand
                    ?? (_enterCommand = new RelayCommand(
                                          () =>
                                          {
                                              if (SelectedChatChannel == null || String.IsNullOrEmpty(SendText) )
                                                  return;
                                              
                                              _dataService.SendMessage(new ChatMessage() { 
                                                Channel = SelectedChatChannel.ChannelName,
                                                ChatName = SelectedChatChannel.ChatName,
                                                IsSentByMe = true,
                                                HighlyImportant = true,
                                                Text = SendText
                                              });
                                              SendText = String.Empty;
                                              ScrollToLastMessage();
                                          }));
            }
        }
        private void ScrollToLastMessage()
        {
            var delaySend = new Timer((obj) =>
            {
                UI.Dispatch(() => MessengerInstance.Send<bool>(false, "MessageSent"));
                UI.Dispatch(() => MessengerInstance.Send<bool>(true, "MessageSent"));
            }, this, 100, Timeout.Infinite);

        }

        /// <summary>
        /// The <see cref="SendTextEditMode" /> property's name.
        /// </summary>
        public const string SendTextEditModePropertyName = "SendTextEditMode";

        private bool _sendTextEditMode = false;

        /// <summary>
        /// Sets and gets the SendTextEditMode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool SendTextEditMode
        {
            get
            {
                return _sendTextEditMode;
            }

            set
            {
                if (_sendTextEditMode == value)
                {
                    return;
                }
                _sendTextEditMode = value;
                RaisePropertyChanged(SendTextEditModePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="EnableMouseTransparency" /> property's name.
        /// </summary>
        public const string EnableMouseTransparencyPropertyName = "EnableMouseTransparency";

        private bool _enableMouseTransparency = false;

        /// <summary>
        /// Sets and gets the EnableMouseTransparency property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool EnableMouseTransparency
        {
            get
            {
                return _enableMouseTransparency;
            }

            set
            {
                if (_enableMouseTransparency == value)
                {
                    return;
                }

                _enableMouseTransparency = value;
                RaisePropertyChanged(EnableMouseTransparencyPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="AppConfig" /> property's name.
        /// </summary>
        public const string AppConfigPropertyName = "AppConfig";

        private AppConfig _appConfig = new AppConfig();

        /// <summary>
        /// Sets and gets the AppConfig property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public AppConfig AppConfig
        {
            get
            {
                return _appConfig;
            }

            set
            {
                if (_appConfig == value)
                {
                    return;
                }
                _appConfig = value;
                RaisePropertyChanged(AppConfigPropertyName);
            }
        }
        private RelayCommand _showDashboard;

        /// <summary>
        /// Gets the ShowDashboard.
        /// </summary>
        public RelayCommand ShowDashboard
        {
            get
            {
                return _showDashboard
                    ?? (_showDashboard = new RelayCommand(
                                          () =>
                                          {
                                              Win.ShowDashBoard();
                                          }));
            }
        }
        public override void Cleanup()
        {
            // Clean up if needed
#if !DEBUG
            keyboardListener.Dispose();
#endif
            MessengerInstance.Unregister<bool>(this, "ReopenMainWindow");
            MessengerInstance.Unregister<ChatMessage>(this, "SetChannel");

            base.Cleanup();
        }


    }
}