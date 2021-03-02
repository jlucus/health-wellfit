
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WellFitPlus.Mobile.Droid.HelperClasses
{
    public struct NotificationData 
    {
        public int ActivityId { get; set; }
        public string Message { get; set; }
    }
}
