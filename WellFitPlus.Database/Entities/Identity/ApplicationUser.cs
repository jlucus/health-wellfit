using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WellFitPlus.Database.Entities.Identity {
    public class ApplicationUser : IdentityUser {
        public ApplicationUser() {
            Disabled = true;
        }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string Company { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; }
        
        [NotMapped]
        [DataType(DataType.Password)]
        public virtual string Password { get; set; }

        public bool Disabled { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType) {
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);

            // TODO:    add custom user claims
            return userIdentity;
        }
    }
}
