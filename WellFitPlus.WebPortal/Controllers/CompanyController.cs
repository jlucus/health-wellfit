using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WellFitPlus.Database.Entities;
using WellFitPlus.Database.Repositories;
using WellFitPlus.WebPortal.Attributes;
using WellFitPlus.WebPortal.Models;

namespace WellFitPlus.WebPortal.Controllers {

    [AuthorizeRoles("admin")]
    public class CompanyController : Controller {
        private readonly CompanyRepository repo;

        public CompanyController() {
            repo = new CompanyRepository();
        }

        public ActionResult Index() {
            var companies = repo.GetAll().ToList();

            return View(companies);
        }

        [HttpGet]
        public ActionResult Edit(Guid? id) {
            PopulateUserDropDown();

            if (!id.HasValue) {
                return View(new CompanyModel());
            } else {
                return View(new CompanyModel(repo.Get(id.Value)));
            }
        }

        [HttpPost]
        public ActionResult Edit(CompanyModel model) {
            PopulateUserDropDown();

            if (!ModelState.IsValid) {
                return View(model);
            }

            var company = repo.Get(model.Id) ?? new Company();

            UpdateModel<Company>(company);

            try {
                if (model.Id == Guid.Empty) {
                    repo.Add(company);
                } else {
                    repo.Edit(company);
                }

                List<Guid> assignedUserIds = new List<Guid>();

                if (model.AssignedUserIds != null) {
                    assignedUserIds = model.AssignedUserIds.Split(',')
                        .Select(id => Guid.Parse(id))
                        .ToList();
                }

                repo.UpdateUsers(company.Id, assignedUserIds);

            } catch (Exception ex) {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        private void PopulateUserDropDown() {
            var users = new UserRepository().GetAll()
                .Where(u => u.CompanyId == null)
                .ToList()
                .Select(u => new UserProfileModel(u))
                .OrderBy(c => c.FullName);
            
            ViewBag.UserDropDown = new SelectList(users, "Id", "FullName");
        }
    }
}