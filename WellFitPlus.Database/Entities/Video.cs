using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WellFitPlus.Common;

namespace WellFitPlus.Database.Entities {
    [Table("Videos")]
    public class Video : EntityBase<Guid> {

        [Required, StringLength(100)]
        public string Path { get; set; }
		
		public VideoType Type { get; set; }
		
		[StringLength(100)] 
		public string Tags { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public bool Active { get; set; }

        [Required]
        public DateTime DateUploaded { get; set; }

        [Required]
        public DateTime DateModified { get; set; }

        [Required]
        public bool Deleted { get; set; }
    }
}
