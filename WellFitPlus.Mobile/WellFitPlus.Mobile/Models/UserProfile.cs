using System;
using System.Collections.Generic;
using System.Text;

namespace WellFitPlus.Mobile.Models
{
    public class UserProfile
    {
        public string Username { get; set; }
        public Guid UserID { get; set; }

        public override string ToString()
        {
            return string.Format("Username: {0}, UserID: {1}", Username, UserID);
        }
    }

    public class UserInfo
    {
        public string Email { get; set; }
        public bool HasRegistered { get; set; }

        public override string ToString()
        {
            return string.Format("Email: {0}, Registered: {1}", Email, HasRegistered);
        }
    }

    public class UserRegistration
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string GroupCode { get; set; }
    }

    public class ChangePassword
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
