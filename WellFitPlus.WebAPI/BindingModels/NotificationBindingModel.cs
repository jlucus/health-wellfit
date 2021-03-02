using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WellFitPlus.WebAPI.BindingModels {

    public class NotificationBindingModel {

        [Required]
        public Guid UserID { get; set; }

        [Required]
        public string Frequency { get; set; }

        [Required]
        public string Days { get; set; }

        [Required]
        public DateTime BeginTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public bool Active { get; set; }

        [Required]
        public DateTime ResumeOn { get; set; }
    }
}
