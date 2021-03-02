using System;
using System.Linq;
using System.Web.Mvc;
using WellFitPlus.Database.Entities;
using WellFitPlus.Database.Repositories;
using WellFitPlus.WebPortal.Attributes;
using WellFitPlus.WebPortal.Models;

namespace WellFitPlus.WebPortal.Controllers {

    [AuthorizeRoles("admin")]
    public class UserController : Controller {
        private readonly UserRepository repo;

        public UserController() {
            repo = new UserRepository();
        }

        public ActionResult Index() {
            var users = repo.GetAll()
                .OrderBy(u => u.Email)
                .ToList();

            return View(users);
        }

        [HttpGet]
        public ActionResult Edit(Guid? id) {
            PopulateCompanyDropDown();

            if (!id.HasValue) {
                return View(new UserProfileModel());
            } else {
                return View(new UserProfileModel(repo.Get(id.Value)));
            }
        }
         
        [HttpPost]
        public ActionResult Edit(UserProfileModel model) {
            PopulateCompanyDropDown(model.CompanyId);

            if (!ModelState.IsValid) {
                return View(model);
            }

            var user = repo.Get(model.Id) ?? new UserProfile();

            UpdateModel<UserProfile>(user);

            try {
                if (model.Id == Guid.Empty) {
                    repo.Add(user);
                } else {
                    repo.Edit(user);
                }
            } catch (Exception ex) {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        private void PopulateCompanyDropDown(Guid? selected = null) {
            var companies = new CompanyRepository().GetAll()
                .OrderBy(c => c.Name)
                .ToList()
                .Select(c => new CompanyModel(c));

            ViewBag.CompanyDropDown = new SelectList(companies, "Id", "Name", selected);
        }
    }
}