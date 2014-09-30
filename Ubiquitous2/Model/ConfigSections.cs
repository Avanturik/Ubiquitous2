using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UB.Model
{
    [Serializable]
    public class ConfigSections
    {
        [XmlArray]
        public List<StreamInfoPreset> StreamInfoPresets { get; set; }
        [XmlArray]
        public List<ChatConfig> ChatConfigs { get; set; }
        [XmlArray]
        public List<ServiceConfig> ServiceConfigs { get; set; }
        [XmlArray]
        public List<WindowSettings> WindowSettings { get; set; }
        [XmlElement]
        public AppConfig AppConfig { get; set; }
    }
}
