using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Content;
using Android.Preferences;
using Android.Util;

using Xamarin.Forms;
using Plugin.InAppBilling;

namespace WellFitPlus.Mobile.Droid.InAppPurchase
{
    /// <summary>
    /// Singleton class responsible for processing in app purchases.
    /// </summary>
    public class IAPManager
    {

		private const string TAG = "IAPManager";

		public enum BillingSupportedResponseType : int
		{
			RESULT_OK = 0,                      //- success
			RESULT_USER_CANCELED = 1,           //- user pressed back or canceled a dialog
			RESULT_SERVICE_UNAVAILABLE = 2,		//- Network connection is down
			RESULT_BILLING_UNAVAILABLE = 3,     //- this billing API version is not supported for the type requested
			RESULT_ITEM_UNAVAILABLE = 4,        //- requested SKU is not available for purchase
			RESULT_DEVELOPER_ERROR = 5,         //- invalid arguments provided to the API
			RESULT_ERROR = 6,                   //- Fatal error during the API action
			RESULT_ITEM_ALREADY_OWNED = 7,      //- Failure to purchase since item is already owned
			RESULT_ITEM_NOT_OWNED = 8,          //- Failure to consume since item is not owned
		}

		#region Constants
		private const string HAS_PURCHASED_SUBSCRIPTION_PREFS = "has_purchased_subscription";
		private const int GOOGLE_BILLING_VERSION = 3;
		#endregion

		#region Static Fields

        private static IAPManager _instance;

        #endregion

		#region Static Properties
		public static IAPManager Instance { 
			get {
				if (_instance == null) {
					_instance = new IAPManager();
				}

				return _instance;
			}
		}
		#endregion

        #region Private Fields

        // Well Fit Product IDs
        private const String SUBSCRIPTION_MONTHLY_PRODUCT_ID = "com.wellfitplus.monthly_subscription"; //"android.test.purchased";
        private const String SUBSCRIPTION_YEARLY_PRODUCT_ID = "com.wellfitplus.yearly_subscription"; //"android.test.purchased";

		// TODO: PRODUCTION: Change subscription product ID's to live
        //private const String SUBSCRIPTION_MONTHLY_PRODUCT_ID = "android.test.purchased";
        //private const String SUBSCRIPTION_YEARLY_PRODUCT_ID = "android.test.purchased";
        //private const String SUBSCRIPTION_YEARLY_PRODUCT_ID = "android.test.item_unavailable";

        private IInAppBilling _serviceBilling;
        private IEnumerable<InAppBillingProduct> _products;
        private const string _playStorePublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAmqWIcaP9LlSAdZ9jHgcBf9hKoV3MMSp6kP7M0KE5yypvH/vK0ayLmVyRGGi9a77jA0VeLE1LuuTC/lKzAMn0A4dEh4j8r3jveL/rpSnWbWEMueEqLT2SiK5z2QWGYxMufsM5T9An7nXejXujYpUBcMRXTkpvNl9/5HwwGB3bUdBcZG9+bt3i9kixe3BakLtrrllM1Pq46LbPeRB/UQOh49Z2/Y/f3A6zX0GuJ/MBwPNRVdOzACiliGgKHLxEGA7iquSANLRIEYlTcFPCFZLptcfTJdFJwYRc8W5UvNZlbFNRKBD1u9XNF4UZwtE6cxnjRbbJVYOaq6mSY+lgS0ir4wIDAQAB";

		#endregion

		#region Properties

		public bool Connected => _serviceBilling != null;
		public bool HasPurchasedSubscription { get; set; }
        #endregion

        private IAPManager()
		{
			// Make sure that we load the last state of the subscription value from phone storage
			HasPurchasedSubscription = GetHasPurchasedSubscriptionValue();
			Log.Info(TAG, "Starting IAPManager. Is user currently subsribed? => " + HasPurchasedSubscription);
			EmailLogger.Instance.AddLog(
				TAG + ": Starting IAPManager. Is user currently subsribed? => " + HasPurchasedSubscription);
		}
 
		#region Play Store Connection Management
		public async void ConnectToPlayStore() {

			if (_serviceBilling != null) {
				// We are already connected to the Google Play Store
				return;
			}

			Log.Info(TAG, "Connecting to the Google Play Store");
			EmailLogger.Instance.AddLog(TAG + ": Connecting to the Google Play Store");

			_serviceBilling = CrossInAppBilling.Current;
			
			var connected = await _serviceBilling.ConnectAsync();
			if(!connected)
            {
				_serviceBilling = null;
				App.Log("Failed to connect");
				return;
            }
			
			//// Load inventory or available products
			this.LoadProducts();

			// Finally we need to check the Google Play app and see if the user has an active subscription
			bool hasSubscription = CheckIfUserHasOwnedSubscription();
			SetHasPurchasedSubscriptionValue(hasSubscription);

		}

		public void DisconnectFromPlayStore() {
			if (_serviceBilling != null)
			{
				Log.Info(TAG, "Disconnecting from Google Play Store");
				EmailLogger.Instance.AddLog(TAG + ": Disconnecting from Google Play Store");
				_serviceBilling.DisconnectAsync();
			}
			CrossInAppBilling.Dispose();			
		}
		#endregion

        #region Purchase Methods

        /// <summary>
        /// Connects to the Google Play Service and gets a list of products that are available
        /// for purchase.
        /// </summary>
        /// <returns>The inventory.</returns>
        private async void LoadProducts()
        {
            // Ensure Service Is Connected
            if (_serviceBilling == null) { return; }

			Log.Info(TAG, "Loading Well Fit Products");
			EmailLogger.Instance.AddLog(TAG + ": Loading Well Fit Products");

			// Ask the open connection's billing handler to return a list of avilable products for the 
			// given list of items.
			// NOTE: We are asking for the Reserved Test Product IDs that allow you to test In-App
			// Billing without actually making a purchase.
			_products = await _serviceBilling.GetProductInfoAsync(ItemType.Subscription);
			if (_products != null)
			{
				foreach (var product in _products)
				{
					Log.Info(TAG, "Loaded product " + product.ProductId);
					EmailLogger.Instance.AddLog(TAG + ": Loaded product " + product.ProductId);
				}
			}
        }

        public void RestorePurchases(bool shouldShowDialog)
        {
            // Not Applicable For Android
        }

        public bool PurchaseYearlySubscription()
        {
			return PurchaseProductById(SUBSCRIPTION_YEARLY_PRODUCT_ID);
        }


		private bool PurchaseProductById(string productId)
		{
			// Ensure Service Is Connected And User Can Make Purchases
			if (_serviceBilling == null
			   )
			{
				// TODO: Android: Notify user that they cannot currently make purchases
				return false;
			}

			if (_products != null)
			{
				// Make sure that we get the correct item from the products list.
				foreach (var prod in _products)
				{

					Log.Info(TAG, "Purchasing Yearly Subscription");
					EmailLogger.Instance.AddLog(TAG + ": Purchasing Yearly Subscription");

					if (prod.ProductId == productId)
					{
						// Buy Yearly Subscription
						var result = _serviceBilling.PurchaseAsync(prod.ProductId, ItemType.Subscription).Result;
						var acknowledgedResult = _serviceBilling.AcknowledgePurchaseAsync(result.PurchaseToken).Result;
						OnProductPurchased(result);
						return acknowledgedResult;
					}
				}
			}
			else
			{
				// Error. We don't have products for some reason.
				App.Log("Error: No Products Purchased");
			}

			return false;
		}

		public bool PurchaseMonthlySubscription()
        {
			return PurchaseProductById(SUBSCRIPTION_MONTHLY_PRODUCT_ID);
		}

		/// <summary>
		/// Handles the purchase result from the MainActivity's OnActivityResult method.
		/// This is required to fully finalize the billing cycle for the Xamarin.InAppBilling plugin.
		/// </summary>
		/// <param name="requestCode">Request code.</param>
		/// <param name="resultCode">Result code.</param>
		/// <param name="data">Data.</param>

        public bool IsSubscribed()
        {
			return HasPurchasedSubscription;
        }

		#endregion

		#region Service Callback Methods 
		private void OnProductPurchased(InAppBillingPurchase purchase) {
			Log.Info(TAG, "Product " + purchase.ProductId + "purchased");
			EmailLogger.Instance.AddLog(TAG + ": Product " + purchase.ProductId + "purchased");

			LoadProducts();

			bool isSubscribed = CheckIfUserHasOwnedSubscription();
			SetHasPurchasedSubscriptionValue(isSubscribed);
		}
		#endregion

		/// <summary>
		/// Checks if user has owned subscription based upon what is returned from the Google Play app.
		/// </summary>
		/// <returns><c>true</c>, If user has owned subscription <c>false</c> otherwise.</returns>
		private bool CheckIfUserHasOwnedSubscription() { 
			// Ensure Service Is Connected
            if (_serviceBilling == null) { return false; }

			// Ask the open connection's billing handler to get any purchases
			var purchases = _serviceBilling.GetPurchasesAsync(ItemType.Subscription).Result;

			bool isSubscribed = false;

			if (purchases.Count() > 0)
			{
				foreach (var product in purchases)
				{
					if (product.State == PurchaseState.Purchased)
					{
						isSubscribed = true;
						Log.Info(TAG, "User has purchased product: " + product.ProductId);
						EmailLogger.Instance.AddLog(TAG + ": User has purchased product: " + product.ProductId);
					}
					else if (product.State == PurchaseState.Restored)
					{
						isSubscribed = true;
						Log.Info(TAG, "User has restored product: " + product.ProductId);
						EmailLogger.Instance.AddLog(TAG + ": User has restored product: " + product.ProductId);
					}

					else if (product.State == PurchaseState.Canceled) {
						Log.Info(TAG, "User has cancelled product: " + product.ProductId);
						EmailLogger.Instance.AddLog(TAG + ": User has cancelled product: " + product.ProductId);
					}
				}
			}

			return isSubscribed;
		}

		/// <summary>
		/// Sets the HasPurchasedSubscription property and also saves that value to phone storage for quick
		/// retrieval upon app startup.
		/// </summary>
		/// <param name="hasSubscription">If set to <c>true</c> has subscription.</param>
		private void SetHasPurchasedSubscriptionValue(bool hasSubscription) {
			var appContext = Android.App.Application.Context;
			ISharedPreferences sharedPrefs = PreferenceManager.GetDefaultSharedPreferences(appContext);
			ISharedPreferencesEditor editor = sharedPrefs.Edit();
			editor.PutBoolean(HAS_PURCHASED_SUBSCRIPTION_PREFS, hasSubscription);
			editor.Apply();

			HasPurchasedSubscription = hasSubscription;

			Log.Info(TAG, "Set subscription value to " + hasSubscription);
			EmailLogger.Instance.AddLog(TAG + ": Set subscription value to " + hasSubscription);
		}

		/// <summary>
		/// Returns a bool indicating whether a user has purchased a subscription or not.
		/// 
		/// This value was saved to phone storage so we don't have a delay upon startup while the app waits to 
		/// check the store for whether or not the user currently has a subscription.
		/// </summary>
		/// <returns><c>true</c>, if has purchased subscription value was gotten, <c>false</c> otherwise.</returns>
		private bool GetHasPurchasedSubscriptionValue() { 
			var appContext = Android.App.Application.Context;
			ISharedPreferences sharedPrefs = PreferenceManager.GetDefaultSharedPreferences(appContext);

			return sharedPrefs.GetBoolean(HAS_PURCHASED_SUBSCRIPTION_PREFS, false);
		}
    }
}

