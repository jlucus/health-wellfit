using System;
namespace WellFitPlus.Mobile
{
	public static class DateFormatHelper
	{
		public static string DateTimeToIsoFormat(DateTime dt)
		{
			return dt.ToString("yyy-MM-ddTHH:mm:ss");
		}

		public static DateTime ParseIsoFormattedString(string isoString)
		{
			return DateTime.Parse(isoString, null, System.Globalization.DateTimeStyles.None);
		}

	}
}

