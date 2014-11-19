using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Ioc;
using UB.Properties;
namespace UB.Model
{
    public class StreamPageDataService : IStreamPageDataService
    {
        private List<StreamInfoPreset> presets;
        private IChatDataService chatDataService;
        public StreamPageDataService()
        {
            Initialize();
        }
        private void Initialize()
        {
            chatDataService = SimpleIoc.Default.GetInstance<IChatDataService>();

            presets = Ubiquitous.Default.Config.StreamInfoPresets;
            if (presets == null)
                presets = new List<StreamInfoPreset>();
        }
        public StreamInfoPreset AddPreset( string presetName )
        {           
            var newPreset = new StreamInfoPreset() { PresetName = presetName };
            newPreset.StreamTopics = new List<StreamInfo>();

            GetStreamTopics((streams) => {
                streams.ForEach(stream => newPreset.StreamTopics.Add(stream.Info.GetCopy()));
            });

            presets.Add(newPreset);

            return presets.Last();
        }
        public void GetPresets(Action<List<StreamInfoPreset>> callback)
        {
            callback(presets);
        }
        public void LoadTopicsFromWeb()
        {
            GetStreamTopics((streams) => streams.ForEach(stream => stream.GetTopic()));
        }       
        public void GetStreamTopics(Action<List<IStreamTopic>> callback)
        {
            var streamTopics = chatDataService.Chats.Where(chat => chat is IStreamTopic).Select( chat => 
            {
                var topic = chat as IStreamTopic;
                topic.Info.CanBeChanged = chat.Status.IsLoggedIn;
                topic.Info.CanBeRead = chat.Status.IsConnected;
                return topic;
            }).ToList();

            if (streamTopics == null)
                callback ( new List<IStreamTopic>());
            else
                callback(streamTopics);
        }
        public void UpdateTopicsOnWeb()
        {
            GetStreamTopics((streams) => streams.Where(stream => (stream as IChat).Enabled).ToList().ForEach(stream => Task.Factory.StartNew(()=> stream.SetTopic())));
        }
        public void RemovePreset(StreamInfoPreset preset)
        {
            presets.Remove(preset);
        }
    }
}
