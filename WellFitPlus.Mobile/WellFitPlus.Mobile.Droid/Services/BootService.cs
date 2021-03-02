using System;
using System.Linq;
using System.Collections.Generic;

using Xamarin.Forms;

using Android.App;
using Android.Widget;
using Android.Content;

using WellFitPlus.Mobile.Database;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitPlus.Mobile.Services;

namespace WellFitPlus.Mobile.Droid
{
	[Service]
	public class BootService : IntentService
	{
		public static readonly string EXTRA_DEVICE_BOOTING_RESCHEDULE = "com.asgrp.Mobile.WellFitPlus.BootReschedule";

		protected override void OnHandleIntent(Intent intent)
		{
			bool rescheduleNotificaitons = intent.GetBooleanExtra(EXTRA_DEVICE_BOOTING_RESCHEDULE, false);

			if (rescheduleNotificaitons)
			{
				// Temporarily initialize Xamarin Forms so we can use the NotificationService/
				// NOTE: The app will crash on startup without this.
				Forms.Init(this, null); 

				// make sure the local database exists
				var db = new LocalDatabaseContext();
				db.Initialize(false);// True if you would like to drop the tables

				WellFitPlus.Mobile.Services.NotificationService.Instance.RescheduleNotifications();
			}
		}

	}
}
