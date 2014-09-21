using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Model;

namespace UB.Design
{
    class GeneralDataServiceDesign : IGeneralDataService
    {
        public GeneralDataServiceDesign()
        {
            Services = new List<IService>();
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
    }
}
