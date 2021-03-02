using log4net;
using System;
using System.Data.Entity;
using System.Linq;
using WellFitPlus.Database.Entities;
using System.Collections.Generic;

namespace WellFitPlus.Database.Repositories {

    public class CompanyRepository : DataRepositoryBase {
        public static readonly ILog log = LogManager.GetLogger(typeof(CompanyRepository));

        public Company Get(Guid id) {
            return _context.Companies
                .Include(c => c.Address)
                .SingleOrDefault(c => c.Id == id);
        }

        public IQueryable<Company> GetAll() {
            return _context.Companies
                 .Include(c => c.Address);
        }

        public Company GetByGroupCode(string groupCode) {
            return _context.Companies
                .Where(c => c.GroupCode != null)
                .SingleOrDefault(c => c.GroupCode == groupCode);
        }

        public void Add(Company company) {
            Validate(company);

            _context.Companies.Add(company);
            _context.SaveChanges();
        }

        public void Edit(Company company) {
            Validate(company);

            _context.SaveChanges();
        }

        public void UpdateUsers(Guid companyId, IEnumerable<Guid> userIds) {
            var users = _context.Users.ToList();

            foreach (var user in users) {
                if (user.CompanyId == companyId) {
                    user.CompanyId = null;
                }
            }

            foreach (var userId in userIds) {
                var user = users.Single(u => u.Id == userId);
                user.CompanyId = companyId;
            }

            _context.SaveChanges();
        }

        /// <summary>
        /// Updates a user's company id field. This is to assign/unassign the user from a company.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="companyId"></param>
        public void UpdateUser(Guid userId, Guid? companyId) {
            var user = _context.Users.SingleOrDefault(u => u.Id == userId);
            Company company;

            if (user == null) {
                throw new ApplicationException("User does not exist");
            }

            if (companyId.HasValue)
            {
                company = Get(companyId.Value);

                if (company == null)
                {
                    throw new ApplicationException("Company does not exist");
                }

                user.CompanyId = company.Id;
            }
            else {
                user.CompanyId = null;
            }
            
            _context.SaveChanges();
        }

        private void Validate(Company company) {
            var otherCompanies = _context.Companies.Where(c => c.Id != company.Id);

            if (otherCompanies.Any(c => c.Name == company.Name)) {
                throw new ApplicationException("Company name already in use.");
            }

            if (otherCompanies.Any(c => c.GroupCode != null && c.GroupCode == company.GroupCode)) {
                throw new ApplicationException("Company group code already in use.");
            }
        }
    }
}