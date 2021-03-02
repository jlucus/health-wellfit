using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellFitPlus.Database.Entities {
    [Table("Addresses")]
    public class Address : EntityBase<Guid> {

        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }        
    }
}
