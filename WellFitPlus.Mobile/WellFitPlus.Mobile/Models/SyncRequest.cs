using System;
using System.Collections.Generic;

namespace WellFitPlus.Mobile.Models
{
	public class SyncRequest
	{
		public Guid UserID { get; set; }
		public float CacheSize { get; set; }
		public List<ServerActivitySession> ListOfNewActivities { get; set; }
		public List<Video> ListOfCurrentVideos { get; set; }
	}
}

