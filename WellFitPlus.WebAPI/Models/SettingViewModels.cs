using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WellFitPlus.WebAPI.Models {

    public class SettingView {

        [Required]
        public Guid UserID { get; set; }

        [Required]
        public bool WiFiDownloadOnly { get; set; }

        [Required]
        public bool Mute { get; set; }

        [Required]
        public long CacheSize { get; set; }

        [Required]
        public int VideoDelayTime { get; set; }

        [Required]
        public bool Reminders { get; set; }

        [Required]
        public bool WellFitEmails { get; set; }

        [Required]
        public DateTime RolloverDate { get; set; }
    }
}
