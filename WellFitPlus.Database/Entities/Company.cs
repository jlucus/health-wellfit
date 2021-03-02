using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using WellFitPlus.Database.Entities;

namespace WellFitPlus.Database.Entities {
    [Table("Companies")]
    public class Company : EntityBase<Guid> {

        public Company() {
            Created = DateTime.Now;
            Active = true;

            Users = new HashSet<UserProfile>();
        }

        public Guid AddressID { get; set; }
        [ForeignKey("AddressID")]
        public virtual Address Address { get; set; }

        [Required]
        public string Name { get; set; }

        public Guid? BillingContactID { get; set; }
        [ForeignKey("BillingContactID")]
        public virtual UserProfile BillingPerson { get; set; }

        public Guid? SalesContactID { get; set; }
        [ForeignKey("SalesContactID")]
        public virtual UserProfile SalesPerson { get; set; }

        public bool AnnualRenewal { get; set; }

        public bool Active { get; set; }

        public DateTime Created { get; set; }

        public string GroupCode { get; set; }

        public virtual ICollection<UserProfile> Users { get; set; }
    }
}
