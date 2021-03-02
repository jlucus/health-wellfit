using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Media;
using Xamarin.Forms;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitMobile.FileSystem.File.Entities;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Services;
using Android.Preferences;
using Android.Text.Method;

namespace WellFitPlus.Mobile.Droid
{

    [Activity(Label = "WellFit Video", Name = "com.asgrp.Mobile.WellFitPlus.VideoActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@android:style/Theme.NoTitleBar")]
    public class VideoActivity : Activity
    {
		private enum VideoPlaybackState
		{
			Starting, // We just opened the video playback page and need to press play
			Playing,
			Paused,
			Completed // We have watched the entire video.
		}

		public const string NOTIFICATION_ACTIVITY_ID_EXTRA = "com.asgrp.Mobile.WellFitPlus.ActivityId";
		public const string IS_BONUS_VIDEO_EXTRA = "com.asgrp.Mobile.WellFitPlus.bonusVideoExtra";

        #region Private Fields

		// Information
        private UserSettings _settings;
        private ActivitySession _activity;
        private bool _hasUserReplayedVideo = false;
		private bool _isBonusVideo;
        private bool _soundIsOn = true;
		private AudioManager _audioManager;

        private Services.NotificationService _notificationService = Services.NotificationService.Instance;
		        
		// Controls
        private TextView _actionInfoText;
        private Android.Widget.Button _playButton;
		private Android.Widget.Button _pauseButton;
        private Android.Widget.Button _backToWorkButton;
        private Android.Widget.Button _replayImageView;
        private ImageView _videoOverlay;
		private Android.Widget.Button _soundToggleButton;
        private Android.Widget.Button _rewind;
        private ImageView _sponsorImageView;
        private VideoView _videoView;
        private MediaPlayer _mediaPlayer;
		private ImageView _soundToggleBackground;
		private ImageView _pauseButtonBackground;
		private TextView _tipOfTheDayText;
        #endregion

		#region Private Properties
		private VideoPlaybackState PlaybackState { get; set; }
		#endregion

		#region Lifecycle Methods

		protected override void OnCreate(Bundle savedInstanceState)
		{
			RequestWindowFeature(WindowFeatures.NoTitle);
			base.OnCreate(savedInstanceState);

			App.Log("Video Activity Started");

			// Load the views
			var inflater = this.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
			var view = inflater.Inflate(Resource.Layout.Video, null);
			SetContentView(view);

			// Set variables
			_settings = UserSettings.GetExistingSettings();
			_audioManager = (AudioManager)GetSystemService(Context.AudioService);
			_isBonusVideo = Intent.GetBooleanExtra(IS_BONUS_VIDEO_EXTRA, false);

			var activityId = Intent.Extras.GetInt(NOTIFICATION_ACTIVITY_ID_EXTRA);
			this._activity = ActivitySessionRepository.Instance.GetActivity(activityId);
			this._activity.StartTime = DateTime.Now;
			this._activity.Acknowledged = true;
			this._activity.IsPending = false;

			if (_isBonusVideo) {
				// This is a previously watched video, hence a bonus video. This means it has the default
				// notification time.
				_activity.NotificationTime = DateTime.MinValue;
			}

			ActivitySessionRepository.Instance.UpdateActivity(this._activity);

			// Load Page Controls
			this.LoadPageControls();

			// Prepare Video
			this.PrepareVideo();

			SetPlaybackState(PlaybackState);

		}

		protected override void OnResume()
		{
			base.OnResume();

			// Pause Video
			if (_videoView != null && PlaybackState == VideoPlaybackState.Paused)
			{

				SetPlaybackState(VideoPlaybackState.Paused);
				_videoView.Pause();
			}
			else {
				// We are starting the video for the first time so auto-play the video
				PlayButton_Click(this, EventArgs.Empty);
			}


		}

        protected override void OnPause()
        {
            base.OnPause();
            
            // Pause Video
			if (_videoView != null && PlaybackState == VideoPlaybackState.Playing)
            {
				
				SetPlaybackState(VideoPlaybackState.Paused);
				_videoView.Pause();
            }
        }

		protected override void OnDestroy()
		{
			base.OnDestroy();
			_mediaPlayer.Release();
		}

        #endregion

		#region Events
        
        private void PlayButton_Click(object sender, EventArgs e)
		{
			if (PlaybackState == VideoPlaybackState.Playing)
			{

				_videoView.Pause();
				SetPlaybackState(VideoPlaybackState.Paused);
			}
			else if (PlaybackState == VideoPlaybackState.Paused || PlaybackState == VideoPlaybackState.Starting)
			{
				SetPlaybackState(VideoPlaybackState.Playing);
				_videoView.Start();
			}
		}

		private void VideoRewind_Click(object sender, EventArgs e)
		{
			// TODO: PRODUCTION: uncomment below code to restore rewind functionality
			int position = _videoView.CurrentPosition - 15000;

			// TODO: PRODUCTION: comment out below code that fast forwards video
			//int position = _videoView.CurrentPosition + 15000;

			position = position < 0 ? 0 : position;

			_videoView.SeekTo(position);
		}

		private void SoundButton_Click(object sender, EventArgs e)
		{
			// Toggle the sound state
			_soundIsOn = !_soundIsOn;
			ToggleSound(_soundIsOn);
		}

		private void VideoView_Completed(object sender, EventArgs e)
		{
			SetPlaybackState(VideoPlaybackState.Completed);
			App.Log("Video Completed");

			// Update Activity
			_activity.EndTime = DateTime.Now;

			if (_hasUserReplayedVideo == false)
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

		private void BackToWorkButton_Click(object sender, EventArgs e)
		{

			if (PlaybackState == VideoPlaybackState.Completed)
			{
				MessagingCenter.Send<ActivitySession>(
					this._activity,
					AppGlobals.Events.COMPLETE_ACTIVITY
				);
			}
			else if (_activity.NotificationTime == DateTime.MinValue) {
				// This is a pure bonus that the user has not finished. Do not count this in our activities list
				ActivitySessionRepository.Instance.RemoveActivity(_activity);
			}

			Finish();
		}

		private void ReplayVideoButton_Click(object sender, EventArgs e)
		{
			// Set Flags
			_hasUserReplayedVideo = true;

			// Update Video
			var videoRepo = new VideoRepository();
			var vid = videoRepo.GetVideo(this._activity.VideoId);
			vid.ViewCount += 1;
			videoRepo.UpdateVideo(vid);

			SetPlaybackState(VideoPlaybackState.Playing);

			// Restart Video
			_videoView.Start();
		}

		#endregion

		private void LoadPageControls()
		{
			_videoView = (VideoView)FindViewById(Resource.Id.video_view);
			_videoOverlay = (ImageView)FindViewById(Resource.Id.videoOverlay);
			_playButton = (Android.Widget.Button)FindViewById(Resource.Id.play_button);
			_pauseButton = (Android.Widget.Button)FindViewById(Resource.Id.pause_button);
			_backToWorkButton = (Android.Widget.Button)FindViewById(Resource.Id.back_to_work_button);
			_actionInfoText = (TextView)FindViewById(Resource.Id.action_info_text);
			_replayImageView = (Android.Widget.Button)FindViewById(Resource.Id.replay_button);
			_soundToggleButton = (Android.Widget.Button)FindViewById(Resource.Id.sound_toggle_button);
			_rewind = (Android.Widget.Button)FindViewById(Resource.Id.rewind);
			_rewind.Visibility = ViewStates.Invisible;
			_sponsorImageView = (ImageView)FindViewById(Resource.Id.sponsorImageView);
			_sponsorImageView.Visibility = ViewStates.Invisible;
			_pauseButtonBackground = (ImageView)FindViewById(Resource.Id.pause_button_background);
			_soundToggleBackground = (ImageView)FindViewById(Resource.Id.sound_toggle_background);
			_tipOfTheDayText = (TextView)FindViewById(Resource.Id.tip_of_the_day_text);

			_mediaPlayer = new MediaPlayer();

			// On Video Touch
			_playButton.Click += this.PlayButton_Click;

			_pauseButton.Click += this.PlayButton_Click;

			// On Back To Work Button Clicked
			_backToWorkButton.Click += this.BackToWorkButton_Click;

			// On User Replay Video
			_replayImageView.Click += this.ReplayVideoButton_Click;

			// On Volume Touch
			_soundToggleButton.Click += this.SoundButton_Click;

			// On Video Rewind
			_rewind.Click += this.VideoRewind_Click;

			string tip = DependencyService.Get<IPreferences>().GetString(AppGlobals.DAILY_TIP_MESSAGE_PREF);
			_tipOfTheDayText.Text = tip == null ? "" : tip;

			_tipOfTheDayText.MovementMethod = new ScrollingMovementMethod();
		}

		private void PrepareVideo()
        {

			VideoRepository videoRepo = new VideoRepository();
			var video = videoRepo.GetVideo(this._activity.VideoId);
			var uri = Android.Net.Uri.Parse(video.FileName);

			App.Log("Watching Video " + video.Title);

			// Check Video Exists
			FileObject file = new FileObject(video.FileName);
			if (file.Exists == false)
			{
			    App.Current.MainPage.DisplayAlert(
					"ERROR", 
					"Video File '" + video.Title + "' Does Not Exist.", 
					"OK").ConfigureAwait(false);
			    return;
			}
			else
			{
			    _videoView.SetVideoURI(uri);
			    _videoView.SoundEffectsEnabled = !this._settings.Mute;
			    _videoView.Completion += VideoView_Completed;
			    _videoView.RequestFocus();
			}

			_soundIsOn = !this._settings.Mute;

			ToggleSound(_soundIsOn);
        }

		private void ToggleSound(bool shouldSoundBeOn) {

			if (shouldSoundBeOn){
				_audioManager.SetStreamMute(Stream.Music, false);
				_soundToggleButton.SetBackgroundResource(Resource.Drawable.iconVolOn);
			}
			else { 
				_audioManager.SetStreamMute(Stream.Music, true);
				_soundToggleButton.SetBackgroundResource(Resource.Drawable.iconVolOff);
			}
		}

		/// <summary>
		/// Sets the state of the playback and also handles what UI elements should be seen or hidden.
		/// </summary>
		/// <param name="playbackState">Playback state.</param>
		private void SetPlaybackState(VideoPlaybackState playbackState) {

			PlaybackState = playbackState;

			switch (playbackState) {
				case VideoPlaybackState.Starting:
					_playButton.SetBackgroundResource(Resource.Drawable.playicon);

					_sponsorImageView.Visibility = ViewStates.Visible;
					_backToWorkButton.Visibility = ViewStates.Visible;

					_tipOfTheDayText.Visibility = ViewStates.Invisible;
					_actionInfoText.Visibility = ViewStates.Invisible;
					_replayImageView.Visibility = ViewStates.Invisible;
					_rewind.Visibility = ViewStates.Invisible;
					_pauseButton.Visibility = ViewStates.Invisible;
					_pauseButtonBackground.Visibility = ViewStates.Invisible;
					_soundToggleBackground.Visibility = ViewStates.Invisible;

					_videoOverlay.Background.SetAlpha(150);
					break;
				case VideoPlaybackState.Playing:
					_playButton.SetBackgroundResource(0);

					_pauseButton.Visibility = ViewStates.Visible;
					_pauseButtonBackground.Visibility = ViewStates.Visible;
					_soundToggleBackground.Visibility = ViewStates.Visible;

					_tipOfTheDayText.Visibility = ViewStates.Invisible;
					_actionInfoText.Visibility = ViewStates.Invisible;
					_backToWorkButton.Visibility = ViewStates.Invisible;
					_replayImageView.Visibility = ViewStates.Invisible;
					_rewind.Visibility = ViewStates.Invisible;
					_sponsorImageView.Visibility = ViewStates.Invisible;

					_videoOverlay.Background.SetAlpha(0);
					break;
				case VideoPlaybackState.Paused:
					_playButton.SetBackgroundResource(Resource.Drawable.playicon);

					// We are hiding this for now (requested by Well Fit 3/27/17)
					// TODO: PRODUCTION: make below invisible
					_backToWorkButton.Visibility = ViewStates.Invisible;
					_rewind.Visibility = ViewStates.Visible;
					_sponsorImageView.Visibility = ViewStates.Visible;
					_tipOfTheDayText.Visibility = ViewStates.Visible;

					_pauseButton.Visibility = ViewStates.Invisible;
					_pauseButtonBackground.Visibility = ViewStates.Invisible;
					_soundToggleBackground.Visibility = ViewStates.Invisible;
					_replayImageView.Visibility = ViewStates.Invisible;
					_actionInfoText.Visibility = ViewStates.Invisible;

					_videoOverlay.Background.SetAlpha(150);

					break;
				case VideoPlaybackState.Completed:
					_playButton.SetBackgroundResource(0);

					_actionInfoText.Visibility = ViewStates.Visible;
					_backToWorkButton.Visibility = ViewStates.Visible;
					_replayImageView.Visibility = ViewStates.Visible;
					_sponsorImageView.Visibility = ViewStates.Visible;
					_tipOfTheDayText.Visibility = ViewStates.Visible;

					_rewind.Visibility = ViewStates.Invisible;
					_pauseButton.Visibility = ViewStates.Invisible;
					_pauseButtonBackground.Visibility = ViewStates.Invisible;
					_soundToggleBackground.Visibility = ViewStates.Invisible;

					_videoOverlay.Background.SetAlpha(150);              
					break;
			}
		}

	}
}