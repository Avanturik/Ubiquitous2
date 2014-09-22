using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UB.Model;
using UB.Properties;

namespace UB.Design
{
    public class GeneralDataServiceDesign : IGeneralDataService
    {
        public GeneralDataServiceDesign()
        {
            Services = new List<IService>() {
                new LastFMServiceDesign( new ServiceConfig() { 
                    ServiceName = SettingsRegistry.ServiceTitleMusicTicker,
                    Enabled = true,
                })
            };

        }
        public List<IService> Services
        {
            get;
            set;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }


        public IService GetService(string serviceName)
        {
            return null;
        }
    }

    public class LastFMServiceDesign : IService
    {
        public LastFMServiceDesign( ServiceConfig config)
        {
            Config = config;
        }
        public bool Start()
        {
            return true;
        }

        public bool Stop()
        {
            return false;
        }

        public void Restart()
        {
            
        }

        public Action AfterStart
        {
            get;
            set;

        }

        public ServiceConfig Config
        {
            get;
            set;
        }

        public StatusBase Status
        {
            get;
            set;

        }

        public void GetData(Action<object> callback)
        {
            callback( new MusicTrackInfo()
            {
                Album = "Lorem ipsum",
                Artist = "Dolor sit amet",
                Title = "Consectetur adipiscing elit, sed do eiusmod tempor",
                ImageURL = Icons.MainHeadsetIcon,
            });
        }

    }
}
