using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Model;

namespace UB.Design
{
    public class DatabaseService : IDatabase
    {

        public void GetViewersCount(int intervalMinutes, Action<List<StatisticsViewers>> callback)
        {
            var startDate = new DateTime(2014, 10, 1, 12, 0, 0);
            var rand = new Random();
            var result = new List<StatisticsViewers>();
            for (int i = 1; i < 1440; i++)
            {
                var datetime = startDate.AddMinutes(i);
                result.Add(
                    new StatisticsViewers()
                    {
                        DateTime = datetime,
                        ServiceId = 1,
                        Viewerscount = rand.Next(1, 50),
                    });
            }
            callback(result);
        }
    }
}
