using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UB.Model
{
    public class StreamInfo :NotifyPropertyChangeBase
    {

        public StreamInfo()
        {
            CurrentGame = new Game();
        }

        /// <summary>
        /// The <see cref="CurrentGame" /> property's name.
        /// </summary>
        [XmlElement]
        public const string CurrentGamePropertyName = "CurrentGame";

        private Game _currentGame = new Game();

        /// <summary>
        /// Sets and gets the CurrentGame property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Game CurrentGame
        {
            get
            {
                return _currentGame;
            }

            set
            {
                if (_currentGame == value)
                {
                    return;
                }

                RaisePropertyChanging(CurrentGamePropertyName);
                _currentGame = value;
                RaisePropertyChanged(CurrentGamePropertyName);
            }
        }


        /// <summary>
        /// The <see cref="Topic" /> property's name.
        /// </summary>
        public const string TopicPropertyName = "Topic";

        private string _topic = null;

        /// <summary>
        /// Sets and gets the Topic property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public string Topic
        {
            get
            {
                return _topic;
            }

            set
            {
                if (_topic == value)
                {
                    return;
                }

                RaisePropertyChanging(TopicPropertyName);
                _topic = value;
                RaisePropertyChanged(TopicPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Description" /> property's name.
        /// </summary>
        public const string DescriptionPropertyName = "Description";

        private string _description = null;

        /// <summary>
        /// Sets and gets the Description property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public string Description
        {
            get
            {
                return _description;
            }

            set
            {
                if (_description == value)
                {
                    return;
                }

                RaisePropertyChanging(DescriptionPropertyName);
                _description = value;
                RaisePropertyChanged(DescriptionPropertyName);
            }
        }
        
    }
}
