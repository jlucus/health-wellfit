using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace WellFitPlus.Database.Entities {
    [Table("Sponsors")]
    public class Sponsor : EntityBase<Guid> {
		
		[Required] 
		public byte[] Logo { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }
    }
}
