using log4net;
using System;
using System.Linq;
using System.Data.Entity;
using WellFitPlus.Database.Entities;

namespace WellFitPlus.Database.Repositories {

    public class UserRepository : DataRepositoryBase {
        public static readonly ILog log = LogManager.GetLogger(typeof(UserRepository));

        public UserRepository() : base() {
        }

        public UserProfile Get(Guid id) {
            return _context.Users
                .Include(u => u.Company)
                .SingleOrDefault(u => u.Id == id);
        }

        public IQueryable<UserProfile> GetAll() {
            return _context.Users
                .Include(u => u.Company);
        }

        public IQueryable<UserProfile> GetByCompany(Guid id) {
            return _context.Users.Where(u => u.CompanyId == id);
        }

        public void Add(UserProfile user) {
            Validate(user);

            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Edit(UserProfile user) {
            Validate(user);

            _context.SaveChanges();
        }

        private void Validate(UserProfile user) {
        }

    }
}