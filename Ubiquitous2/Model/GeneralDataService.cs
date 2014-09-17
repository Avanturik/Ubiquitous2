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
        private List<IService> services { get; set; }
        private SettingsDataService settingsDataService { get; set; }
        

        public GeneralDataService()
        {
            settingsDataService = ServiceLocator.Current.GetInstance<SettingsDataService>();
        }

        public List<IService> Services
        {
            set
            {
                if (services != value)
                    services = value;
            }
            get
            {
                if (services == null)
                {
                    services = new List<IService>();
                    settingsDataService.GetServiceSettings((configs) =>
                    {
                       services = configs.Select(config => SettingsRegistry.ServiceFactory[config.ServiceName](config)).ToList();
                    });
                }
                return services;
            }
        }
    }
}
