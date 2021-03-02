using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellFitPlus.Database.Entities {
    [Table("Settings")]
    public class Setting : EntityBase<Guid> {

        public Guid UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual UserProfile User { get; set; }

        [Required]
        public bool WiFiDownloadOnly { get; set; }
		
		[Required] 
		public bool Mute { get; set; }
		
		[Required]  
		public long CacheSize { get; set; } // In MB
		
		[Required] 
		public int VideoDelayTime { get; set; }
		
		[Required] 
		public bool Reminders { get; set; }
		
		[Required] 
		public bool WellFitEmails { get; set; }
        
        [Column(TypeName = "datetime2")]
        public DateTime RolloverDate { get; set; }
    }
}
