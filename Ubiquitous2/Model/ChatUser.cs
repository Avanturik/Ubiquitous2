using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class ChatUser:NotifyPropertyChangeBase
    {
        public ChatUser()
        {
            Badges = new ObservableCollection<UserBadge>();
            Badges.CollectionChanged += (o,e) => {
                BadgesCount = Badges.Count;
            };
        }
        public string NickName { get; set; }
        public string Channel { get; set; }
        public string ChatName { get; set; }
        public string GroupName { get; set; }

        /// <summary>
        /// The <see cref="Badges" /> property's name.
        /// </summary>
        public const string BadgesPropertyName = "Badges";

        private ObservableCollection<UserBadge> _badges = new ObservableCollection<UserBadge>();

        /// <summary>
        /// Sets and gets the Badges property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<UserBadge> Badges
        {
            get
            {
                return _badges;
            }

            set
            {
                if (_badges == value)
                {
                    return;
                }

                _badges = value;
                RaisePropertyChanged(BadgesPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="BadgesCount" /> property's name.
        /// </summary>
        public const string BadgesCountPropertyName = "BadgesCount";

        private int _badgesCount = 0;

        /// <summary>
        /// Sets and gets the BadgesCount property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int BadgesCount
        {
            get
            {
                return _badgesCount;
            }

            set
            {
                if (_badgesCount == value)
                {
                    return;
                }

                _badgesCount = value;
                RaisePropertyChanged(BadgesCountPropertyName);
            }
        }
    }
}
