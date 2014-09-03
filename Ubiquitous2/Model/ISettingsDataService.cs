using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public interface ISettingsDataService
    {
        void GetChatSettings(Action<ChatConfig[]> callback);
        void GetRandomChatSetting(Action<ChatConfig> callback);
        void GetRandomTextField(Action<ConfigField> callback);
    }
}
