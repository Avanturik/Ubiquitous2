using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public interface IService
    {
        bool Start();
        bool Stop();
        void Restart();
        Action AfterStart { get; set; }
        ServiceConfig Config { get; set; }
        StatusBase Status { get; set; }
        void GetData(Action<object> callback);
    }
}
