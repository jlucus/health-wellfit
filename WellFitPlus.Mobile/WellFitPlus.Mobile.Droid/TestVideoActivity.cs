using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Media;
using Xamarin.Forms;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitMobile.FileSystem.File.Entities;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Services;

namespace WellFitPlus.Mobile.Droid
{
    [Activity(Label = "WellFit Video")]
    public class TestVideoActivity : Activity
    {
        #region Properties

        private int _activityId;
        private bool _videoHasCompleted;
        private UserSettings _settings;
        public static bool VideoIsPlaying;
        public static bool VideoScreenOpen;

        private VideoView myVideoView;
        private TextView videoCompletedTextView;
        private TextView videoCompletedTextView2;
        private bool _hasUserReplayedVideo = false;
        
        private Android.Widget.Button videoCompletedButton;
        private ImageView replayImageView;
        private int position = 0;
        //private ProgressDialog progressDialog;
        private MediaController mediaControls;
        private MediaPlayer _mp;

        #endregion

        #region Initialization

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                RequestWindowFeature(WindowFeatures.NoTitle);                
                base.OnCreate(savedInstanceState);
                
                // Get the layout from video_main.xml
                SetContentView(Resource.Layout.activity_main);

                if (_videoHasCompleted == true) { return; }

                System.Threading.Thread.Sleep(1000);

                VideoIsPlaying = true;
                VideoScreenOpen = true;

                // Get User Settings
                this._settings = UserSettings.GetExistingSettings();
                Helpers.ActivityService.NotificationActive = false;

                // Get Activity Id
                this._activityId = Intent.Extras.GetInt(Mobile.Droid.Services.NotificationService.ACTIVITY_ID);

                // Cancel Notifications - User Acknowledged
                this.CancelNotifications();

                // Update Last Notification Session
                MessagingCenter.Send<string>(this._activityId.ToString(), AppGlobals.Events.ACTIVITY_ACKNOWLEDGED);

                if (mediaControls == null)
                {
                    mediaControls = new MediaController(this);
                    mediaControls.SoundEffectsEnabled = !this._settings.Mute;
                }

                // Find your VideoView in your video_main.xml layout
                myVideoView = (VideoView)FindViewById(Resource.Id.video_view);

                videoCompletedTextView = (TextView)FindViewById(Resource.Id.videoCompletedText);
                videoCompletedTextView2 = (TextView)FindViewById(Resource.Id.videoCompletedText2);
                videoCompletedButton = (Android.Widget.Button)FindViewById(Resource.Id.videoCompletedButton);
                videoCompletedButton.Click += new EventHandler((s, e) => OnBackPressed());

                replayImageView = (ImageView)FindViewById(Resource.Id.replayImageView);

                SetControlVisibility(ViewStates.Invisible);

                // Create a progressbar
                //progressDialog = new ProgressDialog(this);
                //// Set progressbar title
                //progressDialog.SetTitle("WellFit Exercise Video");
                //// Set progressbar message
                //progressDialog.SetMessage("Loading...");

                //progressDialog.SetCancelable(false);
                // Show progressbar
                //progressDialog.Show();

                try
                {
                    myVideoView.SetMediaController(mediaControls);

                    ActivitySessionRepository activityRepo = new ActivitySessionRepository();
                    var activity = activityRepo.GetActivity(this._activityId);

                    if (activity == null)
                    {
                        // WHY IS THIS HAPPENING?
                        OnBackPressed();
                    }

                    activity.StartTime = DateTime.Now;
                    activityRepo.UpdateActivity(activity);
                    
                    replayImageView.Click += new EventHandler((s, e) =>
                    {
                        _hasUserReplayedVideo = true;
                        VideoIsPlaying = true;
                        
                        var repo = new ActivitySessionRepository();
                        var act = repo.GetActivity(this._activityId);
                        act.Bonus = true;
                        repo.UpdateActivity(act);
                        MessagingCenter.Send<string>("", AppGlobals.Events.REFRESH_PROFILE);
                        SetControlVisibility(ViewStates.Invisible);
                        myVideoView.Start();
                    });


                    VideoRepository videoRepo = new VideoRepository();
                    var video = videoRepo.GetVideo(activity.VideoId);                    

                    var uri = Android.Net.Uri.Parse(video.FileName);

                    var videoText = (TextView)FindViewById(Resource.Id.videoText);
                    videoText.Text = video.Title;
                    videoText.TextSize = 28;
                    videoText.SetTextColor(Android.Graphics.Color.White);
                    videoText.SetTypeface(videoText.Typeface, Android.Graphics.TypefaceStyle.Bold);

                    App.Log("Watching Video " + video.Title);

                    // Check Video Exists
                    FileObject file = new FileObject(video.FileName);
                    if (file.Exists == false)
                    {
                        //progressDialog.Dismiss();
                        App.Current.MainPage.DisplayAlert("ERROR", "Video File '" + video.Title + "' Does Not Exist.", "OK").ConfigureAwait(false);
                        //OnBackPressed();
                    }
                    else
                    {
                        myVideoView.SetVideoURI(uri);
                        myVideoView.SoundEffectsEnabled = !this._settings.Mute;
                        //progressDialog.Dismiss();
                        myVideoView.Completion += new EventHandler(this.VideoCompleted);
                        myVideoView.Start();                        
                    }
                }
                catch (Exception e)
                {
                    Log.Debug("Error", e.Message);
                    //App.Current.MainPage.DisplayAlert("ERROR", e.Message, "OK");
                    //progressDialog.Dismiss();
                    OnBackPressed();
                }

                myVideoView.RequestFocus();
                _mp = new MediaPlayer();
                _mp.Prepared += new EventHandler(OnPrepared);

                // If Settings Set To Mute
                if (this._settings.Mute == true)
                {
                    // Set Volume Off
                    _mp.SetVolume(0, 0);
                }
            }
            catch (Exception ex)
            {
                // Try To Get Out Of Here - Something Happend
                // Try Not To Let Errored Out Stale Page Sit Blank - Screws Up Navigation & Throws Exceptions
                App.Instance.MainPage.DisplayAlert("ERROR", "An Error Occurred Attempting To Play The Video. Detail: " + ex.Message, "OK");
                OnBackPressed();
                //Finish();
            }
        }

        public override void Finish()
        {
            VideoIsPlaying = false;
            VideoScreenOpen = false;
            base.Finish();
        }

        // Close the progress bar and play the video
        public void OnPrepared(object sender, EventArgs e)//MediaPlayer mp)
        {
            try
            {
                //progressDialog.Dismiss();
                myVideoView.SeekTo(position);
                if (position == 0)                    
                {
                    SetControlVisibility(ViewStates.Invisible);
                    myVideoView.Start();
                }
                else
                {
                    myVideoView.Pause();
                }
            }
            catch (Exception ex)
            {
                OnBackPressed();
                //Finish();
            }
        }

        #endregion

        #region Events

        public override void OnBackPressed()
        {
            VideoIsPlaying = false;
            VideoScreenOpen = false;

            // Go TO Main Activity
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            StartActivity(intent);
            //base.OnBackPressed(); -> DO NOT CALL THIS LINE OR WILL NAVIGATE BACK
        }

        protected override void OnSaveInstanceState(Bundle savedInstanceState)
        {
            base.OnSaveInstanceState(savedInstanceState);
            savedInstanceState.PutInt("Position", myVideoView.CurrentPosition);
            myVideoView.Pause();
        }
        
        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
            position = savedInstanceState.GetInt("Position");
            myVideoView.SeekTo(position);

            if (_videoHasCompleted)
            {
                SetControlVisibility(ViewStates.Visible);
            }
        }

        private void SetControlVisibility(ViewStates state)
        {
            videoCompletedTextView.Visibility = state;
            videoCompletedTextView2.Visibility = state;
            videoCompletedButton.Visibility = state;
            
            replayImageView.Visibility = _hasUserReplayedVideo == false  && VideoIsPlaying == false ? ViewStates.Visible : ViewStates.Invisible;
        }

        private void VideoCompleted(object sender, EventArgs e)
        {
            _videoHasCompleted = true;
            VideoIsPlaying = false;
            SetControlVisibility(ViewStates.Visible);
            App.Log("Video Completed");

            // Trigger Update Activity
            MessagingCenter.Send<string>(this._activityId.ToString(), AppGlobals.Events.COMPLETE_ACTIVITY);
        }

        #endregion

        #region Functions

        private void CancelNotifications()
        {
            // Get the notification manager:
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;

            // Cancel Notification
            notificationManager.Cancel(Mobile.Droid.Services.NotificationService.NotificationId);
        }

        #endregion
    }
}