using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class UbiquitousContext : DbContext
    {
        public UbiquitousContext()
        {
            Database.SetInitializer<UbiquitousContext>(null);
        }
        public DbSet<StatisticsService> StatisticsServices { get; set; }
        public DbSet<StatisticsViewers> StatisticsViewers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions
                .Remove<PluralizingTableNameConvention>();
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
