using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public interface ISettingsDataService
    {
        void GetChatSettings(Action<List<ChatConfig>> callback);
        void GetServiceSettings(Action<List<ServiceConfig>> callback);
        void GetRandomChatSetting(Action<ChatConfig> callback);
        void GetRandomTextField(Action<ConfigField> callback);
    }
}
