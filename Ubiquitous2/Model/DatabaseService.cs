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
        private object contextLock = new object();

        public void GetViewersCount(int intervalMinutes, Action<List<StatisticsViewers>> callback)
        {
            if (callback == null)
                return;

            Try(() =>
            {
                var maxRecords = 288;
                lock( contextLock )
                {
                    var maxViewersPerInterval = context.StatisticsViewers.ToList().GroupBy(rec => rec.ServiceId + " " + (rec.DateTime.Ticks / TimeSpan.FromMinutes(intervalMinutes).Ticks).ToString())
                        .Select(group => {
                            DateTime time = group.First().DateTime;
                            time = time.AddMinutes(-(time.Minute % intervalMinutes));
                            time = time.AddMilliseconds(-time.Millisecond - 1000 * time.Second);

                            Log.WriteInfo( "Time: {0} viewers: {1}", time, group.Max(item => item.Viewerscount));
                            
                            return new StatisticsViewers()
                            {
                                DateTime = time,
                                Viewerscount = group.Max( x => x.Viewerscount),
                            };
                        });
                    callback(maxViewersPerInterval.Skip(Math.Max(0, maxViewersPerInterval.Count() - maxRecords)).Take(maxRecords).ToList());
                }
            });
        }

        private long GetServiceId( string name )
        {
            lock(contextLock)
            {
                var service = context.StatisticsServices.FirstOrDefault(item => item.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (service == null)
                {
                    var nextId = context.StatisticsServices.Max(item => item.ServiceId) + 1;
                    context.StatisticsServices.Add(new StatisticsService()
                    {
                        Name = name,
                    });
                    Try(() => context.SaveChanges());
                }

                service = context.StatisticsServices.FirstOrDefault(item => item.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

                return service.ServiceId;
            }

        }
        public void AddViewersCount(string serviceName, int viewersCount)
        {
            lock(contextLock)
            {
                Try(() =>
                {
                    context.StatisticsViewers.Add(
                        new StatisticsViewers()
                        {
                            DateTime = DateTime.Now,
                            ServiceId = GetServiceId(serviceName),
                            Viewerscount = viewersCount,
                        });
                    context.SaveChanges();

                });
            }
        }
        private string Try( Action action )
        {
            if (action == null)
                return null;
            string error = null;
            try
            {
                action();
            }
            catch( Exception e)
            {
                Log.WriteError("Database exception: {0}", e.Message);
                error = e.Message + Environment.NewLine + e.StackTrace;
            }

            return error;
        }
    }

}
