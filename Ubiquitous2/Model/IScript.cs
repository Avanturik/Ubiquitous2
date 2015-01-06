using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public interface IScript
    {
        object OnObjectRequest(object config);
        object OnConfigRequest();
    }
}
