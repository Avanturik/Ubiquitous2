using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class SettingsDataService : ISettingsDataService
    {
        public void GetSettings(String section, Action<List<dynamic>> callback)
        {
            switch( section.ToLower() )
            {
                case "chats":
                    callback(new List<dynamic>() { 
                        new { ChatName = "Twitch.tv", Enabled = true, UserName = "justinfan12341234", Password = "123"}
                    });
                    break;                    
            }
        }
    }
}
