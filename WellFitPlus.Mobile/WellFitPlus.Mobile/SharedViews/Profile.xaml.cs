using System;
using System.Linq;
using System.Collections;
using WellFitPlus.Mobile.Controls;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Services;
using WellFitPlus.Mobile.Abstractions;
using Xamarin.Forms;
using System.Threading;
using System.Collections.Generic;
using WellFitPlus.Mobile.Helpers;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace WellFitPlus.Mobile.SharedViews
{
    public partial class Profile : ContentPage
	{
		#region Private Fields
		private NotificationService _notificationService = NotificationService.Instance;
		#endregion

        #region Properties
        
        private ExtLabel _todaySessionsCompletedLabel;
        private ExtLabel _todaySessionsTotalLabel;
        private ExtLabel _todayBonusSessionsLabel;

        private ExtLabel _weekSessionsCompletedLabel;
        private ExtLabel _weekSessionsTotalLabel;
        private ExtLabel _weekBonusSessionsLabel;

        private ExtLabel _dailyAverageSessionsCompletedLabel;

        private ExtLabel _currentSessionStreakLabel;
        private ExtLabel _previousSessionStreakLabel;
        
        private static SyncService _syncService;

        private static bool _pageHasInitialized = false;

        #endregion

        #region Initialization

        public Profile ()
		{
			InitializeComponent();
            //Apply Safe Area
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
            try
            {
                if (_pageHasInitialized == false && _syncService == null)
                {
                    _pageHasInitialized = true;
					_syncService = SyncService.Instance;
                }

                this.BackgroundImage = AppGlobals.Images.BLUE_GRADIENT_IMAGE;

                UserSettings newSettings = UserSettings.GetExistingSettings();
#if DEBUG
                //newSettings.RegistrationDate = DateTime.Now;
                newSettings.IsSubscribed = true; // Might not need this
#endif

				// TODO: FIX: This shows randomly when not subscribed.
                //App.UserIsSubscribed = newSettings.IsSubscribed;
                //if (App.UserIsSubscribed == false && newSettings.FreeTrialHasExpired == true)
                //{
                //    Application.Current.MainPage.DisplayAlert("Alert", "Your free trial has expired. Get the full version to experience all features", "OK");
                //}

				this.menuBar.LeftButton.Clicked += this.MenuButton_Clicked;
				this.menuBar.RightButton.Clicked += this.BackButton_Clicked;

                // Load Controls
                this.LoadStreakControls();
                this.LoadTodayScheduledSessionControls();
                this.LoadTodayBonusSessionControls();
                this.LoadWeekScheduledSessionControls();
                this.LoadWeekBonusSessionControls();
                this.LoadDailyAverageSessionControls();

                // Load User Profile Information
                this.LoadUserProfileInformation();
            }
            catch (Exception ex)
            {
                App.Log("Exception Thrown Loading User Profile");
            }
        }

		#endregion

		#region Destructor/Destructor functionality
		// We need to make sure we unsubscribe to events after this class doesn't exist anymore.
		~Profile() {
			MessagingCenter.Unsubscribe<string>(this, AppGlobals.Events.REFRESH_PROFILE);
			MessagingCenter.Unsubscribe<string>(this, AppGlobals.Events.ACTIVITY_COMPLETED);
		}
		#endregion

		#region Page Lifecycle methods
		protected override void OnAppearing()
		{

			bool canSync = _syncService.CheckCanSync();

			if (!canSync)
			{
				// We are in the delay period meaning we still have time left before we can sync to the server again.
				// In this time check to see if any videos have downloaded.
				_syncService.TryDownloadFailedVideos();
			}

			// Subscribe To The Activity Acknowledged Event
			MessagingCenter.Subscribe<string>(this, AppGlobals.Events.REFRESH_PROFILE, message =>
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					this.LoadUserProfileInformation();
				});
			});

			// Subscribe To The Activity Completed Event
			MessagingCenter.Subscribe<string>(this, AppGlobals.Events.ACTIVITY_COMPLETED, message =>
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					this.LoadUserProfileInformation();
				});
			});

			// Subscribe To The Downloading event
			MessagingCenter.Subscribe<object, string>(
				this, AppGlobals.Events.NOTIFY_DOWNLOADING_PROGRESS, (sender, progressString) => {
					ToggleVideosDownloadingSectionVisibility(true, progressString);
				}
			);

			MessagingCenter.Subscribe<object>(this, AppGlobals.Events.NOTIFY_DOWNLOADING_COMPLETE, (sender) => {

				ToggleVideosDownloadingSectionVisibility(false, "");
				_syncService.TryDownloadFailedVideos();

			});

			// To avoid an infinite loop when a video keeps failing to download we have a separate
			// event for the failed video download completion.
			MessagingCenter.Subscribe<object>(this, AppGlobals.Events.NOTIFY_RETRY_DOWNLOADING_COMPLETE, (sender) => {

				ToggleVideosDownloadingSectionVisibility(false, "");			
			});

			// If we are sitting on this page we should still get notifications prompted
			MessagingCenter.Subscribe<object>(this, AppGlobals.NOTIFICATION_MESSAGE, (sender) =>
			{
				_notificationService.AlertUserIfNewNotificationExists();
			});

			// Schedule notifications to accommodate for conditional notifications. Since notifications
            // need to be pre-scheduled we need to re-schedule them each time a video completes to account for
            // the activity. Also since this screen is shown immediately after the native video window is shown
            // this is a perfect place to do this.
            _notificationService.ScheduleNotifications();

			this.LoadUserProfileInformation(); // Refresh profile page statistics

			RefreshTipOfTheDayMessage();

			// The progress string will be populated if we are currently downloading videos.
			ToggleVideosDownloadingSectionVisibility(
				_syncService.IsDownloadingVideos, _syncService.GetDownloadProgressString());
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			MessagingCenter.Unsubscribe<string>(this, AppGlobals.Events.REFRESH_PROFILE);
			MessagingCenter.Unsubscribe<string>(this, AppGlobals.Events.ACTIVITY_COMPLETED);
			MessagingCenter.Unsubscribe<object>(this, AppGlobals.Events.NOTIFY_DOWNLOADING_PROGRESS);
			MessagingCenter.Unsubscribe<object, string>(this, AppGlobals.NOTIFICATION_MESSAGE);
			MessagingCenter.Unsubscribe<object>(this, AppGlobals.Events.NOTIFY_DOWNLOADING_COMPLETE);
			MessagingCenter.Unsubscribe<object>(this, AppGlobals.Events.NOTIFY_RETRY_DOWNLOADING_COMPLETE);

			progressContainer.IsVisible = false;
			activityIndicator.IsRunning = false;
		}

		#endregion

        #region Load Controls

        private void LoadStreakControls()
        {
            // Streak Section Background Image
            var streakImage = new Image()
            {
                Source = AppGlobals.Images.BLUE_GRADIENT_IMAGE,
                Aspect = Aspect.Fill
            };

            this.streakLayout.Children.Add(streakImage,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent((parent) => { return parent.Width; }),
                Constraint.RelativeToParent((parent) => { return parent.Height; }));

            // Current Streak Label
            this._currentSessionStreakLabel = new ExtLabel()
            {
                Text = "Session Streak",
                //FontAttributes = FontAttributes.Italic,
                FontSize = 36,
                FontFamily = "Raleway-Regular",
                Margin = new Thickness(5),
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                TextColor = Color.White
            };

            this.streakLayout.Children.Add(_currentSessionStreakLabel,
                null, null,
                Constraint.RelativeToParent((parent) => { return parent.Width; }),
                Constraint.RelativeToParent((parent) => { return parent.Height; }));
            
            // Previous Session Label
            this._previousSessionStreakLabel = new ExtLabel()
            {
                Text = "BEST STREAK: 61",
                FontSize = 16,
                FontFamily = "Raleway-Regular",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Color.White
            };

            this.streakLayout.Children.Add(_previousSessionStreakLabel,
                null,
                Constraint.RelativeToView(_currentSessionStreakLabel, (parent, sibling) => { return 90; }),
                Constraint.RelativeToParent((parent) => { return parent.Width; }),
                null);
        }
        
        private void LoadTodayScheduledSessionControls()
        {
            // Left Backgruond Image
            var streakImage = new Image()
            {
                Source = AppGlobals.Images.STREAK_TOP_LEFT2,
                Aspect = Aspect.Fill
            };
            this.todaySessionLayout.Children.Add(streakImage,
              Constraint.Constant(0),
              Constraint.Constant(0),
              Constraint.RelativeToParent((parent) => { return parent.Width; }),
              Constraint.RelativeToParent((parent) => { return parent.Height; }));

            // Left Grid
            Grid grid = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitionCollection()
                {
                    new ColumnDefinition() { Width = new GridLength (1, GridUnitType.Star) },
                    new ColumnDefinition() { Width = new GridLength (.25, GridUnitType.Star) },
                    new ColumnDefinition() { Width = new GridLength (1, GridUnitType.Star) },
                },
                RowDefinitions = new RowDefinitionCollection()
                {
                    new RowDefinition() { Height =  new GridLength (1, GridUnitType.Star)},
                    new RowDefinition() { Height =  new GridLength (.4, GridUnitType.Star) }
                },
                RowSpacing=0,
                ColumnSpacing=7,
                Padding = new Thickness() { Left = 0, Top = 0, Right = 0, Bottom = 0 }
            };

            // Middle Label
            ExtLabel label = new ExtLabel()
            {
                Text = "of",
                FontAttributes = FontAttributes.None,
                FontSize = 18,
                FontFamily = "Raleway-Regular",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness() {  Top = 45 },
                TextColor = Color.White
            };

            Grid.SetColumn(label, 1);
            Grid.SetRow(label, 0);
            grid.Children.Add(label);

            // Left Label
            _todaySessionsCompletedLabel = new ExtLabel()
            {
                Text = "4",
                FontSize = 50,
                FontFamily = "Raleway-ExtraBold",
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness() { Bottom = 0, Right = 0, Top = 15 },
                TextColor = Color.White
            };

            Grid.SetColumn(_todaySessionsCompletedLabel, 0);
            Grid.SetRow(_todaySessionsCompletedLabel, 0);
            grid.Children.Add(_todaySessionsCompletedLabel);

            // Right Label
            _todaySessionsTotalLabel = new ExtLabel()
            {
                Text = "8",
                FontSize = 50,
                FontFamily = "Raleway-Bold",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness() { Bottom = 0, Left = 0, Top = 15 },
                TextColor = Color.White
            };

            Grid.SetColumn(_todaySessionsTotalLabel, 2);
            Grid.SetRow(_todaySessionsTotalLabel, 0);
            grid.Children.Add(_todaySessionsTotalLabel);

            // Bottom Label
            ExtLabel previousSessionsLabel = new ExtLabel()
            {
                Text = "SCHEDULED SESSIONS",
                FontSize = 14,
                FontFamily = "Raleway-Regular",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Color.White
            };

            Grid.SetColumn(previousSessionsLabel, 0);
            Grid.SetRow(previousSessionsLabel, 1);
            Grid.SetColumnSpan(previousSessionsLabel, 3);
            grid.Children.Add(previousSessionsLabel);

            this.todaySessionLayout.Children.Add(grid, 
                Constraint.Constant(0), 
                Constraint.Constant(0), 
                Constraint.RelativeToParent((parent) => { return parent.Width; }),
                Constraint.RelativeToParent((parent) => { return parent.Height; }));
        }
        
        private void LoadTodayBonusSessionControls()
        {
            // Right Backgruond Image
            var streakImage = new Image()
            {
                Source = AppGlobals.Images.STREAK_TOP_RIGHT2,
                Aspect = Aspect.Fill
            };

            this.todayBonusLayout.Children.Add(streakImage,
              Constraint.Constant(0),
              Constraint.Constant(0),
                Constraint.RelativeToParent((parent) => { return parent.Width; }),
                Constraint.RelativeToParent((parent) => { return parent.Height; }));

            // Right Grid
            Grid grid = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitionCollection()
                {
                    new ColumnDefinition() { Width = new GridLength (1, GridUnitType.Star) },
                },
                RowDefinitions = new RowDefinitionCollection()
                {
                    new RowDefinition() { Height =  new GridLength (1, GridUnitType.Star)},
                    new RowDefinition() { Height =  new GridLength (.4, GridUnitType.Star) }
                },
                RowSpacing = 0,
                ColumnSpacing = 0,
                Padding = new Thickness() { Left = 0, Top = 0, Right = 0, Bottom = 0 }
            };

            // Bonus Label
            _todayBonusSessionsLabel = new ExtLabel()
            {
                Text = "3",
                FontSize = 64,
                FontFamily = "Raleway-Bold",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness() { Bottom = 0, Right = 0, Top = 15 },
                TextColor = Color.White
            };

            Grid.SetColumn(_todayBonusSessionsLabel, 0);
            Grid.SetRow(_todayBonusSessionsLabel, 0);
            grid.Children.Add(_todayBonusSessionsLabel);

            // Bottom Label
            ExtLabel previousSessionsLabel = new ExtLabel()
            {
                Text = "BONUS SESSIONS",
                FontSize = 14,
                FontFamily = "Raleway-Regular",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Color.White
            };

            Grid.SetColumn(previousSessionsLabel, 0);
            Grid.SetRow(previousSessionsLabel, 1);
            grid.Children.Add(previousSessionsLabel);

            this.todayBonusLayout.Children.Add(grid,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent((parent) => { return parent.Width; }),
                Constraint.RelativeToParent((parent) => { return parent.Height; }));
        }
        
        private void LoadWeekScheduledSessionControls()
        {
            // Week Left Background Image
            var streakImage = new Image()
            {
                Source = AppGlobals.Images.STREAK_TOP_LEFT2,
                Aspect = Aspect.Fill
            };

            this.weekSessionLayout.Children.Add(streakImage,
              //Constraint.Constant(0),
              //Constraint.Constant(0),
              null,null,
              Constraint.RelativeToParent((parent) => { return parent.Width; }),
              Constraint.RelativeToParent((parent) => { return parent.Height; }));

            // Week Grid
            Grid grid = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitionCollection()
                {
                    new ColumnDefinition() { Width = new GridLength (1, GridUnitType.Star) },
                    new ColumnDefinition() { Width = new GridLength (.25, GridUnitType.Star) },
                    new ColumnDefinition() { Width = new GridLength (1, GridUnitType.Star) },
                },
                RowDefinitions = new RowDefinitionCollection()
                {
                    new RowDefinition() { Height =  new GridLength (1, GridUnitType.Star)},
                    new RowDefinition() { Height =  new GridLength (.4, GridUnitType.Star) }
                },
                RowSpacing = 0,
                ColumnSpacing = 7,
                Padding = new Thickness() { Left = 0, Top = 0, Right = 0, Bottom = 0 }
            };

            // Middle Label
            ExtLabel label = new ExtLabel()
            {
                Text = "of",
                FontAttributes = FontAttributes.None,
                FontSize = 18,
                FontFamily = "Raleway-Regular",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness() { Top = 45 },
                TextColor = Color.White
            };

            Grid.SetColumn(label, 1);
            Grid.SetRow(label, 0);
            grid.Children.Add(label);

            // Left Label
            _weekSessionsCompletedLabel = new ExtLabel()
            {
                Text = "12",
                FontSize = 50,
                FontFamily = "Raleway-Bold",
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness() { Bottom = 0, Right = 0, Top = 15 },
                TextColor = Color.White
            };

            Grid.SetColumn(_weekSessionsCompletedLabel, 0);
            Grid.SetRow(_weekSessionsCompletedLabel, 0);
            grid.Children.Add(_weekSessionsCompletedLabel);

            // Right Label
            _weekSessionsTotalLabel = new ExtLabel()
            {
                Text = "20",
                FontSize = 50,
                FontFamily = "Raleway-Bold",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness() { Bottom = 0, Left = 0, Top = 15 },
                TextColor = Color.White
            };

            Grid.SetColumn(_weekSessionsTotalLabel, 2);
            Grid.SetRow(_weekSessionsTotalLabel, 0);
            grid.Children.Add(_weekSessionsTotalLabel);

            // Bottom Label
            ExtLabel previousSessionsLabel = new ExtLabel()
            {
                Text = "SCHEDULED SESSIONS",
                FontSize = 14,
                FontFamily = "Raleway-Regular",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Color.White
            };

            Grid.SetColumn(previousSessionsLabel, 0);
            Grid.SetRow(previousSessionsLabel, 1);
            Grid.SetColumnSpan(previousSessionsLabel, 3);
            grid.Children.Add(previousSessionsLabel);

            this.weekSessionLayout.Children.Add(grid,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent((parent) => { return parent.Width; }),
                Constraint.RelativeToParent((parent) => { return parent.Height; }));
        }

        private void LoadWeekBonusSessionControls()
        {
            // Week Bonus Background Image
            var streakImage = new Image()
            {
                Source = AppGlobals.Images.STREAK_TOP_RIGHT2,
                Aspect = Aspect.Fill
            };

            this.weekBonusLayout.Children.Add(streakImage,
              Constraint.Constant(0),
              Constraint.Constant(0),
              Constraint.RelativeToParent((parent) => { return parent.Width; }),
              Constraint.RelativeToParent((parent) => { return parent.Height; }));

            // Bonus Grid
            Grid grid = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitionCollection()
                {
                    new ColumnDefinition() { Width = new GridLength (1, GridUnitType.Star) },
                },
                RowDefinitions = new RowDefinitionCollection()
                {
                    new RowDefinition() { Height =  new GridLength (1, GridUnitType.Star)},
                    new RowDefinition() { Height =  new GridLength (.4, GridUnitType.Star) }
                },
                RowSpacing = 0,
                ColumnSpacing = 0,
                Padding= new Thickness() {  Left=0,Top=0,Right=0,Bottom=0}
            };

            // Bonus Label
            _weekBonusSessionsLabel = new ExtLabel()
            {
                Text = "15",
                FontSize = 64,
                FontFamily = "Raleway-Bold",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness() { Bottom = 0, Right = 0, Top = 15 },
                TextColor = Color.White
            };

            Grid.SetColumn(_weekBonusSessionsLabel, 0);
            Grid.SetRow(_weekBonusSessionsLabel, 0);
            grid.Children.Add(_weekBonusSessionsLabel);

            // Bottom Label
            ExtLabel previousSessionsLabel = new ExtLabel()
            {
                Text = "BONUS SESSIONS",
                FontSize = 14,
                FontFamily = "Raleway-Regular",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Color.White
            };

            Grid.SetColumn(previousSessionsLabel, 0);
            Grid.SetRow(previousSessionsLabel, 1);
            grid.Children.Add(previousSessionsLabel);

            this.weekBonusLayout.Children.Add(grid,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent((parent) => { return parent.Width; }),
                Constraint.RelativeToParent((parent) => { return parent.Height; }));
        }

        private void LoadDailyAverageSessionControls()
        {
            // Average Session Background Image
            var streakImage = new Image()
            {
                Source = AppGlobals.Images.BLUE_GRADIENT_IMAGE,
                Aspect = Aspect.Fill
            };

            this.dailyAverageLayout.Children.Add(streakImage,
              Constraint.Constant(0),
              Constraint.Constant(0),
              Constraint.RelativeToParent((parent) => { return parent.Width; }),
              Constraint.RelativeToParent((parent) => { return parent.Height; }));

            // Background Grid
            Grid grid = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitionCollection()
                {
                    new ColumnDefinition() { Width = new GridLength (1, GridUnitType.Star) },
                },
                RowDefinitions = new RowDefinitionCollection()
                {
                    new RowDefinition() { Height =  new GridLength (1, GridUnitType.Star)},
                    new RowDefinition() { Height =  new GridLength (.5, GridUnitType.Star) }
                },
                RowSpacing=0,
                ColumnSpacing=0,
                Padding = new Thickness() { Left = 0, Top = 0, Right = 0, Bottom = 0 }
            };

            // Average Label
            _dailyAverageSessionsCompletedLabel = new ExtLabel()
            {
                Text = "80%",
                FontSize = 64,
                FontFamily = "Raleway-ExtraBold",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Color.White
            };

            Grid.SetColumn(_dailyAverageSessionsCompletedLabel, 0);
            Grid.SetRow(_dailyAverageSessionsCompletedLabel, 0);
            grid.Children.Add(_dailyAverageSessionsCompletedLabel);

            // Bottom Label
            ExtLabel previousSessionsLabel = new ExtLabel()
            {
                Text = "OF SCHEDULED SESSIONS",
                FontSize = 14,
                FontFamily = "Raleway-Regular",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Color.White
            };

            Grid.SetColumn(previousSessionsLabel, 0);
            Grid.SetRow(previousSessionsLabel, 1);
            grid.Children.Add(previousSessionsLabel);

            this.dailyAverageLayout.Children.Add(grid,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent((parent) => { return parent.Width; }),
                Constraint.RelativeToParent((parent) => { return parent.Height; }));
        }

        #endregion

        #region Load User Information

        private void LoadUserProfileInformation()
        {
            try
            {
                ActivitySessionRepository activityRepo = ActivitySessionRepository.Instance;
				var allActivities = activityRepo.GetActivities(DateTime.MinValue);

                this.ClearLabels(); // Set Default values first then set values as we go.

				#region Today

				var activitiesToday = allActivities.Where(activity => activity.NotificationTime >= DateTime.Today).ToList();
				var nonScheduledBonuses =
					allActivities.Where(a =>
										(a.StartTime >= DateTime.Today)
										&& (a.NotificationTime == DateTime.MinValue)
					                    && (a.Bonus == true)).ToList().Count;
				
				int totalBonusesToday = 
					activitiesToday.Where(activity => activity.Bonus == true).Count() + nonScheduledBonuses;

				double totalToday = 0;
				double completedToday = 0;
				if (activitiesToday.Count > 0)
				{
					// Set Today Information
					completedToday =
						activitiesToday.Where(activity => activity.IsPending == false && activity.IsCompleted == true).Count();
					totalToday = activitiesToday.Count();
					this._todaySessionsCompletedLabel.Text = completedToday.ToString();
					this._todaySessionsTotalLabel.Text = totalToday.ToString();
					this._todayBonusSessionsLabel.Text = totalBonusesToday.ToString();
						

					if (totalToday >= 100)
					{
						this._todaySessionsTotalLabel.FontSize = 38;
					}
					if (completedToday >= 100)
					{
						this._todaySessionsCompletedLabel.FontSize = 38;
					}
				}
				#endregion

				#region Week

				// Get all the activities from the current week. Week starts on Monday.

				int dayOfWeek = (int) DateTime.Today.DayOfWeek; // Sun -> Sat is 0 -> 6
				int mon = 1;
				int dayDifference = dayOfWeek != 0 ? dayOfWeek - mon : 6; // Difference from Monday

				// Get all the activities starting from last Monday to today (unless today is Monday)
				var activitiesWeek =
					allActivities.Where(a => a.NotificationTime >= DateTime.Today.AddDays(-dayDifference)).ToList();

				nonScheduledBonuses =
					allActivities.Where(a =>
										(a.StartTime >= DateTime.Today.AddDays(-dayDifference))
										&& (a.NotificationTime == DateTime.MinValue)
					                    && (a.Bonus == true)).Count();

				var totalBonusesThisWeek = 
					activitiesWeek.Where(activity => activity.Bonus == true).Count() + nonScheduledBonuses;

				double completedWeek = 0;
				double totalWeek = 0;
				if (activitiesWeek.Count > 0)
				{
					// Set Week Information
					completedWeek = activitiesWeek.Where(activity => activity.IsPending == false 
					                                     && activity.IsCompleted == true).Count();
					totalWeek = activitiesWeek.Count();
					this._weekSessionsCompletedLabel.Text = completedWeek.ToString();
					this._weekSessionsTotalLabel.Text = totalWeek.ToString();
					this._weekBonusSessionsLabel.Text = totalBonusesThisWeek.ToString();

					if (totalWeek >= 100)
					{
						this._weekSessionsTotalLabel.FontSize = 38;
					}
					if (completedWeek >= 100)
					{
						this._weekSessionsCompletedLabel.FontSize = 38;
					}
				}
				#endregion

				#region Current Streak

				// Order the activities to get ready to traverse through.
				allActivities = allActivities.OrderByDescending(a => a.StartTime).ToList();

				int currentStreak = 0;

				// Use the list of all of the activities to get the current streaks
				for (int i = 0; i < allActivities.Count; i++)
				{
					var activity = allActivities[i];
					if (activity.IsCompleted)
					{
						currentStreak++;
					}
					else if (activity.IsBonusAndNotScheduled) {
						continue; // Don't count pure bonus videos
					}
					else {
						break; // We have reached the end of the current steak
					}

				}

                this._currentSessionStreakLabel.Text = currentStreak == 0 ? "No Active Streak"
                    : currentStreak + " Session Streak";

				#endregion

				#region Best Streak


				int largestStreak = 0;
				int currentCount = 0;

				// Use the list of all of the activities to get the all time best streak
				for (int i = 0; i < allActivities.Count; i++) {
					var activity = allActivities[i];

					if (activity.IsCompleted)
					{
						currentCount++;
					}
					else if (activity.IsBonusAndNotScheduled) {
						continue; // Don't count pure bonus video
					}
					else {
						currentCount = 0;
					}

					largestStreak = Math.Max(currentCount, largestStreak);
				}

				this._previousSessionStreakLabel.Text = "BEST STREAK: " + largestStreak;

				#endregion

				#region Other
				// Calculate and set the all time daily average.
				float totalCompletedActivities = allActivities.Where(a => a.IsCompleted).ToList().Count;
				float scheduledActivities = allActivities.Where(a => a.IsBonusAndNotScheduled == false).Count();
				float dailyPercentage = totalCompletedActivities / scheduledActivities;
				int dailyAverage = (int) Math.Round(dailyPercentage * 100);

				this._dailyAverageSessionsCompletedLabel.Text = dailyAverage + "%";

                try
                {
                    // Set Progress
                    this.progressToday.WidthRequest = totalToday < 1 ? 0 : (completedToday / totalToday) * (menuBar.Width <= 0 ? 400 : menuBar.Width);
                    this.progressWeek.WidthRequest = totalWeek < 1 ? 0 : (completedWeek / totalWeek) * (menuBar.Width <= 0 ? 400 : menuBar.Width);
                }
                catch (Exception ex) { }

                #endregion

            }
            catch (Exception ex)
            {
            }
        }

        private void ClearLabels()
        {
            this._todaySessionsCompletedLabel.Text = "0";
            this._todaySessionsTotalLabel.Text = "0";
            this._todayBonusSessionsLabel.Text = "0";

            this._weekSessionsCompletedLabel.Text = "0";
            this._weekSessionsTotalLabel.Text = "0";
            this._weekBonusSessionsLabel.Text = "0";

            this._currentSessionStreakLabel.Text = "No Active Streak";
            this._previousSessionStreakLabel.Text = "BEST STREAK: 0";            
            this._dailyAverageSessionsCompletedLabel.Text = "0 %";
            
            // Set Progress
            this.progressToday.WidthRequest = 0;
            this.progressWeek.WidthRequest = 0;            
        }

        #endregion

        #region Events

        private async void MenuButton_Clicked(object sender, EventArgs e)
        {
            var menu = new Menu();
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(menu, false);
            await Navigation.PushAsync(menu);

        }

        private void BackButton_Clicked(object sender, EventArgs e)
		{
			//double percentToday = new Random().Next(0, 100);
			//double percentWeek = new Random().Next(0, 100);
			//this.progressToday.WidthRequest = (percentToday / 100) * menuBar.Width;
			//this.progressWeek.WidthRequest = (percentWeek / 100) * menuBar.Width;      
		}

        #endregion

		/// <summary>
		/// Refreshs the tip of the day message. This only refreshes the message once per day and will do nothing
		/// if the message has already been refreshed in the past day.
		/// </summary>
		private async void RefreshTipOfTheDayMessage()
		{

			// Used to save simple values
			var userPreferences = DependencyService.Get<IPreferences>();


			var lastUpdatedIsoString = userPreferences.GetString(AppGlobals.DAILY_TIP_TIMESTAMP_PREF);
			DateTime lastUpdatedDatePlusOneDay = DateTime.MinValue; // We will check against this value

			if (lastUpdatedIsoString != null)
			{
				// We have set a tip of the day previously. Convert the ISO string to a date.
				// Need to add one day to make sure that we only allow the message to change once per day
				lastUpdatedDatePlusOneDay = DateFormatHelper.ParseIsoFormattedString(lastUpdatedIsoString).AddDays(1);
			}

			// Update the Tip of the Day message if we need to.
			if ((lastUpdatedDatePlusOneDay == DateTime.MinValue)
				|| (lastUpdatedDatePlusOneDay.CompareTo(DateTime.UtcNow) < 0))
			{

				// if (< 0) is true in the above check then lastUpdatedDatePlusOneDay is earlier than DateTime.Now

				try
				{

					var service = new WebApiService(SessionService.Instance.Configuration);
					List<Message> messages = await service.Get<List<Message>>("Message/GetMessages");

					string previousMessageDescription = userPreferences.GetString(AppGlobals.DAILY_TIP_MESSAGE_PREF);


					if (previousMessageDescription != null)
					{
						// We need to remove the message we already have from the list of messages so we can pick a
						// new one randomly.
						var previousMessage =
							messages.Where(m => m.Description == previousMessageDescription).FirstOrDefault();

						messages.Remove(previousMessage); // This will do nothing if the message doesn't exist

					}

					Random random = new Random(DateTime.Now.Millisecond); // Seed the Random Number generato
					int newMessageIndex = random.Next(0, messages.Count);

					// Set the new message along with it's new timestam
					userPreferences.SetString(
						AppGlobals.DAILY_TIP_MESSAGE_PREF, 
						messages[newMessageIndex].Description
					);
					userPreferences.SetString(
						AppGlobals.DAILY_TIP_TIMESTAMP_PREF,
						DateFormatHelper.DateTimeToIsoFormat(DateTime.UtcNow)
					);

				}
				catch (SimpleHttpResponseException se)
				{
					Console.WriteLine("Error getting messages: " + se);
				}
				catch (TimeoutException te)
				{
					Console.WriteLine("The request timed-out " + te);
				}
				catch (Exception e)
				{
					Console.WriteLine("An error occured when fetching the tip of the day messages: " + e);
				}

			}

		}

		private void ToggleVideosDownloadingSectionVisibility(bool isVisible, string progressString) { 
			progressContainer.IsVisible = isVisible;
			activityIndicator.IsRunning = isVisible;
			videoDownloadText.Text = progressString;
		}
    }
}
