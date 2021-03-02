using System;
using WellFitPlus.Mobile.Database;
using SQLite;


namespace WellFitPlus.Mobile
{
	public class FrequentVideo
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public Guid VideoID { get; set; }
	}
}

