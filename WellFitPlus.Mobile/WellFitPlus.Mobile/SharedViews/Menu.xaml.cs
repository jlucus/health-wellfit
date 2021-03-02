using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Database;
using WellFitPlus.Mobile.Services;
using WellFitPlus.Mobile.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace WellFitPlus.Mobile.SharedViews
{
	public partial class Menu : ContentPage
	{

		#region Private Fields
		InAppPurchase iapManager = DependencyService.Get<InAppPurchase>();
		INotificationScheduler notificationManager = DependencyService.Get<INotificationScheduler>();
		private NotificationService _notificationService = NotificationService.Instance;
		#endregion

		#region Initialization

		public Menu ()
		{
			InitializeComponent ();
            //Apply Safe Area
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
            this.BackgroundImage = AppGlobals.Images.BLUE_GRADIENT_IMAGE;

            this.profileButton.Clicked += this.ProfileButton_Clicked;
            this.videoButton.Clicked += this.VideoHistoryButton_Clicked;
            this.backButton.Clicked += this.BackButton_Clicked;
            this.notificationButton.Clicked += this.NotificationButton_Clicked;
            this.settingsButton.Clicked += this.SettingsButton_Clicked;
            this.accountButton.Clicked += this.AccountButton_Clicked;
            this.logOutButton.Clicked += this.LogOutButton_Clicked;
            this.aboutButton.Clicked += this.AboutButton_Clicked;
            this.faqButton.Clicked += this.FAQButton_Clicked;
            this.feedbackButton.Clicked += this.FeedbackButton_Clicked;
            this.termsButton.Clicked += this.TermsAndConditionsButton_Clicked;            
            this.fullVersionButton.Clicked += this.UpgradeButton_Clicked;
			this.restorePurchaseButton.Clicked += this.RestorePurchasesButton_Clicked;
            this.shareButton.Clicked += new EventHandler((o, e) =>
                MessagingCenter.Send("User Shared", "SHARE"));

			emailLogsButton.Clicked += SendLogEmailButton_Clicked;

			fullVersionButton.IsVisible = !iapManager.HasPurchasedSubscription();
			//EmailLogger.Instance.AddLog("Menu: isSubscribed = " + iapManager.HasPurchasedSubscription());

#if __ANDROID__
            this.restorePurchaseButton.IsVisible = false;
#endif

        }

		#endregion

		#region Page Lifecycle methods
		protected override void OnAppearing()
		{
			base.OnAppearing();

			// If we are sitting on this page we should still get notifications prompted
			MessagingCenter.Subscribe<object>(this, AppGlobals.NOTIFICATION_MESSAGE, (sender) =>
			{
				_notificationService.AlertUserIfNewNotificationExists();
			});

            // TODO: PRODUCTION: V1. Uncomment when the app is ready to go live on the stores.
            //fullVersionButton.IsVisible = !iapManager.HasPurchasedSubscription();

		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			MessagingCenter.Unsubscribe<object>(this, AppGlobals.NOTIFICATION_MESSAGE);
		}
		#endregion

		#region Events

        private void BackButton_Clicked(object sender, EventArgs e)
        {
             Navigation.PopAsync(true);
        }

        private void ProfileButton_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send<string>("", AppGlobals.Events.REFRESH_PROFILE);
            Navigation.PopAsync(true);
        }

        private void VideoHistoryButton_Clicked(object sender, EventArgs e)
        {
            var currentStackPage = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.FirstOrDefault();
            if (currentStackPage == null || currentStackPage.GetType() == typeof(VideoHistory)) { return; }

            VideoHistory history = new VideoHistory();
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(history, false);
            Navigation.PushAsync(history);
        }

        private void NotificationButton_Clicked(object sender, EventArgs e)
        {
            var currentStackPage = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.FirstOrDefault();
            if (currentStackPage == null || currentStackPage.GetType() == typeof(Notifications)) { return; }

            var notifications = new Notifications();
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(notifications, false);
            Navigation.PushAsync(notifications);
        }

        private void SettingsButton_Clicked(object sender, EventArgs e)
        {
            var currentStackPage = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.FirstOrDefault();
            if (currentStackPage == null || currentStackPage.GetType() == typeof(Settings)) { return; }

            var settings = new Settings();
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(settings, false);
            Navigation.PushAsync(settings);
        }

        private void AccountButton_Clicked(object sender, EventArgs e)
        {
            var currentStackPage = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.FirstOrDefault();
            if (currentStackPage == null || currentStackPage.GetType() == typeof(Account)) { return; }

            var account = new Account();
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(account, false);
            Navigation.PushAsync(account);
        }

        private async void LogOutButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                WebApiService service = new WebApiService(SessionService.Instance.Configuration);
                var result = await service.Logout().ConfigureAwait(false);
                if (result == System.Net.HttpStatusCode.OK)
                {

					CacheHelper.ClearCache();

					// Remove the previous user's credentials
					UserCredentials creds = new UserCredentials();
					creds.DeleteCredentials();

					// Cancel any scheduled notifications.
					notificationManager.CancelOSRegisteredNotifications();

                    // Delete database tables to get ready for the next user.e
                    new LocalDatabaseContext().Initialize(true); // Reset the database

                    // Navigate Back To Login
                    Device.BeginInvokeOnMainThread(() =>
                   {

						Xamarin.Forms.Application.Current.MainPage.Navigation.PopToRootAsync(false).ConfigureAwait(false);
						var login = new Login();
						Xamarin.Forms.NavigationPage.SetHasNavigationBar(login, false);
						Xamarin.Forms.Application.Current.MainPage = login;
                   });
                }
                else
                {

					Device.BeginInvokeOnMainThread(() => {
						DisplayAlert("Error", "Logout Failed. HTTP Code " + result.ToString(), "OK");
					});
                }
            }
            catch (Exception ex)
            {
				Device.BeginInvokeOnMainThread(() =>
				{
	                DisplayAlert("Error", "Logout Failed. Detail " + ex.Message, "OK");
				});
            }
        }

        private void AboutButton_Clicked(object sender, EventArgs e)
        {
            var currentStackPage = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.FirstOrDefault();
            if (currentStackPage == null || currentStackPage.GetType() == typeof(About)) { return; }

            var about = new About();
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(about, false);
            Navigation.PushAsync(about);
        }

        private void FeedbackButton_Clicked(object sender, EventArgs e)
        {
            var currentStackPage = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.FirstOrDefault();
            if (currentStackPage == null || currentStackPage.GetType() == typeof(Feedback)) { return; }

            var feedback = new Feedback();
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(feedback, false);
            Navigation.PushAsync(feedback);
        }

        private void FAQButton_Clicked(object sender, EventArgs e)
        {
            var currentStackPage = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.FirstOrDefault();
            if (currentStackPage == null || currentStackPage.GetType() == typeof(FAQ)) { return; }

            var faq = new FAQ();
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(faq, false);
            Navigation.PushAsync(faq);
        }

        private void TermsAndConditionsButton_Clicked(object sender, EventArgs e)
        {
            var currentStackPage = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.FirstOrDefault();
            if (currentStackPage == null || currentStackPage.GetType() == typeof(TermsAndConditions)) { return; }

            var terms = new TermsAndConditions();
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(terms, false);
            Navigation.PushAsync(terms);
        }

        private void UpgradeButton_Clicked(object sender, EventArgs e)
        {
            var currentStackPage = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.FirstOrDefault();
            if (currentStackPage == null || currentStackPage.GetType() == typeof(Upgrade)) { return; }

            var upgrade = new Upgrade();
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(upgrade, false);
            Navigation.PushAsync(upgrade);
        }

		private void RestorePurchasesButton_Clicked(object sender, EventArgs e) 
		{
			iapManager.RestorePurchases();
			Navigation.PopAsync(true);
			
		}

		// This is for debugging purposes only. Mainly used to monitor Android in app billing
		private void SendLogEmailButton_Clicked(object sender, EventArgs e) {
			EmailLogger.Instance.SendLogsToEmail("rbarber.asg@gmail.com");
		}

		#endregion
    }
}
