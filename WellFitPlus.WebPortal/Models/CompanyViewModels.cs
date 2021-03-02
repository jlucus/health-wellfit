using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using WellFitPlus.Database.Entities;
using System.ComponentModel;

namespace WellFitPlus.WebPortal.Models {
    public class CompanyModel {

        public CompanyModel() {
            Users = new List<UserProfileModel>();
        }

        public CompanyModel(Company company) : this() {
            Id = company.Id;
            Name = company.Name;
            AnnualRenewal = company.AnnualRenewal;
            GroupCode = company.GroupCode;
            Active = company.Active;
            Address = company.Address ?? new Address();

            Users = company.Users.Select(u => new UserProfileModel {
                Id = u.Id,
                LastName = u.LastName,
                FirstName = u.FirstName,
                Email = u.Email
            }).ToList();

            AssignedUserIds = string.Join(",", Users.Select(u => u.Id.ToString()));
        }

        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public Address Address { get; set; }

        [Display(Name = "Group Code")]
        public string GroupCode { get; set; }

        [Display(Name = "Annual Renewal?")]
        public bool AnnualRenewal { get; set; }

        [Display(Name = "Active?")]
        public bool Active { get; set; }

        public string AssignedUserIds { get; set; }

        public List<UserProfileModel> Users { get; set; }
    }
}