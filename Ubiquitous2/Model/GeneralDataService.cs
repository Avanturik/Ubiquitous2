using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;

namespace UB.Model
{
    public class GeneralDataService : IGeneralDataService
    {        
        private SettingsDataService settingsDataService { get; set; }
        

        public GeneralDataService()
        {
            settingsDataService = ServiceLocator.Current.GetInstance<SettingsDataService>();
            Start();
        }

        public List<IService> Services
        {
            get;
            set;
        }


        public void Start()
        {
            if (Services == null)
            {
                settingsDataService.GetServiceSettings((configs) =>
                {
                    Services = configs.Select(config => SettingsRegistry.ServiceFactory[config.ServiceName](config)).ToList();
                });

                if( Services != null )
                {
                    foreach( var service in Services )
                    {
                        if( service.Config.Enabled )
                        {
                            Task.Factory.StartNew(() => service.Start());
                        }
                    }
                }
            }
        }

        public void Stop()
        {
           
        }
    }
}
