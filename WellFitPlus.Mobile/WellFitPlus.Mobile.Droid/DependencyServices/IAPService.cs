using System;
using WellFitPlus.Mobile.Droid;

using WellFitPlus.Mobile.Droid.InAppPurchase;
using WellFitPlus.Mobile.Models;

[assembly: Xamarin.Forms.Dependency(typeof(IAPService))]
namespace WellFitPlus.Mobile.Droid
{
    public class IAPService : WellFitPlus.Mobile.InAppPurchase
    {
	        
        public bool CanMakePurchases()
        {
			return IAPManager.Instance.Connected;
        }

        public bool HasPurchasedSubscription()
        {

			// TODO: PRODUCTION: Remove below line of code when the payment back door is no longer needed
			return true;

			var settings = UserSettings.GetExistingSettings();

			if (!string.IsNullOrEmpty(settings.CompanyName))
			{
				// This user is subscribed under a company. They have a subscription
				return true;
			}

			var iapManager = IAPManager.Instance;

            return iapManager.IsSubscribed();
        }

        public void PurchaseYearlySubscription()
        {
			var iapManager = IAPManager.Instance;

			
            iapManager.PurchaseYearlySubscription();
        }

        public void PurchaseMonthlySubscription()
        {
            var iapManager = IAPManager.Instance;

            iapManager.PurchaseMonthlySubscription();
        }

        public void RestorePurchases()
        {
            // Android doesn't use this functionality
        }

		public bool IsSubscribedOrInTrialPeriod() {

			// TODO: PRODUCTION: remove below line of code when payment backdoor is no longer needed
			return true;

			var iapManager = IAPManager.Instance;

			var settings = Models.UserSettings.GetExistingSettings();
			DateTime userRegistrationDate = settings.RegistrationDate;

			int trialPeriodLength = AppGlobals.TRIAL_PERIOD_DURATION; // In Dayss
			bool isWithinTrialPeriod = false;
			bool isSubscribed = iapManager.IsSubscribed();

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