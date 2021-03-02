using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.Linq;
using WellFitPlus.Database.Contexts;
using WellFitPlus.Database.Entities.Identity;

namespace WellFitPlus.Database.Initializers {
    public class WellFitAuthInitializer : DropCreateDatabaseIfModelChanges<WellFitAuthContext> {
        protected override void Seed(WellFitAuthContext context) {
            InitializeData(context);
        }

        public static void InitializeData(WellFitAuthContext context) {
            RoleManager<IdentityRole> roles = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (!roles.RoleExists("admin")) {
                roles.Create(new IdentityRole("admin"));
            }

            if (!roles.RoleExists("user")) {
                roles.Create(new IdentityRole("user"));
            }

            SeedAdminUser(context, "admin@asgrp.com", "123456");
        }

        private static void SeedAdminUser(WellFitAuthContext context, string email, string password) {

            if (!context.Users.Any(u => u.UserName == email)) {
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var userToInsert = new ApplicationUser {
                    UserName = email,
                    Email = email,
                    FirstName = email.Split('@')[0],
                    LastName = email.Split('@')[0],
                    RegistrationDate = DateTime.Now
                };

                userManager.Create(userToInsert, password);
                userManager.AddToRole(userToInsert.Id, "admin");
            }
        }
    }
}
