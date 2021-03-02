using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WellFitPlus.WebAPI.Models
{
    public class UserVideoViewModel : VideoViewModel
    {
        [Required]
        [Display(Name = "UserID")]
        public Guid UserID { get; set; }
        public DateTime LastPlayed { get; set; }
        public DateTime DownloadDate { get; set; }
        public bool IsFavorite { get; set; }
        public int ViewCount { get; set; }
        public bool DownloadedSuccessfully { get; set; }
        public bool Deleted { get; set; }

        public bool FlaggedForDeletion { get; set; }
    }
}