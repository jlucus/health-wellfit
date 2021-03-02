using System;
namespace WellFitPlus.Mobile.Utils
{
	/// <summary>
	/// This structure is only used for Android and contains the necessary fields to pass through
	/// MessagingCenter to the MainActivity in order to start the VideoPlayback page.
	/// </summary>
	public struct VideoPageData
	{
		public int activityId;
		public bool isBonusVideo;

		public VideoPageData(int id, bool isBonus) {
			activityId = id;
			isBonusVideo = isBonus;
		}
	}
}
