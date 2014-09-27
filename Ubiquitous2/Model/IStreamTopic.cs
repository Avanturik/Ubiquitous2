using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace UB.Model
{
    interface IStreamTopic
    {

        [XmlElement]
        Game CurrentGame
        {
            get;
            set;
        }
        [XmlAttribute]
        string Topic
        {
            get;
            set;
        }
        [XmlAttribute]
        string Description
        {
            get;
            set;
        }

        [XmlIgnore]
        ObservableCollection<Game> Games
        {
            get;
            set;
        }

        [XmlIgnore]
        string SearchQuery
        {
            get;
            set;
        }
        void QueryGameList(string gameName);
        void GetTopic();
        void SetTopic();
    }
}
