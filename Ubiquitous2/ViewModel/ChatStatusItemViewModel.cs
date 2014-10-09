using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devart.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using UB.Model;

namespace UB.ViewModel
{
    public class ChatStatusItemViewModel : ViewModelBase
    {
        [PreferredConstructor]
        public ChatStatusItemViewModel()
        {

        }

        /// <summary>
        /// The <see cref="Chat" /> property's name.
        /// </summary>
        public const string ChatPropertyName = "Chat";

        private IChat _chat = null;

        /// <summary>
        /// Sets and gets the Chat property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public IChat Chat
        {
            get
            {
                return _chat;
            }

            set
            {
                if (_chat == value)
                {
                    return;
                }

                _chat = value;
                RaisePropertyChanged(ChatPropertyName);
            }
        }

    }
}
