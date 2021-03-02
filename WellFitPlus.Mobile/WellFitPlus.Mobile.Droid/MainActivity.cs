using System;
using System.Linq;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Preferences;

using Xamarin.Forms;

using WellFitPlus.Mobile;
using WellFitPlus.Mobile.Database;
using WellFitPlus.Mobile.Abstractions;
using WellFitPlus.Mobile.Utils;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Services;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitPlus.Mobile.SharedViews;
using WellFitPlus.Mobile.Droid.InAppPurchase;
using WellFitPlus.Mobile.Droid.HelperClasses;
using Plugin.InAppBilling;
using Plugin.CurrentActivity;

namespace WellFitPlus.Mobile.Droid
{ 
    //MainLauncher = true,
    [Activity(Label = "WellFitPlus.Mobile", 
	          Icon = "@drawable/icon", 
			  ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, 
	          ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
		private const string TAG = "MainActivity";
		public const string NOTIFY_ACTIVE_USER_MESSAGE = "notify_active_user";
		public const int VIDEO_PLAYBACK_REQUEST_CODE = 3;

		private AppStateTracker _appStateTracker = AppStateTracker.Instance;
		private Services.NotificationService _notificationService;
		private ActivitySessionRepository _activitySessionRepo = ActivitySessionRepository.Instance;
		private IAPManager _iapManager = IAPManager.Instance;

		#region Lifecycle Methods

		protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

			Forms.Init(this, null);
			LoadApplication(new App());

			_notificationService = Services.NotificationService.Instance;

			// Subscribe to Video Playback page event. Called from the Xamarin.Forms side.
			MessagingCenter.Subscribe<string, VideoPageData>(
				this, 
				AppGlobals.Events.VIDEO_HISTORY_PAGE, 
				(str, videoPageData) =>
			{
				OpenVideoPlaybackPage(videoPageData);
			});

            // Subscribe to sharing event
            MessagingCenter.Subscribe<string>(this, AppGlobals.Events.SHARE, message =>
            {
                this.Share("Share", AppGlobals.General.WELLFIT_PLUS_APP_STORE_LINK);
            });

			// Subscribe to the notify active user event
			MessagingCenter.Subscribe<object, NotificationData>(this, NOTIFY_ACTIVE_USER_MESSAGE, (obj, data) => {
				PresentNotificationDialog(data);
			});

			CrossCurrentActivity.Current.Init(this, savedInstanceState);
			CrossCurrentActivity.Current.Activity = this;

			// Start connecting to the Google Play Store.
			_iapManager.ConnectToPlayStore();

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
			// We are coming back from the Video Playback screen. Reset Xamarin.Forms to 
			// the Profile page (now My Statistics)
			if (requestCode == VIDEO_PLAYBACK_REQUEST_CODE)
			{
				var profile = new Profile();
				NavigationPage.SetHasNavigationBar(profile, false);
				Xamarin.Forms.Application.Current.MainPage = new NavigationPage(profile);
			}
		}

		protected override void OnResume()
		{
			base.OnResume();



			_appStateTracker.SetAppState(AppState.Active, this);

			ISharedPreferences sharedPrefs = PreferenceManager.GetDefaultSharedPreferences(this);

			AppState appStateForNotification = 
				(AppState) sharedPrefs.GetInt(NotificationScheduler.PREFS_NOTIFICATION_APP_STATE, 0);

			NotificationState notificationState = _notificationService.HasRecievedNotification();

			if (notificationState == NotificationState.Background) {
				// This notification was received in a closed/backgrounded state.

				if (appStateForNotification == AppState.Background) {
					// The only state that we care about here is if the app received a notification while
					// it was in the background. 
					// Closed state is handled from the Xamarin.Forms Login page.
					// Active state is handled in the Android NotificationService.

					_notificationService.StartVideoPlaybackIfNewNotificationExists();
				}
			}
		}

		protected override void OnPause()
		{
			base.OnPause();

			_appStateTracker.SetAppState(AppState.Background, this);
		}

        protected override void OnDestroy()
        {
			// Unsubscribe from MessagingCenter eventss
			MessagingCenter.Unsubscribe<string, VideoPageData>(this, AppGlobals.Events.VIDEO_HISTORY_PAGE);
			MessagingCenter.Unsubscribe<string>(this, AppGlobals.Events.SHARE);
			MessagingCenter.Unsubscribe<object, NotificationData>(this, NOTIFY_ACTIVE_USER_MESSAGE);

			_appStateTracker.SetAppState(AppState.Closed, null);

			// We need to make sure that there is no open connection lingering.
			_iapManager.DisconnectFromPlayStore();

            base.OnDestroy();
        }

		#endregion

		public void Share(string title, string content)
		{
			//if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
			//    return;

			var name = this.Resources.GetResourceName(Resource.Drawable.locked).Replace(':', '/');
			//var imageUri = Uri.Parse("android.resource://" + name);
			var sharingIntent = new Intent();
			sharingIntent.SetAction(Intent.ActionSend);
			sharingIntent.SetType("image/*");
			sharingIntent.PutExtra(Intent.ExtraText, content);
			//sharingIntent.PutExtra(Intent.ExtraStream, imageUri);
			sharingIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
			this.StartActivity(Intent.CreateChooser(sharingIntent, title));
		}

		private void OpenVideoPlaybackPage(VideoPageData pageData) { 
			Android.Content.Intent videoIntent = new Android.Content.Intent(Forms.Context, typeof(WellFitPlus.Mobile.Droid.VideoActivity));
			videoIntent.PutExtra(VideoActivity.NOTIFICATION_ACTIVITY_ID_EXTRA, pageData.activityId);
			videoIntent.PutExtra(VideoActivity.IS_BONUS_VIDEO_EXTRA, pageData.isBonusVideo);
			StartActivityForResult(videoIntent, VIDEO_PLAYBACK_REQUEST_CODE);
		}

		private void PresentNotificationDialog(NotificationData data) {

			RunOnUiThread(() => 
			{ 

				var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;
				notificationManager.CancelAll(); // Remove all previous notifications from the notification drawer.

				var builder = new AlertDialog.Builder(this);
                VideoPageData pageData = new VideoPageData(data.ActivityId, false);

				builder.SetMessage(data.Message)
					   .SetTitle(AppGlobals.Notifications.NOTIFICATION_TITLE)
                       .SetCancelable(false)
					   .SetPositiveButton("OK", (sender, e) =>
					   {
						   OpenVideoPlaybackPage(pageData);
					   })
					   .SetNegativeButton("Cancel", (sender, e) =>
					   {

							// Get all of the activities
							List<ActivitySession> activities = _activitySessionRepo.GetActivities(DateTime.MinValue);

							ActivitySession activity =
                                activities.Where(a => a.Id == data.ActivityId).FirstOrDefault();

							if (activity == null)
							{
							   // Something weird happened. We should always have an activity here or else this code
							   // wouldn't get hit.
							   return;
							}

							var repo = ActivitySessionRepository.Instance;
							activity.IsPending = false;
							repo.UpdateActivity(activity);

							MessagingCenter.Send<string>("", AppGlobals.Events.REFRESH_PROFILE);
					   });

				AlertDialog dialog =  builder.Create();

				dialog.Show();
			});
		}
    }
}

