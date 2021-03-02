using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WellFitPlus.Database.Entities;


namespace WellFitPlus.WebAPI.BindingModels {

    /// <summary>
    /// Since there are differences between the Activities on the mobile device and what is on the server we
    /// need a binding model to handle the interaction between the two.
    /// </summary>
    public class ActivityBindingModel {

        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid UserID { get; set; }

        [Required]
        public Guid VideoID { get; set; }

        [Required]
        public bool Bonus { get; set; }

        [Required]        
        public DateTime? StartTime { get; set; }

        [Required]
        public DateTime? EndTime { get; set; }

        [Required]
        public DateTime? NotificationTime { get; set; }

        public static explicit operator Activity(ActivityBindingModel bm) {
            Activity activity = new Activity();
            activity.UserID =           bm.UserID;
            activity.Id =               bm.Id;
            activity.VideoID =          bm.VideoID;
            activity.Bonus =            bm.Bonus;
            activity.StartTime =        bm.StartTime.HasValue ? bm.StartTime.Value : DateTime.MinValue;
            activity.EndTime =          bm.EndTime.HasValue ? bm.EndTime.Value : DateTime.MinValue;
            activity.NotificationTime = bm.NotificationTime;

            return activity;
        }
    }
}
