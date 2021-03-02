using System;

using WellFitPlus.Mobile.Droid;

using Android.App;
using Android.Content;
using Android.Preferences;

[assembly: Xamarin.Forms.Dependency(typeof(PreferenceService))]
namespace WellFitPlus.Mobile.Droid
{
	public class PreferenceService: IPreferences
	{
		public PreferenceService()
		{
		}

		public void SetBool(string key, bool value) { 

			ISharedPreferences prefs = 
				PreferenceManager.GetDefaultSharedPreferences(Application.Context);

			ISharedPreferencesEditor editor = prefs.Edit();
			editor.PutBoolean(key, value);
			editor.Apply();
		}

		public void SetInt(string key, int value)
		{

			ISharedPreferences prefs =
				PreferenceManager.GetDefaultSharedPreferences(Application.Context);

			ISharedPreferencesEditor editor = prefs.Edit();
			editor.PutInt(key, value);
			editor.Apply();
		}

		public void SetString(string key, string value)
		{

			ISharedPreferences prefs =
				PreferenceManager.GetDefaultSharedPreferences(Application.Context);

			ISharedPreferencesEditor editor = prefs.Edit();
			editor.PutString(key, value);
			editor.Apply();
		}


		public bool GetBool(string key) { 
			ISharedPreferences prefs =
				PreferenceManager.GetDefaultSharedPreferences(Application.Context);

			return prefs.GetBoolean(key, false);
		}

		public int GetInt(string key) { 
			ISharedPreferences prefs =
				PreferenceManager.GetDefaultSharedPreferences(Application.Context);

			return prefs.GetInt(key, 0);
		}

		public string GetString(string key) { 
			ISharedPreferences prefs =
				PreferenceManager.GetDefaultSharedPreferences(Application.Context);

			return prefs.GetString(key, null);
		}

	}
}
