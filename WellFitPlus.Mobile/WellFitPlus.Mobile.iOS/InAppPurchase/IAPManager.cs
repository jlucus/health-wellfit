using System;
using System.Linq;

using Xamarin.Forms;

using WellFitPlus.Mobile.SharedViews;

using Foundation;
using StoreKit;
using UIKit;

namespace WellFitPlus.Mobile.iOS
{

	/// <summary>
	/// Singleton class responsible for processing in app purchases.
	/// </summary>
	public class IAPManager: SKProductsRequestDelegate, ISKPaymentTransactionObserver, ISKRequestDelegate
	{

		private const String SUBSCRIPTION_MONTHLY_PRODUCT_ID = "com.wellfitplus.monthlysubscription";
		private const String SUBSCRIPTION_YEARLY_PRODUCT_ID = "com.wellfitplus.yearlysubscription";

		#region Static Fields
		private static IAPManager _instance;
		#endregion

		#region Static Methods
		public static IAPManager GetInstance() {
			if (_instance == null) {
				_instance = new IAPManager();
			}
			return _instance;
		}
		#endregion

		#region Public Fields
		public SKProductsRequest request;
		public NSArray products;
		#endregion

		#region Private Fields
		private bool _restoringPurchases;
		#endregion

		private IAPManager()
		{
		}

		public void SetupInAppPurchases() {
			ValidateProductIdentifiers(this.GetProductIdentifiers());

			SKPaymentQueue.DefaultQueue.AddTransactionObserver(this);

			// Check the transactions everytime the user starts the app. This is for cancellations of subscriptions.
			VerifyReceipt();
			//RestorePurchases(false);
		}

		#region Convenience Methods
		public bool IsSubscribed() {
			bool subscribedMonthly = NSUserDefaults.StandardUserDefaults.BoolForKey(SUBSCRIPTION_MONTHLY_PRODUCT_ID);
			bool subscribedYearly = NSUserDefaults.StandardUserDefaults.BoolForKey(SUBSCRIPTION_YEARLY_PRODUCT_ID);

			return subscribedMonthly || subscribedYearly;
		}

		public void PurchaseYearlySubscription() {
			if (products != null)
			{
				// Make sure that we get the correct item from the products NSArray.
				for (nuint i = 0; i < products.Count; i++) {
					SKProduct prod = products.GetItem<SKProduct>(i);
					if (prod.ProductIdentifier == SUBSCRIPTION_YEARLY_PRODUCT_ID) {
						CreatePaymentRequestForProduct(prod);
						break;
					}
				}
			}
			else { 
				// Error. We don't have products for some reason.
			}
		}

		public void PurchaseMonthlySubscription()
		{
			if (products != null)
			{
				// Make sure that we get the correct item from the products NSArray.
				for (nuint i = 0; i < products.Count; i++)
				{
					SKProduct prod = products.GetItem<SKProduct>(i);
					if (prod.ProductIdentifier == SUBSCRIPTION_MONTHLY_PRODUCT_ID)
					{
						CreatePaymentRequestForProduct(prod);
						break;
					}
				}
			}
			else {
				// Error. We don't have products for some reason.
			}
		}
		#endregion

		#region Product functionality
		// Get product identifiers
		public NSArray GetProductIdentifiers() {
			var identifiers = new NSMutableArray();

			identifiers.Add(new NSString(SUBSCRIPTION_MONTHLY_PRODUCT_ID));
			identifiers.Add(new NSString(SUBSCRIPTION_YEARLY_PRODUCT_ID));

			return identifiers as NSArray;
		}

		// Retrieve product information from Apple. Calls SKProductsRequestDelegate callback method
		public void ValidateProductIdentifiers(NSArray identifiers) {
			var productIdentifiers = new NSSet(identifiers);
			var productRequest = new SKProductsRequest(productIdentifiers);
			this.request = productRequest;
			productRequest.Delegate = this;
			productRequest.Start();
		}
		#endregion

		#region Purchasing Products
		/// <summary>
		/// Call this method to start the purchasing process for the product passed in as the parameter.
		/// </summary>
		/// <param name="product">Product.</param>
		public void CreatePaymentRequestForProduct(SKProduct product) {
			var payment = new SKMutablePayment();
			payment.ProductIdentifier = product.ProductIdentifier;
			payment.Quantity = 1;

			// Submit payment request to payment queue (this also handles pending payments and offline states)
			SKPaymentQueue.DefaultQueue.AddPayment(payment);
		}
		#endregion

		#region Verify Receipt
		public void VerifyReceipt(SKPaymentTransaction transaction = null) {
			NSUrl receiptURL = NSBundle.MainBundle.AppStoreReceiptUrl;
			NSData receipt = NSData.FromUrl(receiptURL);

			if (receipt != null)
			{
				// Create a JSON object that will be sent to Apple to verify receipt
				string receiptData = receipt.GetBase64EncodedString(NSDataBase64EncodingOptions.None);
				var objects = new NSObject[] { 
					NSObject.FromObject(receiptData),
			        NSObject.FromObject("3958d8bd837d4cb085e1e04b7a04388f")
				};
				var keys = new NSObject[] { 
					NSObject.FromObject("receipt-data"),
			        NSObject.FromObject("password")
				};
				NSDictionary jsonData = NSDictionary.FromObjectsAndKeys(objects, keys);

				NSError jsonError;
				NSData requestData = NSJsonSerialization.Serialize(jsonData, NSJsonWritingOptions.PrettyPrinted, out jsonError);

				// Build URL request
				// TODO: PRODUCTION: set the verification to Apples live url.
				NSUrl storeURL = NSUrl.FromString("https:/sandbox.itunes.apple.com/verifyReceipt");// Development
				//NSUrl storeURL = NSUrl.FromString("https://buy.itunes.apple.com/verifyReceipt");// Production

				NSMutableUrlRequest request = new NSMutableUrlRequest(storeURL);
				request.HttpMethod = "Post";
				request.Body = requestData;

				NSUrlSession session = NSUrlSession.SharedSession;
				NSUrlSessionTask task = session.CreateDataTask(request, (NSData data, NSUrlResponse response, NSError error) => 
				{
					NSError taskError;
					NSDictionary responseData =
					NSJsonSerialization.Deserialize(data, NSJsonReadingOptions.MutableLeaves, out taskError) as NSDictionary;

					if (responseData != null)
					{
						NSNumber status = responseData.ObjectForKey(NSObject.FromObject("status")) as NSNumber;
						// A status of 0 means a valid receipt
						if (status != null && status.ToString() == "0")
						{
							NSDictionary receiptDict = responseData.ValueForKey(new NSString("receipt")) as NSDictionary;
							NSArray latestReceiptDict = responseData.ValueForKey(new NSString("latest_receipt_info")) as NSArray;


							if (latestReceiptDict != null)
							{
								// Update the latest receipt info.
								this.ValidatePurchaseArray(latestReceiptDict);
							}
							else {
								NSArray purchases = receiptDict.ValueForKey(new NSString("in_app")) as NSArray;
								if (purchases != null)
								{
									this.ValidatePurchaseArray(purchases);
								}
								else {
									Console.WriteLine("IAP: Error while parsing receipt purchases");
								}
							}

							// If we have a valid receipt (status == 0 )
							if (transaction != null) {
								SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
							}

						}
						else {
							// Receipt is invalid
							Console.WriteLine("IAP: Receipt is invalid");
							// If there is a status that is not 0 then show it
							if (status != null) {
								Console.WriteLine("IAP: Error Receipt Status: " + status);
							}
						}
					}
					else {
						Console.WriteLine("IAP: There was an error getting receipt verification response.");
					}

				});

				task.Resume();
			}
			else {
				// Receipt does not exist
				Console.WriteLine("IAP: No Receipt");
			}
		}
		#endregion

		public void ValidatePurchaseArray(NSArray purchases) {

			for (nuint i = 0; i < purchases.Count; i++) {

				//(new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(startdatetime));
				NSDictionary purchase = purchases.GetItem<NSDictionary>(i);
				NSString productId = purchase.ValueForKey(new NSString("product_id")) as NSString;
				NSString expiresDateMSString = purchase.ValueForKey(new NSString("expires_date_ms")) as NSString;
				NSString cancellationString = purchase.ValueForKey(new NSString("cancellation_date")) as NSString;

				if (cancellationString != null) {
					// This product has been cancelled by the user.
					this.LockPurchasedFunctionalityForProductIdentifier(productId);
				}
				else if (expiresDateMSString != null)
				{
					DateTime expiresDate = 
						(new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(expiresDateMSString.ToString()));

					if (this.IsDateExpired(expiresDate))
					{
						// Expired subscription
						this.LockPurchasedFunctionalityForProductIdentifier(productId);
					}
					else {
						// Active Subscription
						this.UnlockPurchasedFunctionalityForProductIdentifier(productId);
					}
				}
				else {
					this.UnlockPurchasedFunctionalityForProductIdentifier(productId.ToString());
				}
			}
		}

		public bool IsDateExpired(DateTime expiresDate) {
			bool isExpired = false;
			DateTime now = DateTime.Now;

			int status = now.CompareTo(expiresDate);

			if (status >= 0) {
				isExpired = true;
			}

			return isExpired;
		}

		#region Unlocking/Locking purchased functionality
		/// <summary>
		/// Sets a boolean value in local storage indicating that the product with the passed in 
		/// product ID has been unlocked/purchased
		/// </summary>
		/// <param name="productId">Product identifier.</param>
		public void UnlockPurchasedFunctionalityForProductIdentifier(string productId) {
			NSUserDefaults.StandardUserDefaults.SetBool(true, productId);
			NSUserDefaults.StandardUserDefaults.Synchronize();

			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false; // Sanity Check
		}

		/// <summary>
		/// Sets a boolean value in local storage indicating that the product with the passed in 
		/// product ID has been locked. (ex. cancelled subscription)
		/// </summary>
		/// <param name="productId">Product identifier.</param>
		public void LockPurchasedFunctionalityForProductIdentifier(string productId)
		{
			NSUserDefaults.StandardUserDefaults.SetBool(false, productId);
			NSUserDefaults.StandardUserDefaults.Synchronize();

			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false; // Sanity Check
		}
		#endregion

		#region SKProductsRequestDelegate methods
		/// <summary>
		/// Response that gets the product objects from Apple.
		/// </summary>
		/// <param name="request">Request.</param>
		/// <param name="response">Response.</param>
		public override void ReceivedResponse(SKProductsRequest request, SKProductsResponse response)
		{
			this.products = NSArray.FromObjects(response.Products);
		}
		#endregion

		#region SKPaymentTransactionObserver Delegate methods
		public void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
		{
			for (int i = 0; i < transactions.Length; i++)
			{
				SKPaymentTransaction transaction = transactions[i];

				switch (transaction.TransactionState)
				{
					case SKPaymentTransactionState.Purchasing:
						UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
						continue;
					case SKPaymentTransactionState.Deferred:
						UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;

						// Re-enable purchasing buttons on the upgrade page
						MessagingCenter.Send<object>(this, Upgrade.PURCHASED_CALLBACK_MESSAGE);
						continue;
					case SKPaymentTransactionState.Failed:
						UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
						SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
						// TODO: Present error dialog to the user.

						// show an alert
						UIAlertController restoreAlert =
							UIAlertController.Create(
								"Failed",
								"Purchase Failed",
								UIAlertControllerStyle.Alert
							);

						restoreAlert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));
						UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(restoreAlert, true, null);

						// Re-enable purchasing buttons on the upgrade page
						MessagingCenter.Send<object>(this, Upgrade.PURCHASED_CALLBACK_MESSAGE);
						continue;
					case SKPaymentTransactionState.Purchased:
						// TODO: set any variables within the Xamarin Forms side and update the database
						//		 showing that the user has purchased this particular product

						this.VerifyReceipt(transaction);

						// Re-enable purchasing buttons on the upgrade page
						MessagingCenter.Send<object>(this, Upgrade.PURCHASED_CALLBACK_MESSAGE);
						continue;
					case SKPaymentTransactionState.Restored:
						// We don't really need this for subscriptions. We will only be refreshing the 
						// reciept from Apple to cross check the purchase.
						// This is normally used for consumable items.

						// Re-enable purchasing buttons on the upgrade page
						MessagingCenter.Send<object>(this, Upgrade.PURCHASED_CALLBACK_MESSAGE);
						continue;
					default:
						continue;
				}
			}

		}
		#endregion

		#region Restoring purchases
		public void RestorePurchases(bool shouldShowDialog) {
			var restoreRequest = new SKReceiptRefreshRequest();
			restoreRequest.Delegate = this;
			restoreRequest.Start();
			_restoringPurchases = shouldShowDialog;
		}

		public override void RequestFinished(SKRequest request)
		{
			// We only need to refresh the receipt to restore the purchase for auto-renewing subscriptions
			this.VerifyReceipt(); 

			// Check needed due to the system calling this delegate method unnecessarily
			if (_restoringPurchases)
			{
				// show an alert
				UIAlertController restoreAlert =
					UIAlertController.Create(
						"Restored",
						"Purchase Restored",
						UIAlertControllerStyle.Alert
					);

				restoreAlert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));
				UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(restoreAlert, true, null);
				_restoringPurchases = false;

			}
		}
		#endregion

	}

}

