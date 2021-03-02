using System;
using Xamarin.Forms;
using WellFitPlus.Mobile;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitPlus.Mobile.SharedViews;

namespace WellFitPlus.Mobile.PlatformViews
{
    public class VideoPlaybackPage : ContentPage
    {
		#region Private Fields
		private ActivitySession _activity;
		private Video _video;
		private bool _shouldNavigateToRoot;

		#endregion

		#region Properties
		// NOTE: the page renderer that this hosts can access these from the "Element" parameter of PageRenderer
		// The "Element" property is actually this ContentPage.
		public ActivitySession Activity { 
			get {
				return _activity;
			}
		}

		public Video Video { 
			get {
				return _video;
			}
		}
		#endregion

		public VideoPlaybackPage(ActivitySession activity, bool shouldNavigateToRoot = false)
        {
			_activity = activity;
			_video = new VideoRepository().GetVideo(_activity.VideoId);
			_shouldNavigateToRoot = shouldNavigateToRoot;
        }

		public void NavigateToRoot() {
			
			var profile = new Profile();
			NavigationPage.SetHasNavigationBar(profile, false);
			Application.Current.MainPage = new NavigationPage(profile);
		}
    }
}
