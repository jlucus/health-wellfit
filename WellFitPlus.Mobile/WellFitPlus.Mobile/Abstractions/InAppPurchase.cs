using System;
namespace WellFitPlus.Mobile
{

	public delegate void IAPCallback();

	public interface InAppPurchase
	{
		bool CanMakePurchases();
		bool HasPurchasedSubscription();

		void PurchaseYearlySubscription();
		void PurchaseMonthlySubscription();

		void RestorePurchases();
		bool IsSubscribedOrInTrialPeriod();
	}
}

