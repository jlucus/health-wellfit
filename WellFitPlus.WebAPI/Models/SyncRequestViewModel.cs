using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WellFitPlus.WebAPI.BindingModels;
using WellFitPlus.WebAPI.Models;

namespace WellFitPlus.WebAPI.Models
{
    public class SyncRequestViewModel
    {
        [Required]
        public Guid UserID { get; set; }

        [Required]
        public float CacheSize { get; set; }
        
        [Required]
        public List<UserVideoViewModel> ListOfCurrentVideos { get; set; }

        [Required]
        public List<ActivityBindingModel> ListOfNewActivities { get; set; }

    }
}