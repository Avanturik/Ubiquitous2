using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class GreetingsDataService : IGreetingsDataService
    {
        public void GetGreetings(Action<List<Greeting>> callback)
        {
            if( callback != null )
            {
                callback(new List<Greeting>()
                {
                    new Greeting("Thank you for following me!", "loremipsumuser"),
                    //new Greeting("Thank you for following me!", "loremipsumuser2")
                });
            }
        }
    }
}
