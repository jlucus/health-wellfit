using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;
namespace WellFitPlus.WebAPI.Models {

    public class CompanyView {

        [Required]
        public Guid CompanyID { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string Zip { get; set; }

        [Required]
        public string Name { get; set; }

        public string BillingContact { get; set; }

        public string SalesContact { get; set; }

        [Required]
        public bool AnnualRenewal { get; set; }
    }
}
