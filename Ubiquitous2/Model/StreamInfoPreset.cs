using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UB.Model
{
    [Serializable]
    public class StreamInfoPreset : NotifyPropertyChangeBase
    {
        public StreamInfoPreset()
        {

        }
        /// <summary>
        /// The <see cref="StreamTopics" /> property's name.
        /// </summary>
        public const string StreamTopicsPropertyName = "StreamTopics";

        private List<StreamInfo> _streamTopics = new List<StreamInfo>();

        /// <summary>
        /// Sets and gets the StreamTopics property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlElement]
        public List<StreamInfo> StreamTopics
        {
            get
            {
                return _streamTopics;
            }

            set
            {
                if (_streamTopics == value)
                {
                    return;
                }

                RaisePropertyChanging(StreamTopicsPropertyName);
                _streamTopics = value;
                RaisePropertyChanged(StreamTopicsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="PresetName" /> property's name.
        /// </summary>
        public const string PresetNamePropertyName = "PresetName";

        private string _presetName = "Default";

        /// <summary>
        /// Sets and gets the PresetName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlAttribute]
        public string PresetName
        {
            get
            {
                return _presetName;
            }

            set
            {
                if (_presetName == value)
                {
                    return;
                }

                RaisePropertyChanging(PresetNamePropertyName);
                _presetName = value;
                RaisePropertyChanged(PresetNamePropertyName);
            }
        }
    }
}
