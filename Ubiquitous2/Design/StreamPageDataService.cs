using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Model;

namespace UB.Design
{
    public class StreamPageDataService : IStreamPageDataService
    {
        public void GetStreamTopics(Action<List<IStreamTopic>> callback)
        {
            
        }

        public void GetPresets(Action<List<StreamInfoPreset>> callback)
        {
           
        }

        public void LoadTopicsFromWeb()
        {
            
        }

        public void UpdateTopicsOnWeb()
        {
            
        }

        public void RemovePreset(StreamInfoPreset preset)
        {
           
        }

        public StreamInfoPreset AddPreset(string presetName)
        {
            return new StreamInfoPreset();
        }
    }
}
