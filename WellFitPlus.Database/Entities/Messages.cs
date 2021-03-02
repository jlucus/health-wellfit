using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellFitPlus.Database.Entities {
    [Table("Messages")]
    public class Message: EntityBase<Guid> {

        public string Description { get; set; }
    }
}
