using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using WellFitPlus.Database.Repositories;
using WellFitPlus.WebAPI.Models;

namespace WellFitPlus.WebAPI.Controllers
{
    [RoutePrefix("api/Sponsor")]
    public class SponsorController : ApiController
    {
        public static readonly ILog log = LogManager.GetLogger(typeof(VideoController));
        private SponsorRepository _sponsorRepo = new SponsorRepository();

        [HttpPost]
        [Route("GetSponsors")]
        public List<SponsorView> GetSponsors()
        {
            List<SponsorView> sponsorViews = new List<SponsorView>();
            var sponsors = _sponsorRepo.GetSponsors();
            foreach (var sponsor in sponsors)
            {
                MemoryStream ms = new MemoryStream(sponsor.Logo);
                SponsorView newSponsor = new SponsorView()
                {
                    Logo = Image.FromStream(ms),
                    Name = sponsor.Name
                };
                sponsorViews.Add(newSponsor);
            }

            return sponsorViews;
        }
    }
}