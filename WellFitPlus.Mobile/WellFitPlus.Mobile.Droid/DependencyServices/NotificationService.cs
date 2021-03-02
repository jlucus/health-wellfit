using System;
using System.Collections.Generic;

using Android.App;
using Android.Util;
using Android.Content;
using Android.Graphics;
using Android.Preferences;
using Android.Support.V4.App;

using Xamarin.Forms;

using WellFitPlus.Mobile.Abstractions;
using WellFitPlus.Mobile.Droid;
using WellFitPlus.Mobile.Services;
using WellFitPlus.Mobile.Database;
using WellFitPlus.Mobile.Database.Repositories;

namespace WellFitPlus.Mobile.Droid
{
    [Service]
    public class NotificationService : IntentService
    {
		#region Constants
		public const String APP_STATE_EXTRA = "com.asgrp.Mobile.WellFitPlus.app_state_extra";
		#endregion

		#region Static Fields
		public static readonly String NOTIFICATION_RECEIVED = "com.asgrp.Mobile.WellFitPlus.notification_received";
		#endregion

        #region Properties
        
        private static ActivityService _activityService;
        public static readonly int NotificationId = 1111;

        private DateTime _lastSyncNotificationTime = DateTime.MinValue;

        #endregion

        #region Private Fields
        private NotificationRepository _notificationRepo = NotificationRepository.Instance;
        #endregion

        protected override void OnHandleIntent(Intent intent)
        {

			// Timestamp from the original database backed ScheduledNotification
			string isoTimestamp = intent.GetStringExtra(NotificationScheduler.TIMESTAMP_EXTRA);
            DateTime timeStamp = DateFormatHelper.ParseIsoFormattedString(isoTimestamp);

            Models.ScheduledNotification scheduledNotification = _notificationRepo.GetNotification(timeStamp);

            string message;

            if (scheduledNotification != null && !string.IsNullOrEmpty(scheduledNotification.Message)) {
                message = scheduledNotification.Message;
            } else {
                message = AppGlobals.Notifications.NOTIFICATION_MESSAGE;
            }
            
            App.Log("WellFit Service Started.");

			var appStateTracker = AppStateTracker.Instance;
			AppState appState = appStateTracker.GetAppState();

			// First save the notification data to SharePreferences. The app will be checking
			// against this in order to process the notifiation.
			// NOTE: On Android the actual visual notification only opens the app. The MainActivity
			//		 handles the processing of the notification since there really isn't a 
			//		 notification object that gets passed back into the app after a notification tap
			SaveNotificationData(
				message,
				isoTimestamp,
				appState
			);

			if (appState == AppState.Closed || appState == AppState.Background)
			{

				Intent mainActivityIntent = new Intent(this, typeof(MainActivity));
				mainActivityIntent.PutExtra(NotificationScheduler.OPENED_FROM_NOTIFICATION_EXTRA, true);
				mainActivityIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
				PendingIntent pi = PendingIntent.GetActivity(this, 0, mainActivityIntent, 0);

				// NOTE: Any missed notifications are handled in the shared NotificationService
				var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;
				notificationManager.CancelAll(); // Remove all previous notifications from the notification drawer.

				// Instantiate the builder and set notification elements:
				NotificationCompat.Builder builder = new NotificationCompat.Builder(this)
					.SetTicker(message)
					.SetContentTitle(AppGlobals.Notifications.NOTIFICATION_TITLE)
					.SetContentText(message)
					.SetSmallIcon(Resource.Drawable.icon)
					.SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.icon))
					.SetAutoCancel(true)
                    .SetOngoing(false)
					.SetContentIntent(pi)
					.SetPriority((int)NotificationPriority.Max);

				// Build the notification:
				Notification notification = builder.Build();
				notification.Defaults |= NotificationDefaults.Vibrate;
				notification.Defaults |= NotificationDefaults.Sound;

				// Publish the notification
				notificationManager.Notify(0, notification);
			}
			else if (appState == AppState.Active) {
				// Use the Android NotificationScheduler dependency service to present the user
				// a dialog asking the user if they would like to watch the next video.
				INotificationScheduler notificationScheduler =
					DependencyService.Get<INotificationScheduler>();

				notificationScheduler.AlertUserIfNewNotificationExists();
			}
        }
        
        #region Methods

        private void DisplaySyncNotification()
        {
            try
            {             
                var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;
                
                App.Log("Creating Notification");
                
                // Instantiate the builder and set notification elements:
                NotificationCompat.Builder builder = new NotificationCompat.Builder(this)
                   .SetContentTitle("Well Fit Sync")
                    .SetContentText("Its been over 24 hours since the Well Fit app has downloaded fresh videos. Please login for the app to download fresh videos")
                    .SetSmallIcon(Resource.Drawable.wfpIcon);

                builder.SetPriority((int)NotificationPriority.High);
                
                // Setup an intent for VideoActivity:
                Intent secondIntent = new Intent(this, typeof(VideoActivity));

                // Pressing the Back button in VideoActivity exits the app:
                Android.Support.V4.App.TaskStackBuilder stackBuilder = Android.Support.V4.App.TaskStackBuilder.Create(this);
                stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(VideoActivity)));
                stackBuilder.AddNextIntent(secondIntent);
                const int pendingIntentId = 0;
                PendingIntent pendingContentIntent = stackBuilder.GetPendingIntent(pendingIntentId, (int)PendingIntentFlags.OneShot);
                builder.SetContentIntent(pendingContentIntent);

                // Build the notification:
                var notification = builder.Build();

                //if (notif.Vibrate)
                notification.Defaults |= NotificationDefaults.Vibrate;
                //if (notif.Sound)
                //    notification.Defaults |= NotificationDefaults.Sound;

                // Publish the notification:
                notificationManager.Notify(NotificationId, notification);

                App.Log("User Has Been Notified");

                System.Threading.Thread.Sleep(30000);
            }
            catch (Exception ex)
            {
                App.Log(string.Format("UNKNOWN ERROR on Notification Creation: {0}", ex.ToString()));
            }
        }

        /// <summary>
        /// Saves the notification data so we can use it from within the app since it is not
        /// guaranteed that we can/should use it within this service.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="isoTimestamp">Iso timestamp.</param>
        /// <param name="appState">App state.</param>
		private void SaveNotificationData(String message, String isoTimestamp, AppState appState) { 
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
			ISharedPreferencesEditor editor = prefs.Edit();
			editor.PutString(NotificationScheduler.PREFS_NOTIFICATION_MESSAGE, message);
			editor.PutString(NotificationScheduler.PREFS_NOTIFICATION_TIMESTAMP, isoTimestamp);
			editor.PutBoolean(NotificationScheduler.PREFS_NEW_NOTIFICATION, true);
			editor.PutInt(NotificationScheduler.PREFS_NOTIFICATION_APP_STATE, (int)appState);
			editor.Apply();
		}

        #endregion

    }
}