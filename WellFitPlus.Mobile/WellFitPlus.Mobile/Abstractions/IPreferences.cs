using System;
namespace WellFitPlus.Mobile
{
	public interface IPreferences
	{

		void SetBool(string key, bool value);

		void SetInt(string key, int value);

		void SetString(string key, string value);

		/// <summary>
		/// Gets the bool.
		/// </summary>
		/// <returns>the value of the bool if it exists. Defaults to false if value doesn't exist.</returns>
		/// <param name="key">Key.</param>
		bool GetBool(string key);

		/// <summary>
		/// Gets the int.
		/// </summary>
		/// <returns>The int or 0 if the value doesn't exist.</returns>
		/// <param name="key">Key.</param>
		int GetInt(string key);

		/// <summary>
		/// Gets the string.
		/// </summary>
		/// <returns>The string or null if the value didn't exist</returns>
		/// <param name="key">Key.</param>
		string GetString(string key);
	}
}
