using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace WellFitPlus.Database.Entities {
    [Table("UserProfiles")]
    public class UserProfile : EntityBase<Guid> {

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [ForeignKey("Company")]
        public Guid? CompanyId { get; set; }

        [InverseProperty("Users")]
        public virtual Company Company { get; set; }
    }
}
