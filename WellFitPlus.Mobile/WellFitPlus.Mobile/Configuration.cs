﻿namespace WellFitPlus.Mobile
{
    public class Configuration {
		//TODO: PRODUCTION: Switch SERVER url to production server
		//public const string SERVER = @"192.168.79.108:10000"; // Local machine path. delete me
		//public const string SERVER = @"192.168.1.187:10000"; // Local machine path. delete me

		public const string SERVER = @"wellfitplus.com:10000"; // Well Fit Production Server

		public const string RESOURCE_URI = @"API";
        public const string API_PATH = @"api";

        public const string AUTH_PATH = @"token";
        public const string LOCAL_DATABASE_NAME = @"localStorage.db3";

        private static Configuration _instance;

        public static Configuration Instance {
            get {
                if (_instance == null) {
                    _instance = new Configuration();
                }

                return _instance;
            }
        }

        public bool IsSecure { get; set; }

        private Configuration() {
            IsSecure = false;
        }
    }
}
