using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public interface IGreetingsDataService
    {
        void GetGreetings(Action<List<Greeting>> callback);
    }
}
