using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;

namespace UB.Model
{
    public class ChatManager
    {
        private SettingsDataService settingsDataService;

        // TODO To implement scripting need to register new chats somehow
        private Dictionary<String, Func<ChatConfig,IChat>> chatFactory = new Dictionary<String, Func<ChatConfig, IChat>>()
        {
            {"Twitch.tv", (config)=>{ return new TwitchChat(config); }}
        };
        public ChatManager()
        {
            settingsDataService = ServiceLocator.Current.GetInstance<SettingsDataService>();
        }
        public List<IChat> Chats {
            get
            {
                var rnd = new Random();
                var chats = new List<IChat>();
                settingsDataService.GetChatSettings((configs) =>
                {
                    foreach (ChatConfig config in configs)
                    {
                        chats.Add(chatFactory[config.ChatName](config));
                    }
                });
                return chats;
            }

        }

    }
}
