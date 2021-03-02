using System;

using WellFitPlus.Mobile.iOS;
using Foundation;

[assembly: Xamarin.Forms.Dependency(typeof(PreferenceService))]
namespace WellFitPlus.Mobile.iOS
{
	public class PreferenceService: IPreferences
	{
		public PreferenceService()
		{
		}


		public void SetBool(string key, bool value) { 
			NSUserDefaults.StandardUserDefaults.SetBool(value, key);
			NSUserDefaults.StandardUserDefaults.Synchronize();
		}

		public void SetInt(string key, int value) {
			NSUserDefaults.StandardUserDefaults.SetInt(value, key);
			NSUserDefaults.StandardUserDefaults.Synchronize();

		}

		public void SetString(string key, string value) { 
			NSUserDefaults.StandardUserDefaults.SetString(value, key);
			NSUserDefaults.StandardUserDefaults.Synchronize();
		}


		/// <summary>
		/// Gets the bool.
		/// </summary>
		/// <returns>the value of the bool if it exists. Defaults to false if value doesn't exist.</returns>
		/// <param name="key">Key.</param>
		public bool GetBool(string key) { 
			return NSUserDefaults.StandardUserDefaults.BoolForKey(key);
		}

		/// <summary>
		/// Gets the int.
		/// </summary>
		/// <returns>The int or 0 if the value doesn't exist.</returns>
		/// <param name="key">Key.</param>
		public int GetInt(string key) {
			return (int) NSUserDefaults.StandardUserDefaults.IntForKey(key);
		}

		/// <summary>
		/// Gets the string.
		/// </summary>
		/// <returns>The string or null if the value didn't exist</returns>
		/// <param name="key">Key.</param>
		public string GetString(string key) {
			return NSUserDefaults.StandardUserDefaults.StringForKey(key);
		}
	}
}
