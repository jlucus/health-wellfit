using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellFitPlus.Database.Entities
{

    public class UserVideo
    {
        [ForeignKey("UserId")]
        public virtual UserProfile User { get; set; }
        [Key, Column(Order =0)]
        public Guid UserId { get; set; }

        [ForeignKey("VideoId")]
        public virtual Video Video { get; set; }
        [Key, Column(Order =1)]
        public Guid VideoId { get; set; }

        public bool IsWatched { get; set; }





    }
}
