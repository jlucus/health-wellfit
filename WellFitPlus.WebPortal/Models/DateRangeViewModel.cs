using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ComponentModel.DataAnnotations;

namespace WellFitPlus.WebPortal.Models
{
    public class DateRangeViewModel
    {

        [Required, Display(Name ="Start Date")]
        public DateTime StartDate { get; set; }

        [Required, Display(Name = "End Date")]
        public DateTime EndDate { get; set; }


    }
}
