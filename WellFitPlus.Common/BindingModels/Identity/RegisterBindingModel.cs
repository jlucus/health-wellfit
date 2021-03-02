using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;

namespace WellFitPlus.Common.BindingModels.Identity
{
    public class RegisterBindingModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "A first name must be provided.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "A last name must be provided.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "A role must be provided.")]
        [Display(Name = "Role")]
        public string Role { get; set; }

        [StringLength(100, ErrorMessage = "Company name's length is not valid.")]
        [Display(Name = "Company")]
        public string Company { get; set; }

        public string GroupCode { get; set; }

        public virtual IdentityUserRole Roles { get; set; }
    }
}
