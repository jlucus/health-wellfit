using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Util;
using Android.Preferences;
using Android.Support.V4.App;

using Xamarin.Forms;
using Xamarin.Android.NUnitLite;

using WellFitPlus.Mobile.Droid;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Utils;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitPlus.Mobile.Helpers;
using WellFitPlus.Mobile.Services;
using WellFitPlus.Mobile.Abstractions;
using WellFitPlus.Mobile.Droid.HelperClasses;

[assembly: Xamarin.Forms.Dependency(typeof(NotificationScheduler))]
namespace WellFitPlus.Mobile.Droid
{

	/// <summary>
	/// This class is responsible for managing the AlarmManager that is responsible
	/// for firing PendingIntents to the WellFitPlus.Mobile.Droid.NotificationService.
    /// 
    /// It is also the primary API location for the DependencyService used within the shared code.
    /// Think of it as the "middle man" between the shared code and the actual Android native code.
	/// </summary>
	public class NotificationScheduler: INotificationScheduler
	{

		#region Constants
		private const string TAG = "NotificationScheduler";

		public const string TIMESTAMP_EXTRA = "com.asgrp.Mobile.WellFitPlus.timestamp_extra";
		public const string OPENED_FROM_NOTIFICATION_EXTRA = "com.asgrp.Mobile.WellFitPlus.opened_from_notification";
		public const string NOTIFICATION_MESSAGE_EXTRA = "com.asgrp.Mobile.WellFitPlus.notification_message_extra";
		public const string PREFS_NOTIFICATION_TIMESTAMP = "com.asgrp.Mobile.WellFitPlus.notification_timestamp";

		// In the future the message will be dynamically changing. This is to prepare for that.
		public const string PREFS_NOTIFICATION_MESSAGE = "com.asgrp.Mobile.WellFitPlus.notification_message";
		public const string PREFS_NEW_NOTIFICATION = "new_notification_available";

		// What state was the app in when the notification was recieved.
		public const string PREFS_NOTIFICATION_APP_STATE = "notification_app_state";
		#endregion

		#region Static Fields
		// Just an abstract number so we don't overdo it in the database
		private static readonly int ANDROID_MAX_NOTIFICATION_COUNT = 100; 
		#endregion

		#region Private Fields
		// Used for configuring AlarmManager. NOTE: This MUST be the application context and NOT an activity context
		private Context _applicationContext = Android.App.Application.Context;
		private AlarmManager _alarmManager;
		private NotificationManager _notificationManager;
		private ActivityService _activityService = ActivityService.Instance;
		private ActivitySessionRepository _activitySessionRepo = ActivitySessionRepository.Instance;
		private NotificationRepository _notificationRepo = NotificationRepository.Instance;
		#endregion

		#region Private Properties
		private AlarmManager AlarmManagerProp { 
			get { 
				if (_alarmManager == null)
				{
					_alarmManager = (AlarmManager)_applicationContext.GetSystemService(Context.AlarmService);
				}

				return _alarmManager;
			}
		}

		private NotificationManager NotificationManagerProp { 
			get {
				if (_notificationManager == null) {
					_notificationManager = NotificationManager.FromContext(_applicationContext);
				}

				return _notificationManager;
			}
		}
		#endregion

		#region Interface Methods

		public async void RegisterNotificationsWithOS(List<ScheduledNotification> notifications)
		{
			AlarmManager alarmManager = AlarmManagerProp;

			// Make sure the zero index is the next scheduled notification.
			notifications = notifications.OrderBy(n => n.ScheduledTimestamp).ToList(); // Sanity check

			int numNotificationsToSchedule = ANDROID_MAX_NOTIFICATION_COUNT;
			if (notifications.Count < ANDROID_MAX_NOTIFICATION_COUNT)
			{
				numNotificationsToSchedule = notifications.Count;
			}

            // We will pre-schedule the Android notification messages the same way we pre-schedule the iOS
            // notification messages.
            // Retrieving the notifications needs to be run on a separate thread due to size of records.
            List<ActivitySession> scheduledActivities = await Task.Run(() => _activitySessionRepo.GetAllScheduledActivities());
            List<string> messages = Services.NotificationService.GetNotificationMessages(scheduledActivities);

            for (int i = 0; i < numNotificationsToSchedule; i++)
            {
                string message;

                if (i < messages.Count)
                {
                    message = messages[i];
                }
                else
                {
                    message = messages.Last();
                }

                ScheduledNotification notification = notifications[i];
                notification.Message = message; // Needed to cancel the pending intent later
                String isoTimestamp = DateFormatHelper.DateTimeToIsoFormat(notification.ScheduledTimestamp);
                Int64 millisTimestamp = GetMillisFromIsoString(isoTimestamp);
                int pendingIntentId = (int)millisTimestamp;

                Intent intent = new Intent(_applicationContext, typeof(NotificationService));
                intent.PutExtra(TIMESTAMP_EXTRA, isoTimestamp);
                intent.PutExtra(NOTIFICATION_MESSAGE_EXTRA, message);

				PendingIntent pi = PendingIntent.GetService(_applicationContext, pendingIntentId, intent, 0);

				if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
				{
				   // Do things the Kitkat way
				   alarmManager.SetExact(AlarmType.RtcWakeup, millisTimestamp, pi);
				}
				else
				{
				   // Do things the pre-Kitkat way
				   alarmManager.Set(AlarmType.RtcWakeup, millisTimestamp, pi);
				}
			}

            // We need to update the database with the notifications now since we have set messages
            // to them. This is so the CancelOSRegisteredNotifications method can re-create the exact
            // PendingIntent needed to cancel the registered OS notifications properly.
            _notificationRepo.UpdateNotifications(notifications);
		}

		public void CancelOSRegisteredNotifications()
		{
			AlarmManager alarmManager = AlarmManagerProp;
			NotificationRepository notificationRepo = NotificationRepository.Instance;

			List<ScheduledNotification> notifications = notificationRepo.GetNotifications();

			int numNotificationsToSchedule = ANDROID_MAX_NOTIFICATION_COUNT;
			if (notifications.Count < ANDROID_MAX_NOTIFICATION_COUNT)
			{
				numNotificationsToSchedule = notifications.Count;
			}

			// We need to re-create each possible scheduled AlarmManager alarm in order to cancel 
            // the corresponding one
			// If one doesn't exist this will fail gracefully.
			for (int i = 0; i < numNotificationsToSchedule; i++)
			{
                try
                {
                    ScheduledNotification notification = notifications[i];
                    String isoTimestamp = DateFormatHelper.DateTimeToIsoFormat(notification.ScheduledTimestamp);
                    Int64 millisTimestamp = GetMillisFromIsoString(isoTimestamp);
                    int pendingIntentId = (int)millisTimestamp;

                    Intent intent = new Intent(_applicationContext, typeof(NotificationService));
                    intent.PutExtra(TIMESTAMP_EXTRA, isoTimestamp);
                    intent.PutExtra(NOTIFICATION_MESSAGE_EXTRA, notification.Message);

                    PendingIntent pi = PendingIntent.GetService(_applicationContext, pendingIntentId, intent, 0);

                    alarmManager.Cancel(pi);
					Task.Delay(10);
                }
                catch (Exception ex)
                {

                }
				
			}
		}

		public bool HasNotificationAuthorization()
		{
			// NOTE: On earlier versions of android the NotificationManager.IsNotificationPolicyAccessGranted
			// crashes the app. Since we are compiling against api level 22 the user accepts the permissions
			// upon app downloading. So there really is no need to check this property (which is not straight
			// forward to check). The alternative on newer API versions is to save boolean values after
			// asking the user whether they would like to use the permission.
			return true;
			//return NotificationManagerProp.IsNotificationPolicyAccessGranted;
		}

		/// <summary>
		/// Alerts the user of new notification if one exists.
		/// 
		/// NOTE: This should only be used if the app is currently in an active state (i.e. not backgrounded or
		/// 	  opening from a closed state).
		/// </summary>
		public void AlertUserIfNewNotificationExists()
		{

			if (HasRecievedNotification() == NotificationState.Foreground)
			{

				// TODO: Check for subscription here
				//if (!appDelegate.IsSubscribedOrInTrialPeriod())
				//{
				//	CancelOSRegisteredNotifications();
				//	return;
				//}

				ActivitySession activity = GetActivityFromNotificationData();
                string message = GetNotificationMessage();

				if (activity == null) { 
					// There was no video that the activity could use or some other error occured. Exit method.
					this.ResetNotificationData();
					return;
				}

                NotificationData data = new NotificationData { 
                    ActivityId = activity.Id,
                    Message = message
                }; 

				// Send message to MainActivity to show notification dialog.
				MessagingCenter.Send<object, NotificationData>(
                    this, 
                    MainActivity.NOTIFY_ACTIVE_USER_MESSAGE, 
                    data);

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
			if (HasRecievedNotification() == NotificationState.Background)
			{

				// TODO: Check user subscription here.
				//if (!appDelegate.IsSubscribedOrInTrialPeriod())
				//{
				//	CancelOSRegisteredNotifications();
				//	return;
				//}

				ActivitySession activity = GetActivityFromNotificationData();

				if (activity == null)
				{
					// There was no video that the activity could use or some other error occured. Exit method.
					this.ResetNotificationData();
					return;
				}

				// We have opened the app after we received a notification. Navigate directly to the Video screen

				_activityService.AcknowledgeActivity(activity);
				activity = _activitySessionRepo.GetActivity(activity.Id); // Refresh the activity

				// Tell the MainActivity to open the Video Playback page.
				var videoPageData = new VideoPageData(activity.Id, false);
				MessagingCenter.Send<string, VideoPageData>("", AppGlobals.Events.VIDEO_HISTORY_PAGE, videoPageData);

				var appContext = Android.App.Application.Context;
				var notificationManager = 
					appContext.GetSystemService(Context.NotificationService) as NotificationManager;
				notificationManager.CancelAll(); // Remove all previous notifications from the notification drawer.

				this.ResetNotificationData(); // Reset the notification data to get ready for the next one.
			}
		}

		public NotificationState HasRecievedNotification() {
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(_applicationContext);

			bool hasNewNotification = prefs.GetBoolean(PREFS_NEW_NOTIFICATION, false);
			AppState notificationAppState = 
				(AppState) prefs.GetInt(PREFS_NOTIFICATION_APP_STATE, 2);

			if (hasNewNotification) {
				
				if (notificationAppState == AppState.Active)
				{
					return NotificationState.Foreground;
				}

				// Notification was received from the background or from a closed state.
				return NotificationState.Background;
			}

			return NotificationState.None;
		}
		#endregion

		// Resets the SharedPreferences data to reflect the system having no current notification.
		private void ResetNotificationData() { 
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(_applicationContext);
			ISharedPreferencesEditor editor = prefs.Edit();
			editor.Remove(PREFS_NOTIFICATION_MESSAGE);
			editor.Remove(PREFS_NOTIFICATION_TIMESTAMP);
			editor.Remove(PREFS_NOTIFICATION_APP_STATE);
			editor.PutBoolean(NotificationScheduler.PREFS_NEW_NOTIFICATION, false);
			editor.Apply();
		}

        private string GetNotificationMessage() {
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(_applicationContext);
            return prefs.GetString(PREFS_NOTIFICATION_MESSAGE, AppGlobals.NOTIFICATION_MESSAGE);
		}

		private Int64 GetMillisFromIsoString(String isoString) {
			DateTime time = DateFormatHelper.ParseIsoFormattedString(isoString);
			double milliseconds = 
				time.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
			
			return Convert.ToInt64(milliseconds);
		}

		private ActivitySession GetActivityFromNotificationData() { 

			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(_applicationContext);

			string isoTimestamp = prefs.GetString(PREFS_NOTIFICATION_TIMESTAMP, null);

			DateTime notificationTimestamp = DateFormatHelper.ParseIsoFormattedString(isoTimestamp);

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
					return null;
				}
				// NOTE: if the activity is null then there was not a video that we could use.
				// The app does not penalize users for not watching videos that they don't have yet.

			}

			return activity;
		}
	}
}

