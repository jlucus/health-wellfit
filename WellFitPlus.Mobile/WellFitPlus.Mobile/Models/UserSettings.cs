using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WellFitPlus.Mobile.Models
{
    // NOTE:    In the current configuration (MVP), only a single-user is supported. To support
    //          multiple users, remove the standalone "UserID" field, and utilize either a List
    //          of key-value pairs (List<KeyValuePair<Guid, object>>) or a Tuple, or a HashBag,
    //          or even an enumerable Lookup (probably not the best choice)
    public class UserSettings : Settings
    {
        #region Keys
        private const string USERID_KEY = "UserId";
        private const string REGISTRATION_DATE_KEY = "RegistrationDate";
        private const string IS_SUBSCRIBED_KEY = "IsSubscribed";
        private const string COMPANY_NAME_KEY = "CompanyName";
        private const string DOWNLOAD_ON_WIFI_ONLY_KEY = "WifiDownloadOnly";
        private const string MUTE_KEY = "Mute";

        // NOTE:    the "current" cache size (GB) should not be a "setting." similarly,
        //          the "clear cache" function in the settings screen is not a setting.
        private const string MAX_CACHE_SIZE_KEY = "CacheSize";

        // NOTE:    value represents seconds
        private const string VIDEO_PLAYBACK_DELAY_KEY = "VideoPlaybackDelay";

        private const string ALLOW_NOTIFICATIONS_KEY = "AllowNotifications";
        private const string DISPLAY_NOTIFICATIONS_ON_LOCK_SCREEN_KEY = "DisplayNotificationsOnLockScreen";
        private const string ALLOW_EMAILS_KEY = "AllowEmails";

        private const string ALARM_MANAGER_CONFIGURED_KEY = "AlarmManagerConfigured";

        private const string NOTIFICATION_FREQUENCY_KEY = "Frequency";
        private const string NOTIFICATION_DAYS_KEY = "Days";
        private const string NOTIFICATION_BEGIN_TIME_KEY = "BeginHour";
        private const string NOTIFICATION_END_TIME_KEY = "EndHour";
        private const string NOTIFICATIONS_ACTIVE_KEY = "Active";
        
        #endregion

        #region Properties
        [DefaultValue(null)]
        public Guid UserId
        {
            get { return AppSettings.GetValueOrDefault(USERID_KEY, DefaultValue<Guid>(nameof(UserId))); }
            set { AppSettings.AddOrUpdateValue(USERID_KEY, value); }
        }

        [DefaultValue(true)]
        public bool WifiDownloadOnly
        {
            get { return AppSettings.GetValueOrDefault(DOWNLOAD_ON_WIFI_ONLY_KEY, DefaultValue<bool>(nameof(WifiDownloadOnly))); }
            set { AppSettings.AddOrUpdateValue(DOWNLOAD_ON_WIFI_ONLY_KEY, value); }
        }

        [DefaultValue(false)]
        public bool Mute
        {
            get { return AppSettings.GetValueOrDefault(MUTE_KEY, DefaultValue<bool>(nameof(Mute))); }
            set { AppSettings.AddOrUpdateValue(MUTE_KEY, value); }
        }

		// In MB
        [DefaultValue(250f)]
        public float CacheSize
        {
            get { return AppSettings.GetValueOrDefault(MAX_CACHE_SIZE_KEY, DefaultValue<float>(nameof(CacheSize))); }
            set { AppSettings.AddOrUpdateValue(MAX_CACHE_SIZE_KEY, value); }
        }

        [DefaultValue(10)]
        public int VideoPlaybackDelay
        {
            get { return AppSettings.GetValueOrDefault(VIDEO_PLAYBACK_DELAY_KEY, DefaultValue<int>(nameof(VideoPlaybackDelay))); }
            set { AppSettings.AddOrUpdateValue(VIDEO_PLAYBACK_DELAY_KEY, value); }
        }

        [DefaultValue(true)]
        public bool AllowNotifications
        {
            get { return AppSettings.GetValueOrDefault(ALLOW_NOTIFICATIONS_KEY, DefaultValue<bool>(nameof(AllowNotifications))); }
            set { AppSettings.AddOrUpdateValue(ALLOW_NOTIFICATIONS_KEY, value); }
        }

        [DefaultValue(true)]
        public bool DisplayNotificationsOnLockScreen
        {
            get { return 
                    AppSettings.GetValueOrDefault(DISPLAY_NOTIFICATIONS_ON_LOCK_SCREEN_KEY, DefaultValue<bool>(nameof(DisplayNotificationsOnLockScreen))); }
            set { AppSettings.AddOrUpdateValue(DISPLAY_NOTIFICATIONS_ON_LOCK_SCREEN_KEY, value); }
        }

        [DefaultValue(true)]
        public bool AllowEmails
        {
            get { return AppSettings.GetValueOrDefault(ALLOW_EMAILS_KEY, DefaultValue<bool>(nameof(AllowEmails))); }
            set { AppSettings.AddOrUpdateValue(ALLOW_EMAILS_KEY, value); }
        }

		// The frequency of sending notificaitons.
        [DefaultValue(20)]
        public int Frequency
        {
            get { return AppSettings.GetValueOrDefault(NOTIFICATION_FREQUENCY_KEY, DefaultValue<int>(nameof(Frequency))); }
            set { AppSettings.AddOrUpdateValue(NOTIFICATION_FREQUENCY_KEY, value); }
        }

        [DefaultValue(62)]
        public int Days
        {
            get { return AppSettings.GetValueOrDefault(NOTIFICATION_DAYS_KEY, DefaultValue<int>(nameof(Days))); }
            set { AppSettings.AddOrUpdateValue(NOTIFICATION_DAYS_KEY, value); }
        }

        [DefaultValue(8)]
        public int BeginHour
        {
            get { return AppSettings.GetValueOrDefault(NOTIFICATION_BEGIN_TIME_KEY, DefaultValue<int>(nameof(BeginHour))); }
            set { AppSettings.AddOrUpdateValue(NOTIFICATION_BEGIN_TIME_KEY, value); }
        }

        [DefaultValue(17)]
        public int EndHour
        {
            get { return AppSettings.GetValueOrDefault(NOTIFICATION_END_TIME_KEY, DefaultValue<int>(nameof(EndHour))); }
            set { AppSettings.AddOrUpdateValue(NOTIFICATION_END_TIME_KEY, value); }
        }

        [DefaultValue(true)]
        public bool Active
        {
            get { return AppSettings.GetValueOrDefault(NOTIFICATIONS_ACTIVE_KEY, DefaultValue<bool>(nameof(Active))); }
            set { AppSettings.AddOrUpdateValue(NOTIFICATIONS_ACTIVE_KEY, value); }
        }

        [DefaultValue(false)]
        public bool AlarmManagerConfigured
        {
            get { return AppSettings.GetValueOrDefault(ALARM_MANAGER_CONFIGURED_KEY, DefaultValue<bool>(nameof(AlarmManagerConfigured))); }
            set { AppSettings.AddOrUpdateValue(ALARM_MANAGER_CONFIGURED_KEY, value); }
        }

        [DefaultValue(null)]
        public DateTime RegistrationDate
        {
            get { return AppSettings.GetValueOrDefault(REGISTRATION_DATE_KEY, DefaultValue<DateTime>(nameof(RegistrationDate))); }
            set { AppSettings.AddOrUpdateValue(REGISTRATION_DATE_KEY, value); }
        }

        [DefaultValue(false)]
        public bool IsSubscribed
        {
            get { return AppSettings.GetValueOrDefault(IS_SUBSCRIBED_KEY, DefaultValue<bool>(nameof(IsSubscribed))); }
            set { AppSettings.AddOrUpdateValue(IS_SUBSCRIBED_KEY, value); }
        }

        [DefaultValue("")]
        public string CompanyName {
            get { return AppSettings.GetValueOrDefault(COMPANY_NAME_KEY, DefaultValue<string>(nameof(CompanyName))); }
			set { AppSettings.AddOrUpdateValue(COMPANY_NAME_KEY, value); }
        }

        public bool FreeTrialHasExpired
        {
            get
            {
                if (IsSubscribed == true) { return false; }
                var expired = RegistrationDate < DateTime.Now.AddDays(-14);
                return expired;
            }
        }


        [Flags]
        public enum DaysOfWeek
        {
            Sunday = 1,
            Monday = 2,
            Tuesday = 4,
            Wednesday = 8,
            Thursday = 16,
            Friday = 32,
            Saturday = 64,

            None = 0,
            All = Weekdays | Weekend,
            Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,
            Weekend = Sunday | Saturday,
        }

        public DaysOfWeek NotificationDays
        {
            get
            {
                return (DaysOfWeek)Enum.Parse(typeof(DaysOfWeek), this.Days.ToString());
            }
        }

        #endregion

        #region Constructors
        public UserSettings(Guid userId)
        {
            UserId = userId;
        }
        #endregion

        #region Methods
        public static UserSettings GetSettings(Guid userId)
        {
            Guid existingId = AppSettings.GetValueOrDefault(USERID_KEY, Guid.Empty);

            if (existingId == null || existingId != userId)
            {
                App.Log(string.Format("No settings exist for user: {0}\r\n\t...Creating new user settings", userId));

                // for the MVP, assume we want a fresh set of settings with all their default values
                UserSettings settings = new UserSettings(userId);
                settings.Save();

                return settings;
            }

            var userSettings = GetExistingSettings(userId);
            return userSettings;
        }

        public static UserSettings GetExistingSettings(Guid? id = null)
        {
            Guid userId = id == null ? Guid.Empty : (Guid)id;
            Guid existingId = AppSettings.GetValueOrDefault(USERID_KEY, userId);

            // settings exist. we already are them. but, in case settings were added, include the defaults in the "get" calls.
            // this is a bit more manual/redundant than we'd like, but it's mostly driven by the "static" nature of this call
            return new UserSettings(existingId)
            {
                Mute = AppSettings.GetValueOrDefault(MUTE_KEY, false),
                WifiDownloadOnly = AppSettings.GetValueOrDefault(DOWNLOAD_ON_WIFI_ONLY_KEY, true),
                CacheSize = AppSettings.GetValueOrDefault(MAX_CACHE_SIZE_KEY, 250f),
                VideoPlaybackDelay = AppSettings.GetValueOrDefault(VIDEO_PLAYBACK_DELAY_KEY, 10),
                AllowNotifications = AppSettings.GetValueOrDefault(ALLOW_NOTIFICATIONS_KEY, true),
                DisplayNotificationsOnLockScreen = AppSettings.GetValueOrDefault(DISPLAY_NOTIFICATIONS_ON_LOCK_SCREEN_KEY, true),
                AllowEmails = AppSettings.GetValueOrDefault(ALLOW_EMAILS_KEY, true),
                Frequency = AppSettings.GetValueOrDefault(NOTIFICATION_FREQUENCY_KEY, 20),
                Days = AppSettings.GetValueOrDefault(NOTIFICATION_DAYS_KEY, 62),
                BeginHour = AppSettings.GetValueOrDefault(NOTIFICATION_BEGIN_TIME_KEY, 8),
                EndHour = AppSettings.GetValueOrDefault(NOTIFICATION_END_TIME_KEY, 17),
                Active = AppSettings.GetValueOrDefault(NOTIFICATIONS_ACTIVE_KEY, true),
                AlarmManagerConfigured = AppSettings.GetValueOrDefault(ALARM_MANAGER_CONFIGURED_KEY, false),
                RegistrationDate = AppSettings.GetValueOrDefault(REGISTRATION_DATE_KEY, DateTime.MinValue),
                IsSubscribed = AppSettings.GetValueOrDefault(IS_SUBSCRIBED_KEY, false),
            };
        }

		public bool IsUserWithinTrialPeriod() { 
			int trialPeriodLength = AppGlobals.TRIAL_PERIOD_DURATION; // In Dayss
			bool isWithinTrialPeriod = false;

			// Check if the user is within their trial periodd
			DateTime now = DateTime.Now;
			DateTime trialExpirationDate = this.RegistrationDate.AddDays(trialPeriodLength);
			if (now.CompareTo(trialExpirationDate) < 0)
			{
				isWithinTrialPeriod = true;
			}

			return isWithinTrialPeriod;
		}

		public List<System.DayOfWeek> NotificationSchedule()
		{
			int currentDays = Days;

			List<System.DayOfWeek> scheduledDays = new List<System.DayOfWeek>();
			for (int i = 0; i < 7; i++)
			{
				int currentPowerOfTwoDayRepresentation = (int)Math.Pow(2, i);

				// Perform binary operation to see if this power of 2 is within the registered days for notifications
				if ((currentDays & currentPowerOfTwoDayRepresentation) == currentPowerOfTwoDayRepresentation)
				{
					// If the current day is registered then add it's day to the list.
					switch (i)
					{
						case (int)DayOfWeek.Sunday:
							scheduledDays.Add(DayOfWeek.Sunday);
							break;
						case (int)DayOfWeek.Monday:
							scheduledDays.Add(DayOfWeek.Monday);
							break;
						case (int)DayOfWeek.Tuesday:
							scheduledDays.Add(DayOfWeek.Tuesday);
							break;
						case (int)DayOfWeek.Wednesday:
							scheduledDays.Add(DayOfWeek.Wednesday);
							break;
						case (int)DayOfWeek.Thursday:
							scheduledDays.Add(DayOfWeek.Thursday);
							break;
						case (int)DayOfWeek.Friday:
							scheduledDays.Add(DayOfWeek.Friday);
							break;
						case (int)DayOfWeek.Saturday:
							scheduledDays.Add(DayOfWeek.Saturday);
							break;
					}
				}
			}

			return scheduledDays;
		}

        public override string ToString()
        {
            return string.Format("Settings:\r\nUserID: {0}\r\n\tMute: {1}\r\n\tWiFi Download: {2}\r\n\tMax Cache: {3}\r\n\tVideo Delay: {4}\r\n\tNotifications: {5}\r\n\tDisplay Notifications: {6}\r\n\tEmails: {7}\r\n\tNotification Frequency: {8}\r\n\tNotification Days: {9}\r\n\tNotificationStart Time: {10}\r\n\tNotification End Time: {11}\r\n\tNotifications Active: {12}",
                UserId, Mute, WifiDownloadOnly, CacheSize, VideoPlaybackDelay, AllowNotifications, DisplayNotificationsOnLockScreen, AllowEmails, Frequency, Days, BeginHour, EndHour, Active);

        }
        #endregion
    }
}
