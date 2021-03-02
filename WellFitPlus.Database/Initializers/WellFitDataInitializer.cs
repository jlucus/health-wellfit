using System.Data.Entity;
using WellFitPlus.Database.Contexts;

namespace WellFitPlus.Database {
    public class WellFitDataInitializer : DropCreateDatabaseIfModelChanges<WellFitDataContext> {
        protected override void Seed(WellFitDataContext context) {
            InitializeData(context);
        }

        public static void InitializeData(WellFitDataContext context) {
        }
    }
}