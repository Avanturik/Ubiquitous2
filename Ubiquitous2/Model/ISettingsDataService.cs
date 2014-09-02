using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public interface ISettingsDataService
    {
        void GetSettings(String section, Action<List<object>> callback);
    }
}
