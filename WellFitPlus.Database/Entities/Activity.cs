using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellFitPlus.Database.Entities {
    [Table("Activities")]
    public class Activity: EntityBase<Guid> {

        public Guid UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual UserProfile User { get; set; }

        public Guid VideoID { get; set; }
        [ForeignKey("VideoID")]
        public virtual Video Video { get; set; }

        public bool Bonus { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? StartTime { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? EndTime { get; set; }

        [Column(TypeName ="datetime2")]
        public DateTime? NotificationTime { get; set; }
    }
}
