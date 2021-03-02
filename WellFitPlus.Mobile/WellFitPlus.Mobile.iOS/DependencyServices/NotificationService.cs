using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

using Xamarin.Forms;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitPlus.Mobile.Services;
using WellFitPlus.Mobile.iOS.DependencyServices;

using Foundation;
using UIKit;

using WellFitPlus.Mobile.PlatformViews;
using WellFitPlus.Mobile.Abstractions;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.iOS;
using WellFitPlus.Mobile.iOS.PlatformViews;

[assembly: Xamarin.Forms.Dependency(typeof(WellFitPlus.Mobile.iOS.DependencyServices.NotificationService))]
namespace WellFitPlus.Mobile.iOS.DependencyServices
{
    public class NotificationService : INotificationScheduler
    {
		#region Static Fields
		public readonly static string NOTIFICATION_TIMESTAMP_KEY = "timestamp";
		public readonly static int IOS_MAX_NOTIFICATION_COUNT = 62;
		private static readonly int NUM_DAYS_TO_SCHEDULE = 2;
   
		// Used for when the user is in the app and we need to present a pop-up
		private static UILocalNotification currentNotification;
		#endregion

		#region Constants
		public const string NEW_NOTIFICATION_FLAG_ID = "New Notification";
		#endregion

		private ActivityService _activityService = ActivityService.Instance;
		private ActivitySessionRepository _activitySessionRepo = ActivitySessionRepository.Instance;
		private NotificationRepository _notificationRepo = NotificationRepository.Instance;

		// Used if we want to auto-open the video playback from a background or closed app state.
		private ScheduledNotification _notificationToAutoOpenVideoPage;

		#region Interface Methods

		public async void RegisterNotificationsWithOS(List<ScheduledNotification> notifications)
		{

			// We are not checking if the user has notifications enabled here. The local notifications will not
			// fire if the user has turned them off in the settings. Use the AlertUserIfNewNotificationExists
			// method to check for the app setting on notifications.
			// NOTE: Make sure that we handle any old notifications in the DB before calling this method.

			// Make sure the zero index is the next scheduled notification.
			notifications = notifications.OrderBy(n => n.ScheduledTimestamp).ToList(); // Sanity check

			int numNotificationsToSchedule = IOS_MAX_NOTIFICATION_COUNT;
			if (notifications.Count < IOS_MAX_NOTIFICATION_COUNT)
			{
				numNotificationsToSchedule = notifications.Count;
			}

            // Needs to run on separate thread due to large record sizes.
            var scheduledActivities = await Task.Run(() => _activitySessionRepo.GetAllScheduledActivities());

            // Get a list of messages to show the user. These will emulate Conditional Notifications but
            // since notification cannot be dynamically changed we must pre-schedule them.
            List<string> messages = 
                Services.NotificationService.GetNotificationMessages(scheduledActivities);

			// Observations show that we need to make sure notifications are scheduled on 
			// main thread or they won't fire.
			Device.BeginInvokeOnMainThread(() =>
			   {
				   // Register the notifications
				   List<UILocalNotification> notificationsToSchedule = new List<UILocalNotification>();
				   for (int i = 0; i < numNotificationsToSchedule; i++)
				   {
                       string message;

                       if (i < messages.Count) {
                           message = messages[i];
	                   } else {
                           message = messages.Last();
	                   } 

					   DateTime notifyDate = notifications[i].ScheduledTimestamp;
					   string isoNotifyDate = DateFormatHelper.DateTimeToIsoFormat(notifyDate);
					   NSDate fireDate = ConvertDateTimeToNSDate(notifyDate);

					   //App.Log("iOS NotificationService: Scheduling Notification for " + isoNotifyDate);

					   NSDictionary userInfo = new NSDictionary(NOTIFICATION_TIMESTAMP_KEY, isoNotifyDate);

					   // Create Notification
					   var notification = new UILocalNotification();
					   notification.FireDate = fireDate;
					   notification.AlertAction = AppGlobals.Notifications.NOTIFICATION_TITLE;
					   notification.AlertBody = message;
					   notification.ApplicationIconBadgeNumber = 1;
					   notification.SoundName = UILocalNotification.DefaultSoundName;
					   notification.UserInfo = userInfo; // To keep track of notification times

					   notificationsToSchedule.Add(notification);
				   }

				   App.Log(
					"iOS NotificationService: Scheduling " + notificationsToSchedule.Count + " notification with the OS");
				   // This will clear out any previous notifications. We are re-adding most each time. Simpler method.
				   UIApplication.SharedApplication.ScheduledLocalNotifications = notificationsToSchedule.ToArray();
				   UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;

				   // In case we are on the profile screen refresh the stats.
				   MessagingCenter.Send<string>("", AppGlobals.Events.REFRESH_PROFILE);
			   });
		}

        public void CancelOSRegisteredNotifications()
        {
			Device.BeginInvokeOnMainThread(() =>
			   {
				   UIApplication.SharedApplication.CancelAllLocalNotifications();
			   });
            
        }

		/// <summary>
		/// Alerts the user of new notification if one exists.
		/// 
		/// NOTE: This should only be used if the app is currently in an active state (i.e. not backgrounded or
		/// 	  opening from a closed state).
		/// </summary>
		public void AlertUserIfNewNotificationExists() {

			if (VideoPlaybackRenderer.IsVideoPlaybackActive) {
				// The video page is currently active. We should not push another video on screen.
				App.Log("iOS NotificationService: Video Playback screen is active. Skip opening another video.");
				ResetNotificationData();
				return;
			}

			if (HasRecievedNotification() == NotificationState.Foreground) {
				AppDelegate appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;

				// (don't show UIAlert if the user should not see them)
				if (!appDelegate.IsSubscribedOrInTrialPeriod())
				{
					CancelOSRegisteredNotifications();
					return;
				}

				// If there is any notifition from any source then we will be able to get a timestamp
				DateTime notificationTimestamp; 

				var notification = currentNotification;

				NSString key = new NSString(NOTIFICATION_TIMESTAMP_KEY);
				string isoTimestamp = notification.UserInfo.ObjectForKey(key) as NSString;
				notificationTimestamp = DateFormatHelper.ParseIsoFormattedString(isoTimestamp);

				// Get all of the activities
				List<ActivitySession> activities = _activitySessionRepo.GetActivities(DateTime.MinValue);

				ActivitySession activity =
					activities.Where(a => a.NotificationTime == notificationTimestamp).FirstOrDefault();

				if (activity == null)
				{
					// This notification has not been accounted for. Create a new activity
					activity = _activityService.GetNewActivity();

					if (activity != null)
					{
						activity.NotificationTime = notificationTimestamp;

						_activitySessionRepo.AddActivity(activity);
					}
					else {
						// There was no video that the activity could use or some other error occured. Exit method.
						this.ResetNotificationData();
						App.Log("iOS NotificationService: There was an error creating Activity for video.");
						return;
					}
					// NOTE: if the activity is null then there was not a video that we could use.
					// The app does not penalize users for not watching videos that they don't have yet.

				}

				// We are currently in the app. Prompt the user to see their scheduled video.
				UIAlertController okayAlertController =
					UIAlertController.Create(
						currentNotification.AlertAction,
						currentNotification.AlertBody,
						UIAlertControllerStyle.Alert
					);

				okayAlertController.AddAction(
					UIAlertAction.Create("NOT NOW", UIAlertActionStyle.Cancel, (result) =>
					{
						var repo = ActivitySessionRepository.Instance;
						activity.IsPending = false;
						repo.UpdateActivity(activity);
						this.ResetNotificationData();

						MessagingCenter.Send<string>("", AppGlobals.Events.REFRESH_PROFILE);
					})
				);

				okayAlertController.AddAction(
					UIAlertAction.Create("OK", UIAlertActionStyle.Default, (result) =>
					{
						MessagingCenter.Send<string, int>("", AppGlobals.Events.VIDEO_PAGE, activity.Id);

					// Update Last Notification Session
					MessagingCenter.Send<Models.ActivitySession>(activity, AppGlobals.Events.ACTIVITY_ACKNOWLEDGED);
						this.ResetNotificationData();
					})
				);

				UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(
				okayAlertController, true, null);

				UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;

				this.ResetNotificationData(); // Reset the notification data to get ready for the next one.
			}
		}

		/// <summary>
		/// Starts the video playback page if a new notification exists.
		/// 
		/// Used to force open the video playback page after the user gets a notification.
		/// 
		/// NOTE: This should only be used in situations where the user is first opening the app (from either a 
		/// 	  closed or backgrounded state) after a notification has been received.
		/// 	  
		/// Do not use if the app is currently active and in the foreground.
		/// </summary>
		public void StartVideoPlaybackIfNewNotificationExists()
		{
			if (VideoPlaybackRenderer.IsVideoPlaybackActive) {
				// The video page is currently active. We should not push another video on screen.
				App.Log("iOS NotificationService: Video Playback screen is active. Skip opening another video.");
				ResetNotificationData();
				return;
			}
			if (HasRecievedNotification() == NotificationState.Background)
			{
				AppDelegate appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;

				// (don't show UIAlert if the user should not see them)
				if (!appDelegate.IsSubscribedOrInTrialPeriod())
				{
					CancelOSRegisteredNotifications();
					return;
				}

				// If there is any notifition from any source then we will be able to get a timestamp
				DateTime notificationTimestamp;
				if (_notificationToAutoOpenVideoPage != null)
				{
					notificationTimestamp = _notificationToAutoOpenVideoPage.ScheduledTimestamp;
				}
				else {
					// We have no new notification
					return;
				}

				// Get all of the activities
				List<ActivitySession> activities = _activitySessionRepo.GetActivities(DateTime.MinValue);

				ActivitySession activity =
					activities.Where(a => a.NotificationTime == notificationTimestamp).FirstOrDefault();

				if (activity == null)
				{
					// This notification has not been accounted for. Create a new activity
					activity = _activityService.GetNewActivity();

					if (activity != null)
					{
						activity.NotificationTime = notificationTimestamp;

						_activitySessionRepo.AddActivity(activity);
					}
					else {
						// There was no video that the activity could use or some other error occured. Exit method.
						this.ResetNotificationData();
						App.Log("iOS NotificationService: There was an error creating Activity for video.");
						return;
					}
					// NOTE: if the activity is null then there was not a video that we could use.
					// The app does not penalize users for not watching videos that they don't have yet.

				}

				// We have opened the app after we received a notification. Navigate directly to the Video screen
 
				_activityService.AcknowledgeActivity(activity);
				activity = _activitySessionRepo.GetActivity(activity.Id); // Refresh the activity

				// Create New Video Page And Navigate To It
				var video = new VideoPlaybackPage(activity);
				NavigationPage.SetHasNavigationBar(video, false);
				Xamarin.Forms.Application.Current.MainPage = new NavigationPage(video);

				UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;

				this.ResetNotificationData(); // Reset the notification data to get ready for the next one.
			}

		}

		public bool HasNotificationAuthorization() {

			UIUserNotificationType types = UIApplication.SharedApplication.CurrentUserNotificationSettings.Types;

			if (types != UIUserNotificationType.None) {
				return true;
			}

			return false;
		}

		/// <summary>
		/// Checks a flag to see if there was a new notification that hasn't been handled yet
		/// 
		/// This is set on the native side
		/// </summary>
		/// <returns><c>true</c>, if new notification was hased, <c>false</c> otherwise.</returns>
		public NotificationState HasRecievedNotification()
		{
			NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
			bool hasNewNotification = defaults.BoolForKey(NEW_NOTIFICATION_FLAG_ID);
			bool notificationIsSet = currentNotification != null;
			bool autoOpenNotificationIsSet = _notificationToAutoOpenVideoPage != null;

			if (hasNewNotification && notificationIsSet)
			{
				return NotificationState.Foreground;
			}
			else if (autoOpenNotificationIsSet) {
				return NotificationState.Background;
			}

			return NotificationState.None;
		}

		#endregion

		#region Local Notification Methods

		/// <summary>
		/// This will set the _notificationToAutoOpenVideoPage field so the AlertUserIfNewNotificationExists method
		/// can AutoOpen the video playback page when the App icon is clicked after a notification was received.
		/// 
		/// This should only be used in situations where the user was presented a notification but did not swipe it; 
		/// instead the user opened the app from the app icon.
		/// 
		/// A missed notification will not be present after HandleMissedNotifications() is called.
		/// 
		/// If there was no missed notification then this method will do nothing.
		/// </summary>
		public void SetAutoOpenVideoNotificationFromMissedNotification() {
			
			var oldNotifications = _notificationRepo
				.GetNotifications().Where(n => n.ScheduledTimestamp < DateTime.Now).ToList();

			var mostRecentMissedNotification = oldNotifications
				.OrderByDescending(n => n.ScheduledTimestamp).FirstOrDefault();

			if (mostRecentMissedNotification != null) {
				App.Log("iOS NotificationService: Setting missed notification");
				App.Log("iOS NotificationService: Is video playback active: " + VideoPlaybackRenderer.IsVideoPlaybackActive);
				_notificationToAutoOpenVideoPage = mostRecentMissedNotification;
			}

		}

		public void SetAutoOpenVideoNotificationFromLocalNotification(UILocalNotification localNotification) {

			// Get local notification timestamp to refer to the correct notification in the DB
			NSString key = new NSString(NOTIFICATION_TIMESTAMP_KEY);
			string isoString = localNotification.UserInfo.ObjectForKey(key) as NSString;
			DateTime localNotificationTimestamp = DateFormatHelper.ParseIsoFormattedString(isoString);

			var notifications = _notificationRepo.GetNotifications();
			var notification = 
				notifications.Where(n => n.ScheduledTimestamp == localNotificationTimestamp).FirstOrDefault();

			if (notification == null) {
				// Something went wrong. We don't have a ScheduledNotification in the database for this.
				return;
			}

			App.Log("iOS NotificationService: Setting notification from local notification");
			App.Log("iOS NotificationService: Is video playback active: " + VideoPlaybackRenderer.IsVideoPlaybackActive);
			_notificationToAutoOpenVideoPage = notification;
		}

		#endregion

		#region Handling Fired Notification
		/// <summary>
		/// Save any notification we need to local disk so we can retrieve it later
		/// 
		/// </summary>
		public void SetNewNotificationData(UILocalNotification notification){
			NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
			defaults.SetBool(true, NEW_NOTIFICATION_FLAG_ID);
			defaults.Synchronize();

			currentNotification = notification; // Set the notification so we can use it later.
		}

		public void ResetNotificationData(){
			NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
			defaults.SetBool(false, NEW_NOTIFICATION_FLAG_ID);
			defaults.Synchronize();

			currentNotification = null; // We have consumed the notification so remove it.
			_notificationToAutoOpenVideoPage = null;
		}
		#endregion

		#region Util Methods
		private NSDate ConvertDateTimeToNSDate(DateTime date)
		{

			var utcDateTime = date.ToUniversalTime();
			DateTime reference = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var newDate = NSDate.FromTimeIntervalSinceReferenceDate((utcDateTime - reference).TotalSeconds);

			return newDate;
		}
		#endregion

    }
}
