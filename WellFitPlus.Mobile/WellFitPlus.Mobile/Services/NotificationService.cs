﻿using System;
using System.Linq;
using System.Collections.Generic;

using Xamarin.Forms;

using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Abstractions;
using WellFitPlus.Mobile.Database.Repositories;

/* This is a Singleton class designed to handle scheduling notifications for the app.
 * NOTE: This will only create the database notifications and only fires the methods necessary to schedule the
 * 		 actual system notifications. A dependency service is used actually register local notifications with the
 * 		 mobile operating systems.
*/
namespace WellFitPlus.Mobile.Services
{
    public enum NotificationState
    {
        Background, // Received notification in the background or from a closed state.
        Foreground, // Received notification in the foreground
        None        // No notification received.
    }

    public class NotificationService
    {

        #region Static Fields
        public readonly static string NOTIFICATION_TIMESTAMP_KEY = "timestamp";
        private static readonly int NUM_DAYS_TO_SCHEDULE = 10;

        private static NotificationService _instance;
        #endregion

        #region Static Properties
        public static NotificationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NotificationService();
                }

                return _instance;
            }
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Gets a list of messages that should be used when showing local notifications to the user.
        /// These messages are based on criteria from how the use has been using the app (i.e. from their
        /// scheduled notification data "ActivitySessions")
        /// 
        /// The criteria is as follows (as of 7/5/2017):
        /// * IF the user performs 3 to 7 videos in a row THEN the next pushed message will include " great job, 
        ///   your on a streak"
        /// * IF the user misses 2 or more videos in a row THEN the next pushed message will include "get up, 
        ///   join in, your health depends on it!"
        /// * IF the user performs 8 or more videos in a row THEN the next pushed message includes "super streak 
        ///   in progress, keep it up!"
        /// * IF the user misses 8 or more videos in a row THEN the next pushed message includes "Well Fit Plus 
        ///   energizes, relieves stress, takes your aches and pains away. Join in!"
        /// * The default message should be: "Everybody up! Swipe to watch your next activity" for iOS and 
        ///   "Everybody up! Tap to watch your next activity" for Android
        /// </summary>
        /// <returns>The notification messages.</returns>
        /// <param name="scheduledActivitySessions">
        /// List of the most recent scheduled activity sessions. This includes pending activities. Also,
        /// while this method does not need the entire list in the database it would be wise to at least pass
        /// in at least the 50 most recent records so future adjustements to this method won't need to rely
        /// on updating calls to this method.
        /// 
        /// NOTE: We need to pass in these notifications manually instead of fetching them from the database
        ///       because this method may be called from multiple threads. This class also uses Xamarin.Form's
        ///       DependencyService for some properties. It is not guaranteed that Xamarin.Forms will have
        ///       been initialized where we need to get these messages; so we need this method to be static.
        /// </param>
        public static List<string> GetNotificationMessages(List<ActivitySession> scheduledActivitySessions)
        {

            var activityList = new List<ActivitySession>();

            if (scheduledActivitySessions != null)
            {
                // This is really just a sanity check in case future development changes something.
                // We need only the scheduled sessions; ordered from most recent to oldest.
                activityList = scheduledActivitySessions.Where(a => a.NotificationTime != DateTime.MinValue)
                                                            .OrderByDescending(a => a.NotificationTime)
                                                            .ToList();
            }

            int completedCount = 0;
            int missedCount = 0;
            int loopCount = 0;
            int limitLoopCount = 8; // We only need to check up to 8 continuous records (according to requirements)

            /* We don't know if there is a completed streak or missed streak as the most recent streak so 
			 * we need both completed and missed counts.
			 * The idea is to keep adding to either variable and as soon as we need to add to the other
			 * variable break out of the loop so we will always have one of the two variables equal to zero.
			 * This way we can difinitively tell what kind of streak the user is currently on.
			*/
            foreach (var activity in activityList)
            {
                if (activity.IsCompleted)
                {
                    if (missedCount != 0)
                    {
                        break; // We have finished adding to the missed count. Break out.
                    }

                    completedCount++;

                }
                else
                {
                    if (completedCount != 0)
                    {
                        break; // We have finished adding to the completed count. Break out.
                    }

                    missedCount++;
                }

                loopCount++;
                if (loopCount >= limitLoopCount)
                {
                    break;
                }
            }

            int result = completedCount - missedCount; // At least one variable will always be zero.

            return DetermineNotificationMessages(result);

        }

        /// <summary>
        /// Determines the notification messages.
        /// 
        /// NOTE: Since iOS notifications need to be pre-scheduled and cannot be dynamically sent we need to
        ///       create a list of messages to set each time.
        /// </summary>
        /// <returns>The notification messages.</returns>
        /// <param name="streakResult">
        /// Streak result. A positive number indicates a completion streak while a negative number represents
        /// a missed streak.
        /// </param>
        private static List<string> DetermineNotificationMessages(int streakResult)
        {
            /* Messages breakdown; based on how many ActivitySessions were either missed or completed to make a streak
			 * 
			 * Number Missed
			 * >= 2: "Get up, join in, your health depends on it!"
			 * >= 8: "Well Fit Plus energizes, relieves stress, takes your aches and pains away. Join in!"
			 * 
			 * Number completed
			 * 3-7 inclusive: "Great job, your on a streak!"
			 * >= 8: "Super streak in progress, keep it up!"
			 * 
			 * Default if none of the above applies
			 * "Everybody up! Swipe to watch your next activity" for iOS 
			 * "Everybody up! Tap to watch your next activity" for Android
			*/

            // NOTE: Notifications will default to the last message in the returned list if the
            //       entire list has been used.

            // Completed Messages
            string completed3To7 = "Great job, your on a streak!";
            string completed8OrMore = "Super streak in progress, keep it up!";

            // Missed Messages
            string missed2OrMore = "Get up, join in, your health depends on it!";
            string missed8OrMore = "Well Fit Plus energizes, relieves stress, takes your aches and pains away. Join in!";

#if __IOS__
            string defaultMessage = "Everybody up! Swipe to watch your next activity";
#elif __ANDROID__
            string defaultMessage = "Everybody up! Tap to watch your next activity";
#endif

            List<string> messages = new List<string>();

            if (streakResult >= 8)
			{
                messages.Add(completed8OrMore); // Missed 1
                messages.Add(defaultMessage); // Missed 2
                messages.Add(missed2OrMore); // Missed 3
                messages.Add(missed2OrMore); // Missed 3
				messages.Add(missed2OrMore); // Missed 4
				messages.Add(missed2OrMore); // Missed 5
				messages.Add(missed2OrMore); // Missed 6
                messages.Add(missed2OrMore); // Missed 7
				messages.Add(missed8OrMore);

            } else if (streakResult >= 3) {
                messages.Add(completed3To7);  // Missed 1
				messages.Add(defaultMessage); // Missed 2
				messages.Add(missed2OrMore); // Missed 3
				messages.Add(missed2OrMore); // Missed 3
				messages.Add(missed2OrMore); // Missed 4
				messages.Add(missed2OrMore); // Missed 5
				messages.Add(missed2OrMore); // Missed 6
				messages.Add(missed2OrMore); // Missed 7
				messages.Add(missed8OrMore);
			}

            if (streakResult <= -8) {
                messages.Add(missed8OrMore);

            } else if (streakResult <= -2) {
                
                for (int i = streakResult; i > -8; i--) {
                    messages.Add(missed2OrMore);
                }

                messages.Add(missed8OrMore);
            }

            if (messages.Count == 0) {
                // The user has either no activities or is outside the requirments for other
                // messages.
				messages.Add(defaultMessage); // Missed 1
				messages.Add(defaultMessage); // Missed 2
				messages.Add(missed2OrMore); // Missed 3
				messages.Add(missed2OrMore); // Missed 4
				messages.Add(missed2OrMore); // Missed 5
				messages.Add(missed2OrMore); // Missed 6
				messages.Add(missed2OrMore); // Missed 7
                messages.Add(missed2OrMore); // Missed 8
				messages.Add(missed8OrMore);
			}

            return messages;
        }
        #endregion

        #region Constants
		public const string NEW_NOTIFICATION_FLAG_ID = "New Notification";
        #endregion

        #region Private Fields
		private ActivityService _activityService = ActivityService.Instance;
		private ActivitySessionRepository _activitySessionRepo = ActivitySessionRepository.Instance;
		private NotificationRepository _notificationRepo = NotificationRepository.Instance;
		private ScheduledNotification _notificationToAutoOpenVideoPage; // Null if we aren't auto opening video page.
		private INotificationScheduler _notificationScheduler;
		private InAppPurchase _iapService;
        #endregion

        #region Public Fields
		// 
		public bool openedAppFromBackground = false; 
        #endregion

		private NotificationService()
		{
			_notificationScheduler = DependencyService.Get<INotificationScheduler>();
			_iapService = DependencyService.Get<InAppPurchase>();
		}

        #region Project Notification API

		public void ScheduleNotifications()
		{

			// Get the user's settings
			var settings = UserSettings.GetExistingSettings();

			// TODO: PRODUCTION: Remove below code when ready to go live
			//settings.Frequency = 1; // In Min

			if (_iapService.IsSubscribedOrInTrialPeriod() == false)
			{
				_notificationScheduler.CancelOSRegisteredNotifications(); // Stop sending local notifications.

				// Make sure that the user doesn't have lingering missed notifications pile up until they
				// subscribe again.
				_notificationRepo.DeleteAllRecords();

				// We are not scheduling notifications if the user is not subscribed and not within trial period.
				return;
			}

			var unHandledNotifications = _notificationRepo.GetNotifications();

			HandleMissedNotifications(unHandledNotifications);

			// Get the notifications in the database. (After we have handled missed notifications.)
			List<ScheduledNotification> notifications = _notificationRepo.GetNotifications();

			var numDaysWithNotifications = NumberOfDaysWithScheduledNotifications(notifications);
			if (numDaysWithNotifications < NUM_DAYS_TO_SCHEDULE)
			{
				int numDaysToAdd = NUM_DAYS_TO_SCHEDULE - numDaysWithNotifications;
				CreateNewNotificationsInDatabase(notifications, numDaysToAdd, settings);
			}

			notifications = _notificationRepo.GetNotifications();

			if (settings.AllowNotifications)
			{
				_notificationScheduler.RegisterNotificationsWithOS(notifications);
			}
			else {
				_notificationScheduler.CancelOSRegisteredNotifications();
			}
		}

		public void RescheduleNotifications()
		{
			CancelNotifications();

			// Make sure we account for any old notifications. Sanity Check before we clear Notification DB
			HandleMissedNotifications(_notificationRepo.GetNotifications());

			// Clear all notifications from database in order to re-schedule them.
			_notificationRepo.DeleteAllRecords();

			ScheduleNotifications();
		}

		public void CancelNotifications()
		{
			HandleMissedNotifications(_notificationRepo.GetNotifications());

			_notificationScheduler.CancelOSRegisteredNotifications();
		}

		// Convenience method so we don't have to make the call from the dependency service outside this class.
		public void AlertUserIfNewNotificationExists() {
			_notificationScheduler.AlertUserIfNewNotificationExists();

		}

		public void StartVideoPlaybackIfNewNotificationExists() {
			_notificationScheduler.StartVideoPlaybackIfNewNotificationExists();
		}

		public NotificationState HasRecievedNotification() {
			return _notificationScheduler.HasRecievedNotification();
		}
        #endregion

        #region Notification Methods

		/// <summary>
		/// Handles the missed notifications by creating their corresponding ActivitySession objects in the database
		/// and removing their ScheduledNotification objects from the database. Essentially we are adding them to the
		/// stats.
		/// </summary>
		/// <param name="allDbNotifications">All db notifications.</param>
		private void HandleMissedNotifications(List<ScheduledNotification> allDbNotifications)
		{
			var oldNotifications = allDbNotifications.Where(n => n.ScheduledTimestamp < DateTime.Now).ToList();
			var allActivities = _activitySessionRepo.GetActivities(DateTime.MinValue);

			if (oldNotifications.Count > 0)
			{

				// Create Activity record for each old notification if one doesn't already exist.
				foreach (ScheduledNotification notification in oldNotifications)
				{
					var existingActivity =
						allActivities.Where(a => a.NotificationTime == notification.ScheduledTimestamp).FirstOrDefault();

					if (existingActivity != null)
					{
						// If this notification is already accounted for in the database/statistics then skip it.
						continue;
					}

					var activity = _activityService.GetNewActivity();

					if (activity != null)
					{
						activity.StartTime = notification.ScheduledTimestamp;
						activity.NotificationTime = notification.ScheduledTimestamp;

						_activitySessionRepo.AddActivity(activity);
					}
					// NOTE: if the activity is null then there was not a video that we could use.
					// The app does not penalize users for not watching videos that they don't have yet.
				}

				_notificationRepo.DeleteNotifications(oldNotifications);
			}
		}

		private int NumberOfDaysWithScheduledNotifications(List<ScheduledNotification> notifications)
		{
			var orderedNotifications = notifications.OrderBy(n => n.ScheduledTimestamp).ToList();

			var dayCount = 0;
			var previousTrackedDay = 0; // Keep track of the last day accounted for.
			for (int i = 0; i < orderedNotifications.Count; i++)
			{
				var dayValue = orderedNotifications[i].ScheduledTimestamp.Day;
				if (dayValue != previousTrackedDay)
				{
					previousTrackedDay = dayValue;
					dayCount++;
				}
			}

			return dayCount;
		}

		private void CreateNewNotificationsInDatabase(List<ScheduledNotification> allDbNotifications,
			int numberOfDaysToSchedule, UserSettings settings)
		{
			var notificationDaySchedule = settings.NotificationSchedule();
			var newestNotification = allDbNotifications.OrderByDescending(n => n.ScheduledTimestamp).FirstOrDefault();

			if (notificationDaySchedule.Count == 0) return;

			if (newestNotification == null)
			{
				newestNotification = new ScheduledNotification
				{
					Id = 0,
					ScheduledTimestamp = DateTime.Now
				};

				if (notificationDaySchedule.Contains(DateTime.Now.DayOfWeek))
				{
					// Schedule notifications for today since we have no notifications
					var listToAddToDb = CreateScheduledNotificationsForPartialDay(DateTime.Now, settings);
					_notificationRepo.UpdateNotifications(listToAddToDb);
				}
			}

			var newestNotificationTimestamp = newestNotification.ScheduledTimestamp;

			DateTime currentNotificationDate = newestNotificationTimestamp;
			for (int i = 0; i < numberOfDaysToSchedule; i++)
			{
				// If the user only schedules one day a week we need to increment the day here to avoid error.
				currentNotificationDate = currentNotificationDate.AddDays(1);

				var currentDay = currentNotificationDate.DayOfWeek;
				// Navigate to the next day the user has scheduled for notifications.
				while (notificationDaySchedule.Contains(currentDay) == false)
				{
					currentNotificationDate = currentNotificationDate.AddDays(1);
					currentDay = currentNotificationDate.DayOfWeek;
				}

				var listToAddToDb = CreateScheduledNotificationsForOneDay(currentNotificationDate, settings);

				_notificationRepo.UpdateNotifications(listToAddToDb);
			}
		}

		private List<ScheduledNotification> CreateScheduledNotificationsForOneDay(DateTime date, UserSettings settings)
		{
			var numMinutesForNotifications = (settings.EndHour - settings.BeginHour) * 60;
			int numNotificationsForDay = (numMinutesForNotifications / settings.Frequency) + 1; // +1 for begining time

			var startDateTime = new DateTime(date.Year, date.Month, date.Day, settings.BeginHour, 0, 0);
			var minutesToAdd = 0;
			var listToAddToDb = new List<ScheduledNotification>();
			for (int i = 0; i < numNotificationsForDay; i++)
			{
				listToAddToDb.Add(new ScheduledNotification
				{
					Id = 0,
					ScheduledTimestamp = startDateTime.AddMinutes(minutesToAdd)
				});
				minutesToAdd += settings.Frequency;
			}

			return listToAddToDb;
		}

		/// <summary>
		/// Creates notifications for one day based upon the date inputted. This method is only used to schedule the
		/// first day the user logs into the app when there aren't any notifications scheduled yet.
		///
		/// This method will only schedule notifications AFTER the time held within the DateTime input parameter.
		/// Ex. date= 11/28/2016T09:30:00 This function will only schedule notifications for the day 11/28/2016 and
		/// only AFTER 9:30 am.
		/// </summary>
		/// <param name="date">The date that the notifications should be scheduled for. The time within
		/// this parameter is taken into consideration.
		/// </param>
		/// <param name="settings"></param>
		private List<ScheduledNotification> CreateScheduledNotificationsForPartialDay(DateTime date, UserSettings settings)
		{

			// If we are passed the EndHour in the settings then we can skip this method
			if (date.Hour >= settings.EndHour)
			{
				return new List<ScheduledNotification>();
			}

			var listToAddToDb = CreateScheduledNotificationsForOneDay(date, settings);

			// Remove the notifications that we don't need.
			listToAddToDb.RemoveAll(n => n.ScheduledTimestamp < date);

			return listToAddToDb;
		}
        #endregion
	}
}