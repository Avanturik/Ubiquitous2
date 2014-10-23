using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class UbiquitousContext : DbContext
    {
        private const int databaseVersion = 1;
        private const string defaultDbPath = @".\database\ubiquitousdefault.sqlite";
        private const string workDbPath = @".\database\ubiquitous.sqlite";
        public UbiquitousContext()
        {
            Database.SetInitializer<UbiquitousContext>(null);
            MigrateDatabase();
        }
        public DbSet<StatisticsService> StatisticsServices { get; set; }
        public DbSet<StatisticsViewers> StatisticsViewers { get; set; }

        private void MigrateDatabase()
        {
            try
            {
                if (!File.Exists(workDbPath))
                {
                    File.Copy(defaultDbPath, workDbPath);
                    AddTestData();
                }
            }
            catch(Exception e)
            {
                Log.WriteError("database migration: {0}", e.Message);
            }
            //TODO: check version, run structure and data migrations
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions
                .Remove<PluralizingTableNameConvention>();
        }

        private void AddTestData()
        {
            var startDate = new DateTime(2014, 10, 1, 12, 0, 0);
            var rand = new Random();
            for (int i = 1; i < 500; i++)
            {
                var datetime = startDate.AddMinutes(i*5);
                StatisticsViewers.Add(
                    new StatisticsViewers() {
                        DateTime = datetime,
                        ServiceId = 1,
                        Viewerscount = rand.Next(1, 50),
                    });
            }
            SaveChanges();
        }
    }



    [Table("services")]
    public class StatisticsService
    {
        [Key]
        [Column("service_id")]  
        public long ServiceId { get; set; }
        [Column("name")]
        public string Name { get; set; }
    }

    [Table("statistics_viewers")]
    public class StatisticsViewers
    {
        public StatisticsViewers()
        {
            DateTime = DateTime.Now;
        }
        [Key]
        [Column("viewers_id")]
        public long ViewersId { get; set; }
        [Column("dt")]
        public DateTime DateTime { get; set; }
        [Column("viewers_count")]
        public int Viewerscount { get; set; }        
        [Column("service_id")]
        public long ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public virtual StatisticsService Service { get; set; }
    }
}
