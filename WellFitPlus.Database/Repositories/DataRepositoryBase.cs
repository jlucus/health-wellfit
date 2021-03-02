using WellFitPlus.Database.Contexts;

namespace WellFitPlus.Database.Repositories {
    public class DataRepositoryBase : RepositoryBase {
        protected new WellFitDataContext _context;

        public DataRepositoryBase() {
            _context = new WellFitDataContext();
        }
    }
}
