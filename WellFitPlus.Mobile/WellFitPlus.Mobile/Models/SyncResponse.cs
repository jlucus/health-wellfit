using System;
using System.Collections.Generic;

namespace WellFitPlus.Mobile.Models
{
	public class SyncResponse
	{
		public List<Video> VideosToDownload { get; set; }
		public List<Video> VideosToDelete { get; set; }
		public List<Video> TopMostFrequentVideos { get; set; }
		public List<ServerActivitySession> AllUserActivities { get; set; }
	}
}

