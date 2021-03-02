using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellFitPlus.Database.Entities {

    /// <summary>
    /// Entity class for log4net ADONetAppender support.
    /// </summary>
    [Table("Logs")]
    public class Log {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }
 
        [Required, StringLength(255)]
        public string Thread { get; set; }

        [Required, StringLength(50)]
        public string Level { get; set; }

        [Required, StringLength(255)]
        public string Logger { get; set; }

        [Required, StringLength(4000)]
        public string Message { get; set; }

        [StringLength(2000)]
        public string Exception { get; set; }
    }
}
