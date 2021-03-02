using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace WellFitPlus.WebAPI.Models {

    public class VideoViewModel {

        [Required]
        public Guid ID { get; set; }

        [Required]
        [Display(Name = "Path")]
        public string Path { get; set; }

        [Required]
        [Display(Name = "Type")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "Tags")]
        public string Tags { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Required]
        [Display(Name = "Date Modified")]
        public DateTime DateModified { get; set; }

        [Required]
        [Display(Name = "Date Uploaded")]
        public DateTime DateUploaded { get; set; }
    }

    public class FileViewModel {

        [Required]
        public HttpPostedFileBase File { get; set; }
    }

    public class UserVideoViewModel : VideoViewModel
    {
        [Required]
        [Display(Name = "UserID")]
        public Guid UserID { get; set; }
        public DateTime LastPlayed { get; set; }
        public bool IsFavorite { get; set; }
    }
}
