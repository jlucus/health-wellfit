
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Widget;

namespace WellFitPlus.Mobile.Droid
{
	[BroadcastReceiver]
	[IntentFilter(new[] { Intent.ActionBootCompleted })]
	public class BootReceiver : BroadcastReceiver
	{

		public override void OnReceive(Context context, Intent intent)
		{
			if ((intent.Action != null) 
			    && (intent.Action == Android.Content.Intent.ActionBootCompleted))
			{

				var serviceIntent = new Intent(context, typeof(BootService));
				serviceIntent.PutExtra(BootService.EXTRA_DEVICE_BOOTING_RESCHEDULE, true);


				context.ApplicationContext.StartService(serviceIntent);
			}
		}
	}
}
