using WellFitPlus.Mobile.PlatformViews;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using WellFitPlus.Mobile.iOS.PlatformViews;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitPlus.Mobile.SharedViews;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Services;
using Foundation;
using AVFoundation;
using AVKit;
using CoreMedia;
using CoreImage;
using ObjCRuntime;
using UIKit;
using System;

[assembly: ExportRenderer(typeof(VideoPlaybackPage), typeof(VideoPlaybackRenderer))]
namespace WellFitPlus.Mobile.iOS.PlatformViews
{
	public partial class VideoPlaybackRenderer : PageRenderer
	{

		// We needed a quick way to tell the native side that the video playback page is running.
		// Using a quick static variable for this. This is set in the LoadView and ViewDidDisappear methods
		private static bool IS_VIDEO_PLAYBACK_ACTIVE = false;

		// Only allow other files to read the active state
		public static bool IsVideoPlaybackActive { 
			get {
				return IS_VIDEO_PLAYBACK_ACTIVE;
			}
		}

		enum PlaybackState { 
			Initialized,
			Playing,
			Paused,
			Finished
		}

		#region Private Fields
		private const string OnVideoFinishedSelectorName = "VideoPlaybackFinishedWithNotification:";

		private bool _isPlaybackFinished;

		// UI Views
		private CIColor COLOR_TRANSPARENT = new CIColor(0, 0, 0, 0);
		private CIColor COLOR_BLACK_TRANSPARENT = new CIColor(0, 0, 0, 0.5F);
		private UIView _view;

        private Services.NotificationService _notificationService = Services.NotificationService.Instance;
		private VideoPlaybackPage _page;
		private ActivitySession _activity;
		private Video _video;
		private bool _didReplay; // Needed so replaying video doesn't adjust the Activity
		#endregion

		#region Public Fields
		AVPlayer _player;
		AVAsset _asset;
		AVPlayerItem _playerItem;
		AVPlayerViewController _avPlayerViewController;
		OrientationService _orientationService = new OrientationService();
		#endregion

		#region Initialization

		protected override void OnElementChanged(VisualElementChangedEventArgs e)
		{

			base.OnElementChanged(e);
			_page = Element as VideoPlaybackPage;
			Console.WriteLine("Activity ID: " + _page.Activity);

			// Get the activity ID from this renderer's content page
			_activity = _page.Activity;
			_video = _page.Video;
            
            // JEO - 8/10/2016 - Ensure User Is Authenticated If App Was Not Running Before Landing Here
            this.EnsureUserAuthenticated();
        }

		public override void LoadView()
		{

			base.LoadView();

			IS_VIDEO_PLAYBACK_ACTIVE = true;

			var xib = NSBundle.MainBundle.LoadNib("VideoPlayback", this, null);

			// Set the buttons
			PlaybackToggleButton.TouchUpInside += PlaybackToggleButton_TouchUpInside;
			PauseButton.TouchUpInside += PlaybackToggleButton_TouchUpInside;
			SoundToggleButton.TouchUpInside += SoundToggleButton_TouchUpInside;
			BackToWorkButton.TouchUpInside += BackToWorkButton_TouchUpInside;
			ReplayButton.TouchUpInside += ReplayButton_TouchUpInside;
			RewindButton.TouchUpInside += RewindButton_TouchUpInside;

			//FavoritesButton.TouchUpInside += FavoritesButton_TouchUpInside; // Future functionality

			// Perform any last minute styling
			BackToWorkButton.Layer.BorderColor = UIColor.FromRGB(232, 131, 45).CGColor;
			BackToWorkButton.Layer.BorderWidth = 2;
			BackToWorkButton.Layer.MasksToBounds = true;
			BackToWorkButton.Layer.CornerRadius = 25;
			UIImage newImage = UIImage.FromBundle("PlayIcon");
			PlayToggleImage.Image = newImage;

			string tip = DependencyService.Get<IPreferences>().GetString(AppGlobals.DAILY_TIP_MESSAGE_PREF);
			TipOfTheDayLabel.Text = tip == null ? "" : tip;

			// TODO: PRODUCTION: Remove below code when ready to publish. Make sure to also remove the UI element
			//TitleLabel.Text = _video.Title;
			TitleLabel.Hidden = true;


			_view = Runtime.GetNSObject(xib.ValueAt(0)) as UIView;

			SetPlaybackUI(PlaybackState.Initialized);
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			IS_VIDEO_PLAYBACK_ACTIVE = true;

			// Hide the navigation bar for this screen
			ViewController.ParentViewController.NavigationController.SetNavigationBarHidden(true, false);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			string pathToFile = ThumbnailService.CreatePathToFile(_video.GetFileNameWithFullPath());

			_asset = AVAsset.FromUrl(NSUrl.FromFilename(pathToFile));
			_playerItem = new AVPlayerItem(_asset);
			_player = new AVPlayer(_playerItem);

			// Mute the player if the "default to mute" settings is checked
			var settings = Models.UserSettings.GetExistingSettings();
			if (settings.Mute) {
				_player.Muted = false; // Sanity check to allow the toggle button to handle necessary functionality
				SoundToggleButton_TouchUpInside(this, EventArgs.Empty);
			}

			// Register the "onVideoFinished" handler
			NSNotificationCenter.DefaultCenter.AddObserver(this,
														   new Selector(OnVideoFinishedSelectorName),
														   AVPlayerItem.DidPlayToEndTimeNotification,
														   _player.CurrentItem);

			// Set up the Sub-ViewController that will handle video playback and screen rotation.
			_avPlayerViewController = new AVPlayerViewController();
			_avPlayerViewController.Player = _player;
			_avPlayerViewController.View.Frame = View.Frame;
			_avPlayerViewController.ShowsPlaybackControls = false;
			_avPlayerViewController.VideoGravity = AVLayerVideoGravity.ResizeAspectFill;
			this.AddChildViewController(_avPlayerViewController);

			_avPlayerViewController.DidMoveToParentViewController(this);

			_view.Frame = _avPlayerViewController.View.Frame;
			_avPlayerViewController.View.AddSubview(_view);

			View.AddSubview(_avPlayerViewController.View);

			PlaybackToggleButton_TouchUpInside(this, EventArgs.Empty);
		}

		public override void ViewDidDisappear(bool animated)
		{
			// We are re-setting the active flag in two spots due to inconsistencies
			IS_VIDEO_PLAYBACK_ACTIVE = false;
			base.ViewDidDisappear(animated);
		}

		public override void ViewWillDisappear(bool animated)
		{

			// Clean up video content so there are no memory leaks. It was found that if we did not null out
			// the video content that iOS would only play up to 16 videos and then never play the videos until a
			// hard restart of the app occured.s
			_video = null;
			_asset = null;
			_player = null;
			_playerItem = null;
			_avPlayerViewController.RemoveFromParentViewController();
			_avPlayerViewController = null;

			// We are re-setting the active flag in two spots due to inconsistencies
			IS_VIDEO_PLAYBACK_ACTIVE = false;
			base.ViewDidDisappear(animated);
		}

		#endregion

		#region Events

		private void SoundToggleButton_TouchUpInside(Object sender, EventArgs e)
		{
			if (_player.Muted)
			{
				UIImage newImage = UIImage.FromBundle("SoundOn");
				SoundToggleButton.SetBackgroundImage(newImage, UIControlState.Normal);
				_player.Muted = false;

			}
			else {
				UIImage newImage = UIImage.FromBundle("SoundOff");
				SoundToggleButton.SetBackgroundImage(newImage, UIControlState.Normal);
				_player.Muted = true;

			}
		}

		private void PlaybackToggleButton_TouchUpInside(Object sender, EventArgs e)
		{

			if ((_player.Rate != 0) && (_player.Error == null))
			{
				// We are coming from a playing state
				SetPlaybackUI(PlaybackState.Paused);
				_player.Pause();
			}
			else {
				// We are coming from a paused state
				SetPlaybackUI(PlaybackState.Playing);

				// We are just begining. Set the start time in the current Activity if not replaying.
				if (_player.CurrentTime.Value == 0 && _didReplay == false) {

                    // JEO - 8/10/2016 - Update Activity Start Time On Playback Start
                    ActivitySessionRepository activityRepo
                        = ActivitySessionRepository.Instance;
                    _activity.StartTime = DateTime.Now;
                    activityRepo.UpdateActivity(_activity);
                }

				_player.Play();
			}
		}

		private void RewindButton_TouchUpInside(Object sender, EventArgs e) {
			
			CMTime currentTime = _player.CurrentTime;
			int timeScale = currentTime.TimeScale;
			int secondsToRewind = 10 * timeScale;

			// TODO: PRODUCTION: Uncomment below code when ready to publish
			long value = currentTime.Value - secondsToRewind >= 0 ? currentTime.Value - secondsToRewind : 0;

			//TODO: PRODUCTION: Comment out below code when ready to publish
			//long value = currentTime.Value + (60 * timeScale);

			CMTime rewindTime = new CMTime(value, timeScale);
			_player.Seek(rewindTime);
		}

		private void BackToWorkButton_TouchUpInside(Object sender, EventArgs e)
		{
			_page.NavigateToRoot();

			if (_isPlaybackFinished)
			{
				MessagingCenter.Send<ActivitySession>(
					this._activity,
					AppGlobals.Events.COMPLETE_ACTIVITY
				);
			}
			else if (_activity.NotificationTime == DateTime.MinValue) {
				// This is a pure bonus that the user has not finished. Do not count this in our activities list.
				ActivitySessionRepository.Instance.RemoveActivity(_activity);

			}

        }

		private void ReplayButton_TouchUpInside(Object sender, EventArgs e)
		{
			_didReplay = true;
			_isPlaybackFinished = false;

			// Go back to the beginning of the video.
			_player.Pause();
			CMTime start = new CMTime(0, 1);
			_player.Seek(start);

			// Reset the UI for playback
			SetPlaybackUI(PlaybackState.Playing);

            // JEO - 8/10/2016 - This Is To Update The Recently Added "ViewCount" Property On The Video Model
            var videoRepo = new Database.Repositories.VideoRepository();
            _video.ViewCount += 1;
            videoRepo.UpdateVideo(_video);

            MessagingCenter.Send<string>("", AppGlobals.Events.REFRESH_PROFILE);

            _player.Play();
		}

		private void FavoritesButton_TouchUpInside(Object sender, EventArgs e)
		{
			// Do something with the Favorites
		}

		[Export(OnVideoFinishedSelectorName)]
		private void VideoPlaybackFinished(NSNotification notification)
		{
			_isPlaybackFinished = true;
			SetPlaybackUI(PlaybackState.Finished);
			if (_didReplay == false)
			{
				_activity.EndTime = DateTime.Now;

				if (_activity.NotificationTime == DateTime.MinValue)
				{
					// This is a bonus activity and not part of the scheduled sessions.
					_activity.Bonus = true;
				}
			}
			else {
				_activity.Bonus = true; // We have replayed a scheduled video.
			}

			MessagingCenter.Send<ActivitySession>(this._activity, AppGlobals.Events.COMPLETE_ACTIVITY);
		}

        #endregion

        #region Convenience Functionality

        // JEO - 8/10/2016 - Ensure User Is Authenticated If App Was Not Running Before Landing Here
        private async void EnsureUserAuthenticated()
        {
            // Load Credentials
            UserCredentials credentials = new UserCredentials();

            try
            {
                // Verify User Authenticated
                if (Mobile.Services.SessionService.Instance == null || Mobile.Services.SessionService.Instance.AuthToken == null
                    || Mobile.Services.SessionService.Instance.Expires < DateTime.Now)
                {
                    // Log Debug Info
                    App.Log("Logging User In");
                    App.Log("Configuration Exists: " + (Mobile.Services.SessionService.Instance.Configuration == null ? "FALSE" : "TRUE"));
                    App.Log("Username Exists: " + (credentials.UserName == null ? "FALSE" : "TRUE"));
                    App.Log("Password Exists: " + (credentials.Password == null ? "FALSE" : "TRUE"));

                    // Login User
                    Mobile.Services.WebApiService service = new Mobile.Services.WebApiService(Mobile.Services.SessionService.Instance.Configuration);
                    var token = await service.Login(credentials.UserName, credentials.Password).ConfigureAwait(false);

                    // Set Authentication Information
                    Mobile.Services.SessionService.Instance.AuthToken = token.AccessToken;
                    Mobile.Services.SessionService.Instance.Issued = Convert.ToDateTime(token.IssuedAt);
                    Mobile.Services.SessionService.Instance.Expires = Convert.ToDateTime(token.ExpiresAt);
                }
            }
            catch (Exception ex)
            {
                App.Log("Exception Thrown Logging User In. Detail: " + ex.Message);
            }
        }

		private void SetPlaybackUI(PlaybackState state)
		{

			switch (state) {
				case PlaybackState.Initialized:
					PlaybackToggleButton.Hidden = false;
					PlayToggleImage.Hidden = false;
					SoundToggleButton.Hidden = false;
					BackToWorkButton.Hidden = false;
					SponsorImage.Hidden = false;

					TipOfTheDayLabel.Hidden = true;
					PauseButton.Hidden = true;
					PauseButtonShadow.Hidden = true;
					SoundToggleShadow.Hidden = true;
					RewindButton.Hidden = true;
					ReplayButton.Hidden = true;
					ReplayIcon.Hidden = true;
					_view.BackgroundColor = UIColor.FromCIColor(COLOR_BLACK_TRANSPARENT);

					break;

				case PlaybackState.Playing:
					PlaybackToggleButton.Hidden = false;
					PlayToggleImage.Hidden = true;
					PlayToggleImage.Image = null;
					PlaybackToggleButton.SetTitle("", UIControlState.Normal);
					SoundToggleButton.Hidden = false;
					PauseButton.Hidden = false;
					PauseButtonShadow.Hidden = false;
					SoundToggleShadow.Hidden = false;

					TipOfTheDayLabel.Hidden = true;
					SponsorImage.Hidden = true;
					RewindButton.Hidden = true;
					ReplayButton.Hidden = true;
					ReplayIcon.Hidden = true;
					BackToWorkButton.Hidden = true;
					_view.BackgroundColor = UIColor.FromCIColor(COLOR_TRANSPARENT);

					break;
				case PlaybackState.Paused:
					PlaybackToggleButton.Hidden = false;
					PlayToggleImage.Hidden = false;
					UIImage newImage = UIImage.FromBundle("PlayIcon");
					PlayToggleImage.Image = newImage;

                    //TODO: PRODUCTION: hide the bellow gui itemm
                    BackToWorkButton.Hidden = true; // We are hiding this when paused for now (requested by Well Fit 3/27/17)
					SoundToggleButton.Hidden = false;
					RewindButton.Hidden = false;
					SponsorImage.Hidden = false;
					TipOfTheDayLabel.Hidden = false;

					PauseButton.Hidden = true;
					PauseButtonShadow.Hidden = true;
					SoundToggleShadow.Hidden = true;
					ReplayButton.Hidden = true;
					ReplayIcon.Hidden = true;
					_view.BackgroundColor = UIColor.FromCIColor(COLOR_BLACK_TRANSPARENT);
					break;

				case PlaybackState.Finished:
					PlaybackToggleButton.Hidden = true;
					PlayToggleImage.Hidden = true;
					SoundToggleButton.Hidden = true;
					RewindButton.Hidden = true;

					PauseButton.Hidden = true;
					PauseButtonShadow.Hidden = true;
					SoundToggleShadow.Hidden = true;
					SponsorImage.Hidden = false;
					TipOfTheDayLabel.Hidden = false;
					ReplayButton.Hidden = false;
					ReplayIcon.Hidden = false;
					BackToWorkButton.Hidden = false;
					_view.BackgroundColor = UIColor.FromCIColor(COLOR_BLACK_TRANSPARENT);
					break;
				
			}
		}

		#endregion
	}  
}

