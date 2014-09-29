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
        /// The <see cref="HasGame" /> property's name.
        /// </summary>
        public const string HasGamePropertyName = "HasGame";

        private bool _hasGame = true;

        /// <summary>
        /// Sets and gets the HasGame property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public bool HasGame
        {
            get
            {
                return _hasGame;
            }

            set
            {
                if (_hasGame == value)
                {
                    return;
                }

                RaisePropertyChanging(HasGamePropertyName);
                _hasGame = value;
                RaisePropertyChanged(HasGamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="HasTopic" /> property's name.
        /// </summary>
        public const string HasTopicPropertyName = "HasTopic";

        private bool _hasTopic = true;

        /// <summary>
        /// Sets and gets the HasTopic property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public bool HasTopic
        {
            get
            {
                return _hasTopic;
            }

            set
            {
                if (_hasTopic == value)
                {
                    return;
                }

                RaisePropertyChanging(HasTopicPropertyName);
                _hasTopic = value;
                RaisePropertyChanged(HasTopicPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="HasDescription" /> property's name.
        /// </summary>
        public const string HasDescriptionPropertyName = "HasDescription";

        private bool _hasDescription = false;

        /// <summary>
        /// Sets and gets the HasDescription property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public bool HasDescription
        {
            get
            {
                return _hasDescription;
            }

            set
            {
                if (_hasDescription == value)
                {
                    return;
                }

                RaisePropertyChanging(HasDescriptionPropertyName);
                _hasDescription = value;
                RaisePropertyChanged(HasDescriptionPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="CurrentGame" /> property's name.
        /// </summary>
        public const string CurrentGamePropertyName = "CurrentGame";

        private Game _currentGame = new Game();

        /// <summary>
        /// Sets and gets the CurrentGame property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlElement]
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
        [XmlElement]
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
        [XmlElement]
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
