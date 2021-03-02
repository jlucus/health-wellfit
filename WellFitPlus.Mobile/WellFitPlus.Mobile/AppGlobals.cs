using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WellFitPlus.Mobile
{
    public static class AppGlobals
    {
		#region Constants
		public const string NOTIFICATION_MESSAGE = "Notification Message";
		public const int TRIAL_PERIOD_DURATION = 14; // In Days
		#endregion

		#region Enums

		//
		// Summary:
		//     A generic result type on a processing expression, transaction, or method
		public enum ResultType
        {
            //
            // Summary:
            //     Result type is unknown
            Unknown = 0,
            //
            // Summary:
            //     Represents a successful expression return flag
            Success = 1,
            //
            // Summary:
            //     Represents a failed expression return flag
            Failure = 2
        }

        #endregion

        #region General

        public static class General
        {
			#if __IOS__
			public static readonly string WELLFIT_PLUS_APP_STORE_LINK = 
				"https://itunes.apple.com/us/app/well-fit-plus/id1145824915?ls=1&mt=8";
			#elif __ANDROID__
            public static readonly string WELLFIT_PLUS_APP_STORE_LINK = 
				"https://play.google.com/store/apps/details?id=com.asgrp.Mobile.WellFitPlus";
			#endif
        }       
        
        #endregion

        #region Images

        public static class Images
        {
            //public static readonly string BLUE_GRADIENT_IMAGE = "blueGradient.png";
            public static readonly string BLUE_GRADIENT_IMAGE = "backgroundGradient.png";
            public static readonly string UP_ARROW = "upArrow.png";

            public static readonly string STREAK_TOP_LEFT = "profileTopLeft.png";
            public static readonly string STREAK_TOP_RIGHT = "profileTopRight.png";
            public static readonly string STREAK_TOP_LEFT2 = "profileTopLeft2.png";
            public static readonly string STREAK_TOP_RIGHT2 = "profileTopRight2.png";

            public static readonly string MENU_BAR_BACKGROUND = "menuBarBackground.png";
            public static readonly string MENU_BUTTON = "menuButton.png";
            public static readonly string BACK_BUTTON = "backButton.png";

            public static readonly string LOGIN_BACKGROUND = "loginBackground.png";
        }

        #endregion

        #region Events

        public static class Events
        {
            public static readonly string SHARE = nameof(SHARE);
            public static readonly string REFRESH_PROFILE = nameof(REFRESH_PROFILE);
			public static readonly string NOTIFY_DOWNLOADING_PROGRESS = nameof(NOTIFY_DOWNLOADING_PROGRESS);
			public static readonly string NOTIFY_DOWNLOADING_COMPLETE = nameof(NOTIFY_DOWNLOADING_COMPLETE);
			public static readonly string NOTIFY_RETRY_DOWNLOADING_COMPLETE = nameof(NOTIFY_RETRY_DOWNLOADING_COMPLETE);
            public static readonly string ACTIVITY_ACKNOWLEDGED = nameof(ACTIVITY_ACKNOWLEDGED);
            public static readonly string ACTIVITY_COMPLETED = nameof(ACTIVITY_COMPLETED);
            public static readonly string COMPLETE_ACTIVITY = nameof(COMPLETE_ACTIVITY);
            public static readonly string VIDEO_PAGE = nameof(VIDEO_PAGE);
            public static readonly string VIDEO_HISTORY_PAGE = nameof(VIDEO_HISTORY_PAGE);
            public static readonly string SYNC_DOWNLOAD_VIDEOS_STARTED = nameof(SYNC_DOWNLOAD_VIDEOS_STARTED);
            public static readonly string SYNC_DOWNLOAD_VIDEOS_COMPLETED = nameof(SYNC_DOWNLOAD_VIDEOS_COMPLETED);
        }

        #endregion

        #region Messages

        public static class Messages
        {
            public static readonly string DISCLAIMER_TITLE = "Participation Disclaimer";
            public static readonly string DISCLAIMER_MESSAGE = 
@"The Well Fit Plus program is intended for individuals with no exercise or activity restrictions. You should consult with your physician before participating if you have a medical condition, any restriction to activity due to a health condition or experience any type of pain during physical activities.
                
This program is intended to provide simple low impact functional movements and utilizes gentle exercise and purposeful movement activities to get you up and moving. STOP IF YOU EXPERIENCE DISCOMFORT, PAIN, SHORTNESS OF BREATH, or ANY QUESTIONABLE PHYSICAL SYMPTOMS.
                
Your activation of this video presentation serves as your acknowledgment and understanding of your participation in this program at your own risk.";
        }

        #endregion

        #region Notifications

        public static class Notifications
        {
            public static readonly int NOTIFICATION_CHECK_INTERVAL = 10000;

            public static readonly string NOTIFICATION_TITLE = "WellFit Notification";

#if __IOS__
            public static readonly string NOTIFICATION_MESSAGE = "Everybody Up! Swipe to watch your next activity.";
#elif __ANDROID__
            public static readonly string NOTIFICATION_MESSAGE = "Everybody Up! Tap to watch your next activity.";
#endif

			public const string NAVIGATE_TO_VIDEO = "NAVIGATE_TO_VIDEO";

            public static readonly Dictionary<string, int> FREQUENCIES = new Dictionary<string, int>
            {
                { "20 Minutes", 20 },
                { "40 Minutes", 40 },
                { "1 Hour", 60 },
                { "2 Hours", 120 }
            };
        }

        #endregion

        #region Settings

        public static class Settings
        {
			
#if __IOS__
            public static readonly string MOBILE_DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif __ANDROID__
            public static readonly string MOBILE_DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#endif

			public static List<string> PERMITTED_VIDEO_EXTENSIONS = new List<string>()
            {
                ".mp4"
            };

            public static readonly Dictionary<string, int> CACHE_SIZES = new Dictionary<string, int>
            {
                { "250 MB", 250 },
                { "500 MB", 500 },
                { " 1 GB ", 1000 }
            };
        }

#endregion

		#region Database

        public static class Database
        {
            public static string DatabasePath
            {
                get
                {
                    var sqliteFilename = Configuration.LOCAL_DATABASE_NAME;
#if __IOS__
				string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal); // Documents folder
				string libraryPath = Path.Combine (documentsPath, "..", "Library"); // Library folder
				var path = Path.Combine(libraryPath, sqliteFilename);
#else
#if __ANDROID__
                    var path = Path.Combine(AppGlobals.Settings.MOBILE_DIRECTORY, sqliteFilename);
#else
				// WinPhone
				var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, sqliteFilename);;
#endif
#endif
                    return path;
                }
            }
        }

		#endregion

		#region Feedback

        public static class Feedback
        {
            public static readonly string FEEDBACK_EMAIL_ADDRESS_TO = "feedback@wellfitplus.com";
            public static readonly string FEEDBACK_EMAIL_ADDRESS_FROM = "feedback@wellfitplus.com";
            public static readonly string FEEDBACK_EMAIL_SUBJECT = "Well Fit Plus Feedback";
        }

		#endregion

		#region User Preferences Keys (simple saved data keys)
		public static readonly string DAILY_TIP_MESSAGE_PREF = "com.asgrp.Mobile.WellFitPlus.daily_tip_message";
		public static readonly string DAILY_TIP_TIMESTAMP_PREF = "com.asgrp.Mobile.WellFitPlus.daily_tip_timestamp";
		#endregion
    }
}
