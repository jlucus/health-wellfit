using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using WellFitPlus.Database.Entities;

namespace WellFitPlus.Database.Repositories {
    public class SponsorRepository : DataRepositoryBase {

        public static readonly ILog log = LogManager.GetLogger(typeof(SponsorRepository));

        public Guid Add(Sponsor Sponsor) {
            try {

                _context.Sponsors.Add(Sponsor);
                _context.SaveChanges();

            } catch (Exception ex) {
                log.Error(ex);
                return Guid.Empty;
            }
            return Sponsor.Id;
        }

        public List<Sponsor> GetSponsors() {
            List<Sponsor> sponsorList = new List<Sponsor>();
            try {
                sponsorList = _context.Sponsors.ToList();

            } catch (Exception ex) {
                log.Error(ex);
            }
            return sponsorList;
        }

        public Sponsor GetSponsor(Guid sponsorID) {
            Sponsor sponsor = new Sponsor();
            try {
                sponsor = _context.Sponsors.Where(s => s.Id == sponsorID).FirstOrDefault();
            } catch (Exception ex) {
                log.Error(ex);
            }
            return sponsor;
        }
    }
}
