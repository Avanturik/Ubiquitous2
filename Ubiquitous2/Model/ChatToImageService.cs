using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Model;

namespace UB.Model
{
    public class ChatToImageService : NotifyPropertyChangeBase, IService
    {
        public ChatToImageService(ServiceConfig config)
        {
            Config = config;
            Status = new StatusBase();
        }

        public bool Start()
        {
            return true;
        }

        public bool Stop()
        {
            return true;
        }

        public void Restart()
        {
            Stop();
            Start();
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

        }
    }
}
