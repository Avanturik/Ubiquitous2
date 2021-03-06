﻿/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:ChatControlTest2.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ServiceLocation;
using UB.Model;

namespace UB.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            DispatcherHelper.Initialize();


            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<ISettingsDataService, Design.DesignSettingsDataService>();
                SimpleIoc.Default.Register<IChatDataService, Design.DesignDataService>();
                SimpleIoc.Default.Register<IImageDataSource, Design.DesignImageCacheDataService>();
                SimpleIoc.Default.Register<IGeneralDataService, Design.GeneralDataServiceDesign>();
                SimpleIoc.Default.Register<IDatabase, Design.DatabaseService>();
                SimpleIoc.Default.Register<IStreamPageDataService, Design.StreamPageDataService>();
                SimpleIoc.Default.Register<IGreetingsDataService, Design.GreetingsDataService>();
            }
            else
            {
                SimpleIoc.Default.Register<ISettingsDataService, SettingsDataService>();
                SimpleIoc.Default.Register<IChatDataService, ChatDataService>();
                SimpleIoc.Default.Register<IImageDataSource, ImageCacheDataService>();
                SimpleIoc.Default.Register<IGeneralDataService, GeneralDataService>();
                SimpleIoc.Default.Register<IDatabase, DatabaseService>();
                SimpleIoc.Default.Register<IStreamPageDataService, StreamPageDataService>();
                SimpleIoc.Default.Register<IGreetingsDataService, GreetingsDataService>();
            }
            SimpleIoc.Default.Register<UserListViewModel>();
            SimpleIoc.Default.Register<SubFollowDonationViewModel>();
            SimpleIoc.Default.Register<StreamTopicSectionViewModel>();
            SimpleIoc.Default.Register<EditBoxViewModel>();
            SimpleIoc.Default.Register<DashBoardViewModel>();
            SimpleIoc.Default.Register<HelloPlaceHolderViewModel>();
            SimpleIoc.Default.Register<YesNoDialogViewModel>();
            SimpleIoc.Default.Register<SteamGuardViewModel>();
            //SimpleIoc.Default.Register<LastFMService>();
            SimpleIoc.Default.Register<MusicTickerViewModel>();
            SimpleIoc.Default.Register<StatusViewModel>();
            SimpleIoc.Default.Register<SettingsFieldViewModel>();
            SimpleIoc.Default.Register<SettingsSectionViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<ChatMessageViewModel>();
            SimpleIoc.Default.Register<ChatBoxViewModel>();
            SimpleIoc.Default.Register<MainViewModel>();
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }


        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public SettingsViewModel Settings
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingsViewModel>();
            }
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ChatBoxViewModel ChatBox
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ChatBoxViewModel>();
            }
        }


        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ChatMessageViewModel ChatMessage
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ChatMessageViewModel>();
            }
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public SettingsSectionViewModel SettingsSection
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingsSectionViewModel>();
            }
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public SettingsFieldViewModel SettingsField
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingsFieldViewModel>();
            }
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public StatusViewModel Status
        {
            get
            {
                return ServiceLocator.Current.GetInstance<StatusViewModel>();
            }
        }
        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
    "CA1822:MarkMembersAsStatic",
    Justification = "This non-static member is needed for data binding purposes.")]
        public MusicTickerViewModel MusicTicker
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MusicTickerViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
    "CA1822:MarkMembersAsStatic",
    Justification = "This non-static member is needed for data binding purposes.")]
        public SteamGuardViewModel SteamGuard
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SteamGuardViewModel>();
            }
        }

                [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
    "CA1822:MarkMembersAsStatic",
    Justification = "This non-static member is needed for data binding purposes.")]
        public YesNoDialogViewModel YesNoDialog
        {
            get
            {
                return ServiceLocator.Current.GetInstance<YesNoDialogViewModel>();
            }
        }
       

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",    "CA1822:MarkMembersAsStatic",    Justification = "This non-static member is needed for data binding purposes.")]
        public HelloPlaceHolderViewModel HelloPlaceHolder
        {
            get
            {
                return ServiceLocator.Current.GetInstance<HelloPlaceHolderViewModel>();
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
        public DashBoardViewModel DashBoard
        {
            get
            {
                return ServiceLocator.Current.GetInstance<DashBoardViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1822:MarkMembersAsStatic",Justification = "This non-static member is needed for data binding purposes.")]
        public EditBoxViewModel EditBox
        {
            get
            {
                return ServiceLocator.Current.GetInstance<EditBoxViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
        public StreamTopicSectionViewModel StreamTopicSection
        {
            get
            {
                return ServiceLocator.Current.GetInstance<StreamTopicSectionViewModel>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
        public SubFollowDonationViewModel SubFollowDonation
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SubFollowDonationViewModel>();
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This non-static member is needed for data binding purposes.")]
        public UserListViewModel UserList
        {
            get
            {
                return ServiceLocator.Current.GetInstance<UserListViewModel>();
            }
        }
       
    }
}