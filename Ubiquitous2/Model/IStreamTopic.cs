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
        StreamInfo Info
        {
            get;
            set;
        }

        ObservableCollection<Game> Games
        {
            get;
            set;
        }

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
