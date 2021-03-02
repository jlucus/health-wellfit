using System.Data.Entity;
using WellFitPlus.Database.Entities;
using System.Data.Entity.ModelConfiguration.Conventions;
using System;
using System.Linq;

namespace WellFitPlus.Database.Contexts {
    public class WellFitDataContext : DbContext {
        public DbSet<Log> Logs { get; set; }
        public DbSet<UserProfile> Users { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<NotificationSetting> NotificationSettings { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<UserVideo> UserVideos { get; set; }

        public WellFitDataContext()
              : base("WellFitSQLConnection") {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Properties<DateTime>().Where(p => new string[] { "StartTime", "EndTime" }.Contains(p.Name))
            .Configure(c => c.HasColumnType("datetime2"));
        }
    }
}
