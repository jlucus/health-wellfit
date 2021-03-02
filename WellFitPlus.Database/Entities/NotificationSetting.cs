using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WellFitPlus.Common;

namespace WellFitPlus.Database.Entities {
    [Table("NotificationSettings")]
    public class NotificationSetting: EntityBase<Guid> {

        public Guid UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual UserProfile User { get; set; }

        [Required]
        public NotificationFrequency Frequency { get; set; }
		
		[Required, StringLength(100)] 
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
