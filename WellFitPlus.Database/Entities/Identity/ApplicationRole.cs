using Microsoft.AspNet.Identity.EntityFramework;

namespace WellFitPlus.Database.Entities.Identity {
    public class ApplicationRole : IdentityRole {
        public ApplicationRole() 
            : base() {
        }

        public ApplicationRole(string name, string description) 
            : base(name) {

            Description = description;
        }

        public virtual string Description { get; set; }
    }
}
