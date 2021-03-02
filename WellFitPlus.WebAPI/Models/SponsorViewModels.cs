using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace WellFitPlus.WebAPI.Models {

    public class SponsorView {

        [Required]
        [Display(Name = "Logo")]
        public Image Logo { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}
