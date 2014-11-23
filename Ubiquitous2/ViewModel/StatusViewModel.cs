using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Devart.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using UB.Model;
using UB.Properties;
using UB.Utils;

namespace UB.ViewModel
{
    public class StatusViewModel : ViewModelBase
    {
        private IChatDataService _dataService;
        private IGeneralDataService _generalDataService;
        private IService imageService;

        [PreferredConstructor]
        public StatusViewModel(IChatDataService dataService, IGeneralDataService generalDataService)
        {

            _dataService = dataService;
            _generalDataService = generalDataService;
            Initialize();
        }

        private void Initialize()
        {
            if (Chats == null)
                Chats = new ObservableCollection<IChat>();

            imageService = _generalDataService.GetService(SettingsRegistry.ServiceTitleImageSaver);
            ImageServiceConfig = imageService.Config;
            StatusToImagePath = ImageServiceConfig.GetParameterValue("FilenameStatus") as string;

            ChatsView = CollectionViewSource.GetDefaultView(Chats) as ListCollectionView;
            ChatsView.CustomSort = new SortViewerCount();
            _dataService.ChatStatusHandler = (chat) =>
            {
                    UI.Dispatch(() => {
                        Chats.RemoveAll(item => item.ChatName == chat.ChatName);
                        if (chat.Enabled == true)
                        {
                            Chats.Add(chat);
                            chat.Status.PropertyChanged += (o, e) =>
                            {
                                if( ImageServiceConfig.Enabled )
                                {
                                    IsNeedSave = true;
                                    IsNeedSave = false;
                                }
                            };
                        }
                        if( ChatsView.NeedsRefresh )
                            ChatsView.Refresh();
                        
                    });
            };
            AppConfig = Ubiquitous.Default.Config.AppConfig;

        }

        /// <summary>
        /// The <see cref="AppConfig" /> property's name.
        /// </summary>
        public const string AppConfigPropertyName = "AppConfig";

        private AppConfig _appConfig = null;

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


        public ListCollectionView ChatsView { get; set; }


        /// <summary>
        /// The <see cref="ImageServiceConfig" /> property's name.
        /// </summary>
        public const string ImageServiceConfigPropertyName = "ImageServiceConfig";

        private ServiceConfig _imageServiceConfig = new ServiceConfig();

        /// <summary>
        /// Sets and gets the ImageServiceConfig property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ServiceConfig ImageServiceConfig
        {
            get
            {
                return _imageServiceConfig;
            }

            set
            {
                if (_imageServiceConfig == value)
                {
                    return;
                }
                _imageServiceConfig = value;
                RaisePropertyChanged(ImageServiceConfigPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsNeedSave" /> property's name.
        /// </summary>
        public const string IsNeedSavePropertyName = "IsNeedSave";

        private bool _isNeedSave = false;

        /// <summary>
        /// Sets and gets the IsNeedSave property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsNeedSave
        {
            get
            {
                return _isNeedSave;
            }

            set
            {
                if (_isNeedSave == value)
                {
                    return;
                }
                _isNeedSave = value;
                RaisePropertyChanged(IsNeedSavePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="StatusToImagePath" /> property's name.
        /// </summary>
        public const string StatusToImagePathPropertyName = "StatusToImagePath";

        private string _statusToImagePath = null;

        /// <summary>
        /// Sets and gets the StatusToImagePath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string StatusToImagePath
        {
            get
            {
                return _statusToImagePath;
            }

            set
            {
                if (_statusToImagePath == value)
                {
                    return;
                }

                _statusToImagePath = value;
                RaisePropertyChanged(StatusToImagePathPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Chats" /> property's name.
        /// </summary>
        public const string ChatsPropertyName = "Chats";

        private ObservableCollection<IChat> _chats = new ObservableCollection<IChat>();

        /// <summary>
        /// Sets and gets the Chats property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<IChat> Chats
        {
            get
            {
                return _chats;
            }

            set
            {
                if (_chats == value)
                {
                    return;
                }
                _chats = value;
                RaisePropertyChanged(ChatsPropertyName);
            }
        }
    }

    public class SortViewerCount : IComparer
    {
        public int Compare( object x, object y )
        {
            IChat chatX = x as IChat;
            IChat chatY = y as IChat;
            return chatX.Status.ViewersCount < chatY.Status.ViewersCount ? 1 : -1;
        }
    }
}
