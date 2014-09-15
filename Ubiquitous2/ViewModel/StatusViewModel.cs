﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devart.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using UB.Model;

namespace UB.ViewModel
{
    public class StatusViewModel : ViewModelBase
    {
        private IChatDataService _dataService;
        
        [PreferredConstructor]
        public StatusViewModel( IChatDataService dataService)
        {
            _dataService = dataService;
            Initialize();
        }

        private void Initialize()
        {
            if (Chats == null)
                Chats = new ObservableCollection<IChat>();

            _dataService.ChatStatusHandler = (chat) => {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => {
                        if (chat.Enabled == true)
                        {
                            Chats.Add(chat);
                        }
                        else
                        { 
                            var removeItem = Chats.FirstOrDefault( item => item.ChatName == chat.ChatName);
                            if (removeItem != null)
                                Chats.Remove(removeItem);
                        }
                    });
            };
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

                RaisePropertyChanging(ChatsPropertyName);
                _chats = value;
                RaisePropertyChanged(ChatsPropertyName);
            }
        }
    }
}