using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Services;
using WellFitPlus.Mobile.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace WellFitPlus.Mobile.SharedViews
{
    public partial class Notifications : ContentPage
    {
        #region Properties

        private bool mondayIncluded = false;
        private bool tuesdayIncluded = false;
        private bool wednesdayIncluded = false;
        private bool thursdayIncluded = false;
        private bool fridayIncluded = false;
        private bool saturdayIncluded = false;
        private bool sundayIncluded = false;

        private Xamarin.Forms.Picker _notifyPicker;

        #endregion

        private NotificationService _notificationService = NotificationService.Instance;

        #region Initialization

        public Notifications()
        {
            InitializeComponent();
            //Apply Safe Area
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
            this.menuBar.LeftButton.Clicked += this.MenuButton_Clicked;
            this.menuBar.RightButton.Clicked += this.SaveButton_Clicked;
            this.mondayButton.Clicked += this.MondayButton_Clicked;
            this.tuesdayButton.Clicked += this.TuesdayButton_Clicked;
            this.wednesdayButton.Clicked += this.WednesdayButton_Clicked;
            this.thursdayButton.Clicked += this.ThursdayButton_Clicked;
            this.fridayButton.Clicked += this.FridayButton_Clicked;
            this.saturdayButton.Clicked += this.SaturdayButton_Clicked;
            this.sundayButton.Clicked += this.SundayButton_Clicked;

            this._notifyPicker = new Xamarin.Forms.Picker()
            {
                Title = "Time",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                TextColor = Color.Gray,
                Margin = new Thickness() { Left = -35, Top = -5, Right = 0, Bottom = 0 },
                Scale = 1.25
            };

            //this.hourMinimumPicker.SelectedIndexChanged += new EventHandler((sender, e) => 
            //{
            //    if (this.hourMinimumPicker.SelectedIndex > this.hourMaximumPicker.SelectedIndex)
            //    {
            //        this.hourMinimumPicker.SelectedIndex = this.hourMaximumPicker.SelectedIndex;
            //    };
            //});

            //this.hourMaximumPicker.SelectedIndexChanged += new EventHandler((sender, e) =>
            //{
            //    if (this.hourMaximumPicker.SelectedIndex < this.hourMinimumPicker.SelectedIndex)
            //    {
            //        this.hourMaximumPicker.SelectedIndex = this.hourMinimumPicker.SelectedIndex;
            //    };
            //});

            Grid.SetRow(_notifyPicker, 0);
            Grid.SetColumn(_notifyPicker, 2);
            this.grid.Children.Add(_notifyPicker);

            AppGlobals.Notifications.FREQUENCIES.ToList().ForEach(i => this._notifyPicker.Items.Add(i.Key));

            // Load Settings 
            this.LoadSettings();
        }

        #endregion

        #region Page Lifecycle methods
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // If we are sitting on this page we should still get notifications prompte
            MessagingCenter.Subscribe<object>(this, AppGlobals.NOTIFICATION_MESSAGE, (sender) =>
            {
                _notificationService.AlertUserIfNewNotificationExists();
            });
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

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            // Save Settings
            this.SaveSettings();
        }

        private void MondayButton_Clicked(object sender, EventArgs e)
        {
            this.mondayIncluded = !this.mondayIncluded;

            if (this.mondayIncluded == true)
            {
                this.mondayButton.TextColor = Color.FromHex("#FFB638");
                this.mondayBox.Color = Color.FromHex("#FFB638");
                this.mondayBox.IsVisible = true;
            }
            else
            {
                this.mondayButton.TextColor = Color.Gray;
                this.mondayBox.IsVisible = false;
            }
        }

        private void TuesdayButton_Clicked(object sender, EventArgs e)
        {
            this.tuesdayIncluded = !this.tuesdayIncluded;

            if (this.tuesdayIncluded == true)
            {
                this.tuesdayButton.TextColor = Color.FromHex("#FFB638");
                this.tuesdayBox.Color = Color.FromHex("#FFB638");
                this.tuesdayBox.IsVisible = true;
            }
            else
            {
                this.tuesdayButton.TextColor = Color.Gray;
                this.tuesdayBox.IsVisible = false;
            }
        }

        private void WednesdayButton_Clicked(object sender, EventArgs e)
        {
            this.wednesdayIncluded = !this.wednesdayIncluded;

            if (this.wednesdayIncluded == true)
            {
                this.wednesdayButton.TextColor = Color.FromHex("#FFB638");
                this.wednesdayBox.Color = Color.FromHex("#FFB638");
                this.wednesdayBox.IsVisible = true;
            }
            else
            {
                this.wednesdayButton.TextColor = Color.Gray;
                this.wednesdayBox.IsVisible = false;
            }
        }

        private void ThursdayButton_Clicked(object sender, EventArgs e)
        {
            this.thursdayIncluded = !this.thursdayIncluded;

            if (this.thursdayIncluded == true)
            {
                this.thursdayButton.TextColor = Color.FromHex("#FFB638");
                this.thursdayBox.Color = Color.FromHex("#FFB638");
                this.thursdayBox.IsVisible = true;
            }
            else
            {
                this.thursdayButton.TextColor = Color.Gray;
                this.thursdayBox.IsVisible = false;
            }
        }

        private void FridayButton_Clicked(object sender, EventArgs e)
        {
            this.fridayIncluded = !this.fridayIncluded;

            if (this.fridayIncluded == true)
            {
                this.fridayButton.TextColor = Color.FromHex("#FFB638");
                this.fridayBox.Color = Color.FromHex("#FFB638");
                this.fridayBox.IsVisible = true;
            }
            else
            {
                this.fridayButton.TextColor = Color.Gray;
                this.fridayBox.IsVisible = false;
            }
        }

        private void SaturdayButton_Clicked(object sender, EventArgs e)
        {
            this.saturdayIncluded = !this.saturdayIncluded;

            if (this.saturdayIncluded == true)
            {
                this.saturdayButton.TextColor = Color.FromHex("#FFB638");
                this.saturdayBox.Color = Color.FromHex("#FFB638");
                this.saturdayBox.IsVisible = true;
            }
            else
            {
                this.saturdayButton.TextColor = Color.Gray;
                this.saturdayBox.IsVisible = false;
            }
        }

        private void SundayButton_Clicked(object sender, EventArgs e)
        {
            this.sundayIncluded = !this.sundayIncluded;

            if (this.sundayIncluded == true)
            {
                this.sundayButton.TextColor = Color.FromHex("#FFB638");
                this.sundayBox.Color = Color.FromHex("#FFB638");
                this.sundayBox.IsVisible = true;
            }
            else
            {
                this.sundayButton.TextColor = Color.Gray;
                this.sundayBox.IsVisible = false;
            }
        }

        #endregion

        #region Functions

        private void LoadSettings()
        {
            UserSettings.DaysOfWeek days = SessionService.Instance.Settings.NotificationDays;
            if (days.HasFlag(UserSettings.DaysOfWeek.Monday))
            {
                this.MondayButton_Clicked(null, null);
            }
            if (days.HasFlag(UserSettings.DaysOfWeek.Tuesday))
            {
                this.TuesdayButton_Clicked(null, null);
            }
            if (days.HasFlag(UserSettings.DaysOfWeek.Wednesday))
            {
                this.WednesdayButton_Clicked(null, null);
            }
            if (days.HasFlag(UserSettings.DaysOfWeek.Thursday))
            {
                this.ThursdayButton_Clicked(null, null);
            }
            if (days.HasFlag(UserSettings.DaysOfWeek.Friday))
            {
                this.FridayButton_Clicked(null, null);
            }
            if (days.HasFlag(UserSettings.DaysOfWeek.Saturday))
            {
                this.SaturdayButton_Clicked(null, null);
            }
            if (days.HasFlag(UserSettings.DaysOfWeek.Sunday))
            {
                this.SundayButton_Clicked(null, null);
            }

            this.hourMinimumPicker.SelectedIndex = (SessionService.Instance.Settings.BeginHour - 1 < 0) ? 0 : SessionService.Instance.Settings.BeginHour - 1;
            this.hourMaximumPicker.SelectedIndex = (SessionService.Instance.Settings.EndHour < 0) ? 0 : SessionService.Instance.Settings.EndHour - 1;

            this._notifyPicker.SelectedIndex = 0;
            for (int i = 0; i < AppGlobals.Notifications.FREQUENCIES.Count; i++)
            {
                if (AppGlobals.Notifications.FREQUENCIES.ElementAt(i).Value == SessionService.Instance.Settings.Frequency)
                {
                    this._notifyPicker.SelectedIndex = i;
                }
            }
        }

        private async void SaveSettings()
        {
            // TODO: Validate input for notifications so the calculations don't have conflicts.

            UserSettings.DaysOfWeek days = UserSettings.DaysOfWeek.None;

            if (this.mondayIncluded)
            {
                days |= UserSettings.DaysOfWeek.Monday;
            }
            if (this.tuesdayIncluded)
            {
                days |= UserSettings.DaysOfWeek.Tuesday;
            }
            if (this.wednesdayIncluded)
            {
                days |= UserSettings.DaysOfWeek.Wednesday;
            }
            if (this.thursdayIncluded)
            {
                days |= UserSettings.DaysOfWeek.Thursday;
            }
            if (this.fridayIncluded)
            {
                days |= UserSettings.DaysOfWeek.Friday;
            }
            if (this.saturdayIncluded)
            {
                days |= UserSettings.DaysOfWeek.Saturday;
            }
            if (this.sundayIncluded)
            {
                days |= UserSettings.DaysOfWeek.Sunday;
            }
            SessionService.Instance.Settings.Days = (int)days;
            SessionService.Instance.Settings.BeginHour = this.hourMinimumPicker.SelectedIndex + 1;
            SessionService.Instance.Settings.EndHour = this.hourMaximumPicker.SelectedIndex + 1;
            SessionService.Instance.Settings.Frequency = AppGlobals.Notifications.FREQUENCIES.ElementAt(this._notifyPicker.SelectedIndex).Value;

            SessionService.Instance.Settings.Save();

            Device.BeginInvokeOnMainThread(() =>
               {
                   DisplayAlert("", "Your settings have been saved", "OK");
               });

            NotificationService notificationService = NotificationService.Instance;
            notificationService.RescheduleNotifications();

            // Navigate Back To Menu
            await Navigation.PopAsync(true);
        }

        #endregion
    }
}
