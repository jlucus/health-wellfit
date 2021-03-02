using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using StoreKit;
using Xamarin.Forms;

using ObjCRuntime;

using WellFitPlus.Mobile;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Database;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitPlus.Mobile.Services;
using WellFitPlus.Mobile.iOS.DependencyServices;
using WellFitPlus.Mobile.iOS.PlatformViews;
using WellFitPlus.Mobile.Helpers;
using WellFitPlus.Mobile.Abstractions;

namespace WellFitPlus.Mobile.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate  //: UIApplicationDelegate
        : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {

		#region Static Fields
		public static int ActivityId;

		private static bool canPurchase = false; // Set upon startup.
		#endregion

		#region Static Properties
		public static bool DeviceCanMakePurchases { 
			get {
				return canPurchase;
			}
		}
		#endregion

		#region Private Fields
		private Services.ActivityService _activityService;
		private DependencyServices.NotificationService _notificationService;
		#endregion

		#region Public Fields
		public UIInterfaceOrientationMask currentOrientation = UIInterfaceOrientationMask.Portrait;
		#endregion

		#region Properties
		public ActivityService ActivityServiceInstance { 
			get {
				return _activityService;
			}
		}
		#endregion

		#region Overrides
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {

			// make sure the local database exists
			var db = new LocalDatabaseContext();
			db.Initialize(false);// True if you would like to drop the tables

			// Start Xamarin Forms here so we can use DependencyService
			global::Xamarin.Forms.Forms.Init();


			_notificationService = 
				DependencyService.Get<INotificationScheduler>() as DependencyServices.NotificationService;

			// Is this device authorized to make payments?
			if (SKPaymentQueue.CanMakePayments)
			{
				AppDelegate.canPurchase = true;
				IAPManager.GetInstance().SetupInAppPurchases();
			}
            
            // Check OS Version
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                // Set Notification Settings
                var notificationSettings = UIUserNotificationSettings.GetSettingsForTypes(
                    UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound, null
                );

                // Register This App For iOS Notifications
                app.RegisterUserNotificationSettings(notificationSettings);
            }

			// Instantiate Activity Service
			_activityService = ActivityService.Instance;

            // Subscribe To Share Event
            MessagingCenter.Subscribe<string>(this, AppGlobals.Events.SHARE, message =>
            {
                this.Share();
            });

			// Local notifications will be processed here if the app was in a closed state and opened via notification
			if (options != null && options.ContainsKey(UIApplication.LaunchOptionsLocalNotificationKey))
			{
				var localNotification = options[UIApplication.LaunchOptionsLocalNotificationKey] as UILocalNotification;
				if (localNotification != null)
				{
					_notificationService.SetAutoOpenVideoNotificationFromLocalNotification(localNotification);
				}
				else { 
					// This will do nothing if there wasn't a missed notification.
					_notificationService.SetAutoOpenVideoNotificationFromMissedNotification();
				}
			}
			else {
				// We aren't opening from a notification so make sure if there was one that we go directly to
				// Video playback page upon launch.
				_notificationService.SetAutoOpenVideoNotificationFromMissedNotification();
			}

			// Clear out the notification badge upon app initializations
			UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;

			// Load the application after any potential notifications have been handled.
			LoadApplication(new App());

			return base.FinishedLaunching(app, options);
        }
        
		// The local notification will be processed here if the app is in the foreground or background.
        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
			App.Log("AppDelegate: Recieved Local Notification");
			App.Log("AppDelegate: Is Video Playback playing: " + VideoPlaybackRenderer.IsVideoPlaybackActive);
			// Sanity check in case the notification has been set inside WillEnterForegroundd
			var test = _notificationService.HasRecievedNotification();
			if (_notificationService.HasRecievedNotification() == NotificationState.None)
			{
				if (application.ApplicationState != UIApplicationState.Active)
				{
					// Auto login functionality
					_notificationService.SetAutoOpenVideoNotificationFromLocalNotification(notification);

					ProcessNonActiveStateNotification();
				}
				else {
					// App is active and in the foreground, notify the user with a pop-up.
					ProcessLocalNotification(notification);
				}
			}
        }

        public override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            // Inform system of fetch results
            completionHandler(UIBackgroundFetchResult.NewData);
        }

		public override void OnActivated(UIApplication uiApplication)
		{
			base.OnActivated(uiApplication);

 			// Sanity Check in case a swiped notification was already set inside ReceivedLocalNotificationn
			if (_notificationService.HasRecievedNotification() == NotificationState.None)
			{
				// Set the necessary ScheduledNotification from the iOS NotificationService.
				_notificationService.SetAutoOpenVideoNotificationFromMissedNotification();

				ProcessNonActiveStateNotification();
			}
		}
		#endregion

		#region Methods

        private void Share()
        {
            // Set data to share
            var activityController = new UIActivityViewController(new NSObject[] {
                //UIActivity.FromObject( <UIIMAGE_OBJECT> ),
                UIActivity.FromObject( AppGlobals.General.WELLFIT_PLUS_APP_STORE_LINK ),
            }, null);
            // Get controller to handle share process
            var topController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            while (topController.PresentedViewController != null)
            {
                topController = topController.PresentedViewController;
            }
            // Show share options
            topController.PresentViewController(activityController, true, null);
        }

		/// <summary>
		/// This method is used to handle ONLY a recieved notification while the user is currently using the app
		/// (i.e. the app is currently in an Active state while the notification was received.)
		/// </summary>
		/// <param name="notification">The local notification.</param>
		private void ProcessLocalNotification(UILocalNotification notification) {
			// Save the notification data so we can retrieve it from the Xamarin Forms side..
			_notificationService.SetNewNotificationData(notification);

			// Notify the Xamarin Forms side that there is a new notification that needs to be processed.
			MessagingCenter.Send<object>(this, AppGlobals.NOTIFICATION_MESSAGE);

			App.Log("AppDelegate: App recieved notification while open.");
		}

		/// <summary>
		/// This method is used to handle any notification (missed or swiped) when the app is becoming active from
		/// a closed or backgrounded state.
		/// </summary>
		private void ProcessNonActiveStateNotification() { 
			//MessagingCenter.Send<object>(this, AppGlobals.NOTIFICATION_MESSAGE);

			// Using the Shared NotificationService we are navigating to the Video Playback screen.
			Services.NotificationService notificationService = Services.NotificationService.Instance;

			var notificationState = notificationService.HasRecievedNotification();

			if (notificationState == NotificationState.Background)
			{
				notificationService.StartVideoPlaybackIfNewNotificationExists();
			}
		}

		/// Returns whether or not a user should have access to new data. The user should have access to new content
		/// if they are within the 14 day trial period or if they have purchased a subscription.
		/// </summary>
		/// <returns><c>true</c>, if user should have access to new content <c>false</c> otherwise.</returns> 
		public bool IsSubscribedOrInTrialPeriod()
		{
			var settings = Models.UserSettings.GetExistingSettings();
			DateTime userRegistrationDate = settings.RegistrationDate;

			int trialPeriodLength = AppGlobals.TRIAL_PERIOD_DURATION; // In Dayss
			bool isWithinTrialPeriod = false;
			bool isSubscribed = false;

			// Check if the user is within their trial period
			DateTime now = DateTime.Now;
			DateTime trialExpirationDate = userRegistrationDate.AddDays(trialPeriodLength);
			if (now.CompareTo(trialExpirationDate) < 0)
			{
				isWithinTrialPeriod = true;
			}

			// Check to see if the user has puchased a subscription
			var iapService = new IAPService();
			isSubscribed = iapService.HasPurchasedSubscription();

            return isWithinTrialPeriod || isSubscribed;
		}

        #endregion

		#region Orientation
		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow forWindow)
		{
			return currentOrientation;
		}
		#endregion
    }
}
