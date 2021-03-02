using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WellFitMobile.FileSystem.Directory.Entities;
using WellFitPlus.Mobile.Abstractions;
using WellFitPlus.Mobile.Controls;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Services;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace WellFitPlus.Mobile.SharedViews
{
    public partial class Settings : ContentPage
    {
        // Should we show a pop-up notifying the user why the settings page is in place?
        private const string SHOWN_INITIAL_DESCRIPTION_PREF = "show_popup_description";

        #region Properties

        private Xamarin.Forms.Picker _videoStartDelayTime;
        private Xamarin.Forms.Picker _cacheSize;

        #endregion

        #region Private Fields
        private NotificationService _notificationService = NotificationService.Instance;
        private IPreferences _userPreferences = DependencyService.Get<IPreferences>();
        private bool _userHasSeenDescriptionPopup = false;
        #endregion

        #region Initialization

        public Settings()
        {
            InitializeComponent();
            //Apply Safe Area
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
            this.menuBar.LeftButton.Clicked += this.MenuButton_Clicked;
            this.menuBar.RightButton.Clicked += this.SaveButton_Clicked;
            this.clearCacheButton.Clicked += this.ClearCacheButton_Clicked;

            this._cacheSize = new Xamarin.Forms.Picker()
            {
                Title = "Max Size",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.StartAndExpand,
                TextColor = Color.Gray,
                Margin = new Thickness() { Left = 0, Top = -5, Right = 0, Bottom = 0 }
            };

            Grid.SetRow(_cacheSize, 2);
            Grid.SetColumn(_cacheSize, 1);
            this.scrollGrid.Children.Add(_cacheSize);

            AppGlobals.Settings.CACHE_SIZES.ToList().ForEach(i => this._cacheSize.Items.Add(i.Key));

            // Load Settings
            this.LoadSettings();
        }

        #endregion

        #region Page Lifecycle methods
        protected async override void OnAppearing()
        {
            base.OnAppearing();

            // If we are sitting on this page we should still get notifications prompted
            MessagingCenter.Subscribe<object>(this, AppGlobals.NOTIFICATION_MESSAGE, (sender) =>
            {
                _notificationService.AlertUserIfNewNotificationExists();
            });


            // Show pop-up description if this is the first time opening the app.
            _userHasSeenDescriptionPopup = _userPreferences.GetBool(SHOWN_INITIAL_DESCRIPTION_PREF);

            if (_userHasSeenDescriptionPopup == false)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert(
                    "App Settings",
                    "These are your default settings. No action is required to use the app. You may customize these " +
                    "settings at any time.",
                    "OK").ConfigureAwait(false);

                _userPreferences.SetBool(SHOWN_INITIAL_DESCRIPTION_PREF, true);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<object>(this, AppGlobals.NOTIFICATION_MESSAGE);

        }

        #endregion

        #region Events

        private async void MenuButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(true);
        }

        private void SaveButton_Clicked(object sender, EventArgs e)
        {
            // Save Settings 
            this.SaveSettings();
        }

        private void ClearCacheButton_Clicked(object sender, EventArgs e)
        {
            this.ClearCache();
        }

        #endregion

        #region Functions

        private void LoadSettings()
        {
            // Get User Info
            //SessionService.Instance.Settings = UserSettings.GetSettings(SessionService.Instance.User.UserID);
            this.downloadOnlyOnWiFi.IsToggled = SessionService.Instance.Settings.WifiDownloadOnly;
            this.getEmailsFromWellFit.IsToggled = SessionService.Instance.Settings.AllowEmails;
            //this.notificationsOnLockScreen.IsToggled = SessionService.Instance.Settings.DisplayNotificationsOnLockScreen;
            this.pushNotifications.IsToggled = SessionService.Instance.Settings.AllowNotifications;
            this.defaultToMute.IsToggled = SessionService.Instance.Settings.Mute;

            for (int i = 0; i < AppGlobals.Settings.CACHE_SIZES.Count; i++)
            {
                if (AppGlobals.Settings.CACHE_SIZES.ElementAt(i).Value == SessionService.Instance.Settings.CacheSize)
                {
                    this._cacheSize.SelectedIndex = i;
                }
            }

            this.DisplayCurrentCacheSize();
        }

        private async void SaveSettings()
        {
            // Set User Info
            SessionService.Instance.Settings.WifiDownloadOnly = this.downloadOnlyOnWiFi.IsToggled;
            SessionService.Instance.Settings.AllowEmails = this.getEmailsFromWellFit.IsToggled;
            //SessionService.Instance.Settings.DisplayNotificationsOnLockScreen = this.notificationsOnLockScreen.IsToggled;
            SessionService.Instance.Settings.AllowNotifications = this.pushNotifications.IsToggled;
            SessionService.Instance.Settings.Mute = this.defaultToMute.IsToggled;

            // This handles both true/false cases with the user's AllowNotifications setting
            NotificationService.Instance.RescheduleNotifications();

            // Save Cache Setting
            int cacheIndex = this._cacheSize.SelectedIndex;
            if (cacheIndex > -1)
            {
                SessionService.Instance.Settings.CacheSize = AppGlobals.Settings.CACHE_SIZES.ElementAt(cacheIndex).Value;
            }

            // Save Settings
            SessionService.Instance.Settings.Save();

            System.Threading.Thread.Sleep(500);

            // NOTE: On IOS - Following Alert Causes Navigation Pop To Navigate To Blank White Screen

            // Display Settings Saved Alert
            // await DisplayAlert("", "Your settings have been saved", "OK");

            // Navigate Back To Menu
            await Navigation.PopAsync(true);

        }

        private void DisplayCurrentCacheSize()
        {
            Device.BeginInvokeOnMainThread(() =>
           {
               // Get Videos
               VideoRepository repo = new VideoRepository();
               var videos = repo.GetPlayableVideos();

               // If There Are Videos
               if (videos.Count == 0)
               {
                   this.currentCachSize.Text = "0 MB";
                   return;
               }

               // Get Files
               var dir = new DirectoryObject(AppGlobals.Settings.MOBILE_DIRECTORY);
               var files = dir.Files.Where(f => videos.Any(v => v.GetFileNameWithFullPath() == f.FilePath)).ToList();

               // Get Size
               decimal totalSize = files.Select(f => f.Size.MegaBytes).Sum() + 1;
               string size = Math.Round(totalSize, 0).ToString();
               this.currentCachSize.Text = size + " MB";
           });
        }

        private async void ClearCache()
        {
            int videosDeleted = 0;

            // Create Directory
            DirectoryObject dir = new DirectoryObject(AppGlobals.Settings.MOBILE_DIRECTORY);

            // Get Video Records
            VideoRepository repo = new VideoRepository();
            var videos = repo.GetPlayableVideos();

            // Get Video Files
            var videoFiles = dir.Files.Where(file => file.Exists == true
                && AppGlobals.Settings.PERMITTED_VIDEO_EXTENSIONS.Any(ext => ext.ToLower() == file.Extension.ToLower())
                && videos.Any(video => video.GetFileNameWithFullPath() == file.FilePath)).ToList();

            // If No Videos To Delete
            if (videos.Count == 0 || videoFiles.Count == 0)
            {
                await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Notice", "No videos to delete", "OK").ConfigureAwait(false);
                return;
            }
            else
            {
                // Prompt User To Confirm Cache Clear
                var promptResult = await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("Warning", "Are you sure you want to delete "
                    + videoFiles.Count + (videoFiles.Count == 1 ? " video?" : " videos?"), "OK", "Cancel").ConfigureAwait(false);
                if (promptResult == false) { return; }
            }

            // Loop Videos
            foreach (var video in videos)
            {
                try
                {
                    // Get Video File
                    var file = videoFiles.Where(f => f.FilePath == video.GetFileNameWithFullPath()).FirstOrDefault();

                    if (file == null)
                    {
                        continue;
                    }

                    // Delete File
                    AppGlobals.ResultType deleteFileResult = file.Delete();

                    if (deleteFileResult != AppGlobals.ResultType.Success)
                    {
                        throw new Exception("Could not delete file");
                    }

                    // Update Video Record
                    video.Deleted = true;
                    int recordResult = repo.UpdateVideo(video);

                    if (recordResult > 0)
                    {
                        videosDeleted += 1;
                    }
                }
                catch (Exception ex)
                {
                    App.Log("An Error Occurred Deleting Video " + video.Title);
                }
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Xamarin.Forms.Application.Current.MainPage.Navigation.PopAsync().ConfigureAwait(false);
            });
        }

        #endregion
    }
}
