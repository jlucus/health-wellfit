using System;
using WellFitPlus.Mobile.Models;

namespace WellFitPlus.Mobile.Services
{
    public class SessionService
    {
        public UserProfile User { get; set; }
        public UserSettings Settings { get; set; }
        public Configuration Configuration { get; set; }
        public string AuthToken { get; set; }
        public DateTime Issued { get; set; }
        public DateTime Expires { get; set; }

        public static SessionService Instance {
            get {
                if (_instance == null) {
                    _instance = new SessionService();
                }

                return _instance;
            }
        }

        private static SessionService _instance;

        private SessionService() {
            Configuration = Configuration.Instance;
        }

        public override string ToString() {
            return string.Format("Session:\r\n\tUser: {0}\r\n\tAuthToken: {1}\r\n\tIssued: {2}\r\n\tExpires: {3}", 
                User, AuthToken, Issued, Expires);
        }
    }
}
