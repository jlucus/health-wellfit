using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WellFitPlus.Mobile.Services;
using WellFitPlus.Mobile.Abstractions;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace WellFitPlus.Mobile.SharedViews
{
	public partial class Upgrade : ContentPage
	{
		public const String PURCHASED_CALLBACK_MESSAGE = "Purchased Callback Message";

		#region Private Fields
		private InAppPurchase _iapManager;
		private INotificationScheduler _notificationScheduler;
		private NotificationService _notificationService = NotificationService.Instance;
		#endregion

		#region Initialization

		public Upgrade ()
		{
			InitializeComponent ();
			//Apply Safe Area
			On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
			_iapManager = DependencyService.Get<InAppPurchase>();
			_notificationScheduler = DependencyService.Get<INotificationScheduler>();

            this.backButton.Clicked += this.BackButton_Clicked;
			this.purchaseMonthlyButton.Clicked += this.PurchaseMonthlyButton_Clicked;
			this.purchaseYearlyButton.Clicked += this.PurchaseYearlyButton_Clicked;

        }

        #endregion

		#region Page Lifecycle methods
		protected override void OnAppearing()
		{
			MessagingCenter.Subscribe<object>(this, PURCHASED_CALLBACK_MESSAGE, (obj) =>
			{
				App.Log("re-enabling purchase buttons");
				((Button)purchaseMonthlyButton).IsEnabled = true;
				((Button)purchaseYearlyButton).IsEnabled = true;

				Navigation.PopAsync(true);
				
			});

			// If we are sitting on this page we should still get notifications prompted
			MessagingCenter.Subscribe<object>(this, AppGlobals.NOTIFICATION_MESSAGE, (sender) =>
			{
				_notificationService.AlertUserIfNewNotificationExists();
			});
		}

		protected override void OnDisappearing()
		{
			MessagingCenter.Unsubscribe<object>(this, PURCHASED_CALLBACK_MESSAGE);
			MessagingCenter.Unsubscribe<object>(this, AppGlobals.NOTIFICATION_MESSAGE);

		}


		#endregion

        #region Events

        private async void BackButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(true);
        }

		private void PurchaseMonthlyButton_Clicked(object sender, EventArgs e) {
			_iapManager.PurchaseMonthlySubscription();
			((Button)purchaseYearlyButton).IsEnabled = false;
			((Button)purchaseMonthlyButton).IsEnabled = false;

			App.Log("Disabling Monthly button");
		}

		private void PurchaseYearlyButton_Clicked(object sender, EventArgs e) {
			_iapManager.PurchaseYearlySubscription();
			((Button)purchaseYearlyButton).IsEnabled = false;
			((Button)purchaseMonthlyButton).IsEnabled = false;

			App.Log("Disabling Yearly button");
		}

		#endregion
    }
}
