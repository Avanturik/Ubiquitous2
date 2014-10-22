using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.SQLite.EF6;

namespace UB.Model
{
    public class DatabaseService : IDatabase
    {
        private UbiquitousContext context = new UbiquitousContext();

        public void GetViewersCountToday(Action callback)
        {
            if (callback == null)
                return;

            if (!context.StatisticsServices.Any(rec => rec.Name == "test"))
                context.StatisticsServices.Add(new StatisticsService() { Name = "test" });

            var service = context.StatisticsServices.First(rec => rec.Name == "test");

            context.StatisticsViewers.Add(new StatisticsViewers() { 
                Service = service,
                Viewerscount = 50
            });
            context.SaveChanges();
        }
    }
}
