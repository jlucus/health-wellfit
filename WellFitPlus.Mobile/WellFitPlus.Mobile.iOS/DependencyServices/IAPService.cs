using System;

using WellFitPlus.Mobile.iOS;
using WellFitPlus.Mobile.Models;
using Foundation;
using UIKit;


[assembly: Xamarin.Forms.Dependency(typeof(IAPService))]
namespace WellFitPlus.Mobile.iOS
{
	public class IAPService: InAppPurchase
	{
		public IAPService()
		{
		}

		/// <summary>
		/// Returns whether or not this phone can make purchases.
		/// </summary>
		/// <returns><c>true</c>, if make purchases was caned, <c>false</c> otherwise.</returns>
		public bool CanMakePurchases() {
			return AppDelegate.DeviceCanMakePurchases;
		}

		public bool HasPurchasedSubscription() {
            return true; // TODO: Remove this line of code when subscription backdoor is no longer needed

            var settings = UserSettings.GetExistingSettings();

            if (!string.IsNullOrEmpty(settings.CompanyName)) {
                // This user is subscribed under a company. They have a subscription
                return true;
            }

			if (!CanMakePurchases()) return false; // Make sure we do nothing if the phone can't make purchases

			IAPManager iapManager = IAPManager.GetInstance();
			bool testbool = iapManager.IsSubscribed();
			return iapManager.IsSubscribed();
		}

		public void PurchaseYearlySubscription() {
			if (!CanMakePurchases()) return; // Make sure we do nothing if the phone can't make purchases

			IAPManager iapManager = IAPManager.GetInstance();
			iapManager.PurchaseYearlySubscription();
		}

		public void PurchaseMonthlySubscription()
		{
			if (!CanMakePurchases()) return; // Make sure we do nothing if the phone can't make purchases

			IAPManager iapManager = IAPManager.GetInstance();
			iapManager.PurchaseMonthlySubscription();
		}

		public void RestorePurchases() {
			if (!CanMakePurchases()) return; // Make sure we do nothing if the phone can't make purchases

			IAPManager iapManager = IAPManager.GetInstance();

			iapManager.RestorePurchases(true);

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

	}
}

