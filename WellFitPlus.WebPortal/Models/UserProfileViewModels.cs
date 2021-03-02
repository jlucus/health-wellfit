using System;
using System.ComponentModel.DataAnnotations;
using WellFitPlus.Database.Entities;

namespace WellFitPlus.WebPortal.Models {

    public class UserProfileModel {

        public UserProfileModel() {
        }

        public UserProfileModel(UserProfile profile) : this() {
            Id = profile.Id;
            FirstName = profile.FirstName;
            LastName = profile.LastName;
            Email = profile.Email;
            CompanyId = profile.CompanyId;
        }

        public Guid Id { get; set; }

        [Required, Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Display(Name = "Company")]
        public Guid? CompanyId { get; set; }

        public string FullName {
            get { return string.Format("{0}, {1}", LastName, FirstName); }
        }
    }
}