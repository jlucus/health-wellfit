using System;
using System.Linq;
using System.Data.Entity.Migrations;
using WellFitPlus.Database.Contexts;
using WellFitPlus.Database.Initializers;

namespace WellFitPlus.Database.AuthenticationMigrations
{
    public class Configuration : DbMigrationsConfiguration<WellFitAuthContext>
    {
        public Configuration() {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            MigrationsDirectory = @"AuthenticationMigrations";
        }

        protected override void Seed(WellFitAuthContext context) {
            WellFitAuthInitializer.InitializeData(context);

            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
        }
    }
}
