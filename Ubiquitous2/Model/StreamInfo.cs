using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UB.Model
{
    [Serializable]
    public class StreamInfo :NotifyPropertyChangeBase
    {

        public StreamInfo()
        {
            CurrentGame = new Game();
        }

        public StreamInfo GetCopy()
        {
            var result = new StreamInfo() { 
                CanBeChanged = this.CanBeChanged,
                CanBeRead = this.CanBeRead,
                ChatName = this.ChatName,
                CurrentGame = new Game()
                {
                    Id = this.CurrentGame.Id,
                    Name = this.CurrentGame.Name,
                },
                Description = this.Description,
                HasDescription = this.HasDescription,
                HasGame = this.HasGame,
                HasTopic = this.HasTopic,
                Topic = this.Topic

            };
            return result;
        }
        /// <summary>
        /// The <see cref="Language" /> property's name.
        /// </summary>
        public const string LanguagePropertyName = "Language";

        private string _language = String.Empty;

        /// <summary>
        /// Sets and gets the Language property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Language
        {
            get
            {
                return _language;
            }

            set
            {
                if (_language == value)
                {
                    return;
                }

                _language = value;
                RaisePropertyChanged(LanguagePropertyName);
            }
        }
        /// <summary>
        /// The <see cref="ChatName" /> property's name.
        /// </summary>
        public const string ChatNamePropertyName = "ChatName";

        private string _chatName = null;

        /// <summary>
        /// Sets and gets the ChatName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public string ChatName
        {
            get
            {
                return _chatName;
            }

            set
            {
                if (_chatName == value)
                {
                    return;
                }

                RaisePropertyChanging(ChatNamePropertyName);
                _chatName = value;
                RaisePropertyChanged(ChatNamePropertyName);
            }
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


        /// <summary>
        /// The <see cref="CanBeChanged" /> property's name.
        /// </summary>
        public const string CanBeChangedPropertyName = "CanBeChanged";

        private bool _canBeChanged = false;

        /// <summary>
        /// Sets and gets the CanBeChanged property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
        public bool CanBeChanged
        {
            get
            {
                return _canBeChanged;
            }

            set
            {
                if (_canBeChanged == value)
                {
                    return;
                }

                RaisePropertyChanging(CanBeChangedPropertyName);
                _canBeChanged = value;
                RaisePropertyChanged(CanBeChangedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="CanBeRead" /> property's name.
        /// </summary>
        public const string CanBeReadPropertyName = "CanBeRead";

        private bool _canBeRead = false;

        /// <summary>
        /// Sets and gets the CanBeRead property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
        public bool CanBeRead
        {
            get
            {
                return _canBeRead;
            }

            set
            {
                if (_canBeRead == value)
                {
                    return;
                }

                RaisePropertyChanging(CanBeReadPropertyName);
                _canBeRead = value;
                RaisePropertyChanged(CanBeReadPropertyName);
            }
        }
        
    }
}
