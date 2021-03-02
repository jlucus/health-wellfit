using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace WellFitPlus.WebPortal.Models {

    public class MessageViewModel {

        [Required]
        public Guid ID { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

    }
}
