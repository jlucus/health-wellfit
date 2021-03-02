using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using WellFitPlus.Database.Entities.Identity;

namespace WellFitPlus.Database.Contexts {
    public class WellFitAuthContext : IdentityDbContext<ApplicationUser> {
        public new virtual DbSet<ApplicationRole> Roles { get; set; }

        public WellFitAuthContext()
            : base("WellFitAuthConnection") {

        }

        public static WellFitAuthContext Create() {
            return new WellFitAuthContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            if (modelBuilder == null) {
                throw new ArgumentNullException("modelBuilder");
            }

            modelBuilder.Entity<IdentityUserLogin>().HasKey<string>(l => l.UserId);
            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new { r.RoleId, r.UserId });

            modelBuilder.Entity<IdentityUser>().ToTable("AspNetUsers");

            EntityTypeConfiguration<ApplicationUser> table = modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
            table.Property((ApplicationUser u) => u.UserName).IsRequired();
            modelBuilder.Entity<ApplicationUser>().HasMany((ApplicationUser u) => u.Roles);
        }
    }
}