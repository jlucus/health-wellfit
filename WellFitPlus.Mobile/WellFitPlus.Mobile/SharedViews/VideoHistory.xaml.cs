using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Utils;
using WellFitPlus.Mobile.PlatformViews;
using WellFitPlus.Mobile.Services;
using WellFitPlus.Mobile.Abstractions;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace WellFitPlus.Mobile
{
    public partial class VideoHistory : ContentPage
    {

        #region Private Fields
        private ActivityService _activityService = ActivityService.Instance;
        private ActivitySessionRepository _activitySessionRepo = ActivitySessionRepository.Instance;
        private ActivitySession _currentActivity;
        private VideoRepository _videoRepo = new VideoRepository();
        private FrequentVideoRepository _frequentVideoRepo = FrequentVideoRepository.Instance;
        private Video _upNextVideo;
        private List<Video> _videos;
        private IThumbnailGetter _thumbnailService = DependencyService.Get<IThumbnailGetter>();
        private INotificationScheduler notificationService = DependencyService.Get<INotificationScheduler>();
        private int _secondsToGetThumbnail = 12; // How many seconds into the video should we take the image from
        private bool _isActivityNew = false; // Did we create a new activity that is not yet in the repo?
        private bool _isPlayingVideo = false; // Should we unsubscribe from navigation event?
        private NotificationService _notificationService = NotificationService.Instance;
        #endregion

        public VideoHistory()
        {

            InitializeComponent();
            //Apply Safe Area
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
            this.BackgroundImage = AppGlobals.Images.BLUE_GRADIENT_IMAGE;
            menuBar.LeftButton.Clicked += MenuButton_Clicked;

            InAppPurchase iapManager = DependencyService.Get<InAppPurchase>();
            INotificationScheduler notificationManager = DependencyService.Get<INotificationScheduler>();

            UserSettings settings = UserSettings.GetExistingSettings();

            bool isSubscribed = iapManager.HasPurchasedSubscription() || settings.IsUserWithinTrialPeriod();

            List<ActivitySession> pendingActivities =
                _activitySessionRepo.GetActivities(DateTime.Today.AddDays(-1)).Where(a => a.IsPending == true).ToList();

            //EmailLogger.Instance.AddLog("VideoHistory: isSubscribed/inTrialPeriod = " + isSubscribed);

            // Verify which video should be shown (i.e. a new video from bingo or a previously viewed one)
            if (notificationManager.HasNotificationAuthorization() && isSubscribed)
            {
                // Check if we have any pending videos. 
                if (pendingActivities.Count > 0)
                {
                    this.SetupMainVideoWithPendingActivity(pendingActivities[0]);
                }
                else
                {
                    this.SetMainVideoAsNewVideo();
                }
            }
            else
            {
                // Even if the user has not subscribed they are still allowed to access videos that have already
                // been downloaded. Cycle through those.
                SetMainVideoAsNewVideo();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var frequentVideoRepo = _frequentVideoRepo;
            List<FrequentVideo> topMostFrequentVideos = frequentVideoRepo.GetFrequentVideos();

            List<Video> frequentVideos = new List<Video>();
            for (int i = 0; i < topMostFrequentVideos.Count; i++)
            {
                Video frequentVideo = _videoRepo.GetVideo(topMostFrequentVideos[i].VideoID);
                if (frequentVideo != null)
                {
                    frequentVideos.Add(frequentVideo);
                }
            }

            List<Video> lastFivePlayedVideos = _videoRepo.GetLastFivePlayedVideos();

            List<Video> lastTwoPlayedVideos = new List<Video>();
            if (lastFivePlayedVideos != null)
            {
                List<Guid> guidList = new List<Guid>();
                foreach (var video in frequentVideos)
                {
                    guidList.Add(video.ID);
                }

                for (int i = 0; i < lastFivePlayedVideos.Count; i++)
                {

                    if (!guidList.Contains(lastFivePlayedVideos[i].ID))
                    {
                        lastTwoPlayedVideos.Add(lastFivePlayedVideos[i]);

                        if (lastTwoPlayedVideos.Count >= 2)
                        {
                            break;
                        }
                    }
                }

            }

            _videos = new List<Video>();
            _videos.AddRange(frequentVideos);
            _videos.AddRange(lastTwoPlayedVideos);

            // Filter the videos that have actually been downloaded
            _videos = _videos.Where(v => v.DownloadedSuccessfully == true).ToList();

            SetVideoSelectionGrid();

            // Lastly we will be subscribing to the notification event
            // If we are sitting on this page we should still get notifications prompte
            MessagingCenter.Subscribe<object>(this, AppGlobals.NOTIFICATION_MESSAGE, (sender) =>
            {
                _notificationService.AlertUserIfNewNotificationExists();
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Unsubscribe from events
            MessagingCenter.Unsubscribe<object>(this, AppGlobals.NOTIFICATION_MESSAGE);

        }

        /// <summary>
        /// Adds the list of video's to the selection grid.
        /// 
        /// NOTE: This method utilizes the Xamarin.Forms Grid's auto-layout for row and column placement.
        /// 	  It also layers a button over an Image within each cell of the Grid Layout.
        /// </summary>
        private void SetVideoSelectionGrid()
        {
            if (_videos.Count > 0)
            {

                // 2 columns with varying rows
                int gridColumnIndex = 0; // needs to be 0 or 1
                int gridRowIndex = 0; // needs to keep incrementing

                videoSelectionGrid.Children.Clear(); // Sanity check. Make sure grid is empty.

                //
                for (int i = 0; i < _videos.Count; i++)
                {

                    gridColumnIndex = i % 2; // Need only two columns so values 0 and 1

                    // gridColumnIndex == 0 when we are starting a new row
                    if (i != 0 && gridColumnIndex == 0)
                    {
                        gridRowIndex++;
                    }

                    // The first cell child will set the size of the cell.
                    Image forcedSizeImage = new Image
                    {
                        WidthRequest = 150,
                        HeightRequest = 100,
                        BackgroundColor = Color.Black,
                        Opacity = 0.5
                    };

                    // Image that will go underneath the button
                    Image thumbnail = new Image
                    {
                        WidthRequest = 150,
                        HeightRequest = 100,
                        //Opacity = 0.25
                        Opacity = 0.75
                    };

                    // Create a new Button
                    Button button = new Button
                    {
                        Text = _videos[i].Title,
                        Font = Font.SystemFontOfSize(NamedSize.Medium),
                        FontSize = 14,
                        TextColor = Color.White,
                        BackgroundColor = Color.Transparent,
                        //HorizontalOptions = LayoutOptions.Center,
                        //VerticalOptions = LayoutOptions.CenterAndExpand,
                        WidthRequest = 150,
                        HeightRequest = 100
                    };

                    int j = i;
                    button.Clicked += delegate
                    {
                        VideoHistoryButtonClicked(_videos[j]);
                    };

                    // A bit of a Xamarin hack but we can layer views if we put them in the same cell.
                    videoSelectionGrid.Children.Add(forcedSizeImage, gridColumnIndex, gridRowIndex);
                    videoSelectionGrid.Children.Add(thumbnail, gridColumnIndex, gridRowIndex);
                    videoSelectionGrid.Children.Add(button, gridColumnIndex, gridRowIndex);


#if __IOS__
					thumbnail.Source = _thumbnailService.GetThumbnail(_videos[i], _secondsToGetThumbnail);
#elif __ANDROID__
                    WeakReference<Image> weakImage = new WeakReference<Image>(thumbnail);
                    _thumbnailService.SetThumbnail(weakImage, _videos[i], _secondsToGetThumbnail);
#endif
                }

            }
        }

        #region Button Clicks
        private async void MenuButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(true);
        }

        private async void PlayButtonClick(object sender, EventArgs e)
        {

            var answer = await DisplayAlert(
                "Start Scheduled Video?", "This video will count towards statistics. Do you want to play it?", "Yes", "No");
            if (answer == true)
            {
                Console.WriteLine("Play Button has been clicked");
                // Create New Activity
                _activityService.AcknowledgeActivity(_currentActivity);

                // Only add this activity to the repo if it's new.
                if (_isActivityNew)
                {
                    ActivitySessionRepository.Instance.AddActivity(_currentActivity);
                }

                // Navigation to the Video Playback page is platform specific.
#if __IOS__
				try 
	            {	        
	            	VideoPlaybackPage videoPage = new VideoPlaybackPage(_currentActivity, true);
	          		Xamarin.Forms.NavigationPage.SetHasNavigationBar(videoPage, false);
	          		await Navigation.PushAsync(videoPage);
	            }
	            catch (global::System.Exception ex)
	            {
                ex.ToString();
	            }
#elif __ANDROID__
                try
                {
                    var videoPageData = new VideoPageData(_currentActivity.Id, false);
                    MessagingCenter.Send<string, VideoPageData>("", AppGlobals.Events.VIDEO_HISTORY_PAGE, videoPageData);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
#endif
            }
        }

        private async void VideoHistoryButtonClicked(Video videoToPlay)
        {

            ActivitySession activity = new ActivitySession()
            {
                VideoId = videoToPlay.ID,
                StartTime = DateTime.Now,
                NotificationTime = DateTime.MinValue, // MinValue because this is not a scheduled activity
                Bonus = false,
                UserId = videoToPlay.UserID,
                Acknowledged = true,
                IsPending = false
            };

            ActivitySessionRepository.Instance.AddActivity(activity);

            // Navigation to the Video Playback page is platform specific.
#if __IOS__
				VideoPlaybackPage videoPage = new VideoPlaybackPage(activity, true);
				Xamarin.Forms.NavigationPage.SetHasNavigationBar(videoPage, false);
				await Navigation.PushAsync(videoPage);
#elif __ANDROID__
            var videoPageData = new VideoPageData(activity.Id, false);
            MessagingCenter.Send<string, VideoPageData>("", AppGlobals.Events.VIDEO_HISTORY_PAGE, videoPageData);
#endif


        }
        #endregion

        // Sets the activity for the main video and also sets the image thumbnail for it.
        private void SetupMainVideoWithPendingActivity(ActivitySession newActivity)
        {
            _isActivityNew = false;

            _currentActivity = newActivity;
            _upNextVideo = _videoRepo.GetVideo(_currentActivity.VideoId);

#if __IOS__
			selectedVideoImage.Source = _thumbnailService.GetThumbnail(_upNextVideo, _secondsToGetThumbnail);
#elif __ANDROID__
            WeakReference<Image> weakImage = new WeakReference<Image>(selectedVideoImage);
            _thumbnailService.SetThumbnail(weakImage, _upNextVideo, _secondsToGetThumbnail);
#endif
        }

        private void SetMainVideoAsNewVideo()
        {
            _isActivityNew = true;

            // If notifications are not active but we still have access to new content
            ActivitySession newActivity = _activityService.GetNewActivity();

            // newActivity will be null if there was an issue fetching a video
            if (newActivity != null)
            {
                _activityService.AcknowledgeActivity(newActivity);
                _currentActivity = newActivity;
                _upNextVideo = _videoRepo.GetVideo(_currentActivity.VideoId);

#if __IOS__
				selectedVideoImage.Source = _thumbnailService.GetThumbnail(_upNextVideo, _secondsToGetThumbnail);
#elif __ANDROID__
                WeakReference<Image> weakImage = new WeakReference<Image>(selectedVideoImage);
                _thumbnailService.SetThumbnail(weakImage, _upNextVideo, _secondsToGetThumbnail);
#endif

            }
        }

        // Not used anymore but kept in case we need this in the future.
        private void SetMainVideoAsLastViewedVideo()
        {
            _isActivityNew = true;

            // Only set the main video if there was a "last played" video.
            if (_videos.Count > 0)
            {

                // If notifications are turned off we don't want to add this activity to the stats
                // So if we give a DateTime.MinValue value they won't be shown in the stats. This will also
                // show what activities were run without notifications enabled.
                DateTime notificationTime = DateTime.MinValue;

                if (notificationService.HasNotificationAuthorization())
                {
                    notificationTime = DateTime.Now;
                }

                _currentActivity = new ActivitySession()
                {
                    VideoId = _videos[0].ID,
                    StartTime = DateTime.Now,
                    NotificationTime = notificationTime,
                    Bonus = true,
                    UserId = _videos[0].UserID,
                    Acknowledged = false,
                    IsPending = false
                };

                _upNextVideo = _videoRepo.GetVideo(_currentActivity.VideoId);

#if __IOS__
				selectedVideoImage.Source = _thumbnailService.GetThumbnail(_upNextVideo, _secondsToGetThumbnail);
#elif __ANDROID__
                WeakReference<Image> weakImage = new WeakReference<Image>(selectedVideoImage);
                _thumbnailService.SetThumbnail(weakImage, _upNextVideo, _secondsToGetThumbnail);
#endif
            }
            else
            {
                // if there are no videos to play then don't set anything and hide the play button
                selectedVideoButton.IsVisible = false;
            }

        }
    }
}

