namespace WellFitPlus.Database.AuthenticationMigrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddRegistrationDate : DbMigration
    {
        public override void Up() {
            // for this migration: the Model definition adds the new column, but leaves NULLs in the fields (which we can't have)
            // so drop the column and re-add it with the current date. subsequent registrations should have the correct date
            DropColumn("AspNetUsers", "RegistrationDate");
            AddColumn("AspNetUsers", "RegistrationDate", c => c.DateTime(nullable: false, defaultValueSql: "GetDate()"));
        }

        public override void Down() {
            DropColumn("AspNetUsers", "RegistrationDate");
        }
    }
}
