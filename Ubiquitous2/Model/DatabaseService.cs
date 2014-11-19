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

        public void GetViewersCount(int intervalMinutes, Action<List<StatisticsViewers>> callback)
        {
            if (callback == null)
                return;

            TryDB(() =>
            {
                var maxRecords = 288;
                var maxViewersPerInterval = context.StatisticsViewers.ToList().GroupBy(rec =>
                {
                    DateTime time = rec.DateTime;
                    time = time.AddMinutes(-(time.Minute % intervalMinutes));
                    time = time.AddMilliseconds(-time.Millisecond - 1000 * time.Second);
                    return time;
                }).Select(group => new StatisticsViewers()
                {
                    DateTime = group.Key,
                    Viewerscount = group.Max(item => item.Viewerscount)
                });
                callback(maxViewersPerInterval.Skip(Math.Max(0, maxViewersPerInterval.Count() - maxRecords)).Take(maxRecords).ToList());
            });
        }
        private void TryDB( Action action )
        {
            try
            {
                if (action != null)
                    action();
            }
            catch(Exception e)
            {
                Log.WriteError("Database exception {0}", e.Message);
            }
        }
    }

}
