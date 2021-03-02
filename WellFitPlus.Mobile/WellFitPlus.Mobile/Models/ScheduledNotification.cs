using System;
using WellFitPlus.Mobile.Database;
using SQLite;


namespace WellFitPlus.Mobile.Models
{
	/// <summary>
	/// Used to keep track of what notifications were scheduled and also to keep track of what notifications
	/// have been scheduled.
	/// </summary>
	public class ScheduledNotification
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public DateTime ScheduledTimestamp { get; set; }
        public string Message { get; set; }

	}
}

