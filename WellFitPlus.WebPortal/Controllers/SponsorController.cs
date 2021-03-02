using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using System.Web.Hosting;
using WellFitPlus.Database;
using WellFitPlus.Database.Entities;
using WellFitPlus.Database.Repositories;
using WellFitPlus.WebPortal.Models;
//using System.Web;
//using System.Web.Http;

using WellFitPlus.WebPortal.Views;

namespace WellFitPlus.WebPortal.Controllers {
    public class SponsorController : Controller {

        const string SPONSOR_DIRECTORY = "~/Content/Sponsor";

        // POST: /video/MainPage
        public ActionResult MainPage() {


            return View();
        }


        [HttpPost]
        public ActionResult LoadSponsor(HttpPostedFileBase sponsorToUpload) {

            // Verify that the user selected a file
            if (sponsorToUpload != null && sponsorToUpload.ContentLength > 0) {
                // extract only the filename
                var fileName = Path.GetFileName(sponsorToUpload.FileName);

                // store the file 
                //var path = Path.Combine(Server.MapPath(VIDEO_DIRECTORY), fileName);
                var path = Path.Combine(HostingEnvironment.MapPath(SPONSOR_DIRECTORY));
                //var path = Path.Combine(VIDEO_DIRECTORY, fileName);
                path += "\\" + fileName;
                sponsorToUpload.SaveAs(path);

                SponsorRepository _sponsorRepo = new SponsorRepository();

                Sponsor newSponsor = new Sponsor();

                newSponsor.Name = fileName;

                MemoryStream target = new MemoryStream();
                sponsorToUpload.InputStream.CopyTo(target);
                byte[] data = target.ToArray();

                newSponsor.Logo = data;


                _sponsorRepo.Add(newSponsor);

            }

            // redirect back to the index action to show the form once again
            return RedirectToAction("MainPage", "Video");
        }

    }
}
