using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace UB.Model
{
    public interface IStreamTopic
    {

        [XmlElement]
        StreamInfo Info
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
        void QueryGameList(string gameName, Action callback);
        void GetTopic();
        void SetTopic();

        Action StreamTopicAcquired { get; set; }
    }
}
