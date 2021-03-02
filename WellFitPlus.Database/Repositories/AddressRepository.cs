using log4net;
using System;
using System.Linq;
using WellFitPlus.Database.Entities;

namespace WellFitPlus.Database.Repositories {
    public class AddressRepository : DataRepositoryBase {

        public static readonly ILog log = LogManager.GetLogger(typeof(AddressRepository));

        public void Add(Address Address) {
            try {
                _context.Addresses.Add(Address);
                _context.SaveChanges();

            } catch (Exception ex) {
                log.Error(ex);
            }
        }

        public void Edit(Address Address) {
            try {
                _context.SaveChanges();

            } catch (Exception ex) {
                log.Error(ex);
            }
        }

        public Address GetAddress(Guid AddressID) {
            Address address = new Address();
            try {
                address = _context.Addresses.Where(c => c.Id == AddressID).FirstOrDefault();

            } catch (Exception ex) {
                log.Error(ex);
            }
            return address;
        }
    }
}
