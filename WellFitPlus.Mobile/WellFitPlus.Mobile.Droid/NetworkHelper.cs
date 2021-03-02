
using Android.Content;
using Android.Net;
using Xamarin.Forms;

namespace WellFitPlus.Mobile.Droid
{
    public static class NetworkHelper
    {
        public static bool IsDeviceConnectedToWiFi()
        {
            ConnectivityManager connectivityManager = (ConnectivityManager)Forms.Context.GetSystemService(Context.ConnectivityService);
            NetworkInfo wifiInfo = connectivityManager.GetNetworkInfo(ConnectivityType.Wifi);

            if (wifiInfo.IsConnected)
            {
                App.Log(" Device Wifi connected.");
            }
            else
            {
                App.Log(" Device Is Wifi disconnected.");
            }

            return wifiInfo.IsConnected;
        }

        public static bool IsDeviceRoaming()
        {
            ConnectivityManager connectivityManager = (ConnectivityManager)Forms.Context.GetSystemService(Context.ConnectivityService);
            NetworkInfo mobileInfo = connectivityManager.GetNetworkInfo(ConnectivityType.Mobile);
            if (mobileInfo.IsRoaming && mobileInfo.IsConnected)
            {
                App.Log(" Device Is Roaming.");
            }
            else
            {
                App.Log(" Device Is Not roaming.");
            }

            return mobileInfo.IsRoaming && mobileInfo.IsConnected;
        }
    }
}