using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using WellFitPlus.Mobile.Helpers;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.PlatformViews;
using WellFitPlus.Mobile.Services;
using Xamarin.Forms;
using System.Globalization;
using System.Threading;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace WellFitPlus.Mobile.SharedViews
{
    public partial class Login : ContentPage
    {
        #region Private Fields
        private ActivityIndicator _waitIndicator;
        private NotificationService _notificationService = NotificationService.Instance;
        #endregion

        public Login()
        {
            InitializeComponent();
            //Apply Safe Area
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
            // Save Credentials If They Do Not Exist
            UserCredentials credentials = new UserCredentials();
            if (credentials.UserName != null && credentials.UserName != ""
                && credentials.Password != null && credentials.Password != "")
            {
                eUsername.Text = credentials.UserName;
                ePassword.Text = credentials.Password;

                // Attemp Auto Login
                this.OnLoginClicked(null, null);
            }

            eUsername.Completed += new EventHandler((sender, e) => { ePassword.Focus(); });
            ePassword.Completed += new EventHandler((sender, e) => { LoginButton.Focus(); });

        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private async void OnLoginClicked(object sender, EventArgs args)
        {

            LoginButton.IsEnabled = false;

            try
            {

                WebApiService service = new WebApiService(SessionService.Instance.Configuration);
                AuthToken token = await service.Login(eUsername.Text, ePassword.Text).ConfigureAwait(false);

                // Save Credentials If They Do Not Exist
                UserCredentials credentials = new UserCredentials();
                credentials.SaveCredentials(eUsername.Text, ePassword.Text);

                SessionService.Instance.AuthToken = token.AccessToken;
                SessionService.Instance.Issued = Convert.ToDateTime(token.IssuedAt);
                SessionService.Instance.Expires = Convert.ToDateTime(token.ExpiresAt);

                SessionService.Instance.User = new UserProfile
                {
                    UserID = Guid.Parse(token.UserID),
                    Username = token.Username
                };

                App.Log(string.Format("User logged in: {0}", SessionService.Instance.User.Username));
                App.Log(string.Format("Started session: {0}", SessionService.Instance));

                var info = await service.Get<UserInfo>("Account/UserInfo");
                App.Log(string.Format("User Info: {0}", info));

                Device.BeginInvokeOnMainThread(() =>
                {
                    // Upon each successfull login we need to set the registration date.
                    UserSettings settings = UserSettings.GetSettings(SessionService.Instance.User.UserID);
                    var datetime = token.RegistrationDate.Split(' ');
                    string date = datetime[0];
                    var d1 = date.Split('/');
                    var date1 = d1[0];
                    var date2 = d1[1];
                    var date3 = d1[2];
                    var time = datetime[1];
                    var AMPM = datetime[2];
                    var exacttime = time + " " + AMPM;
                    var correctdate = date2 + "/" + date1 + "/" + date3;
                    var correctdateandtime = correctdate + " " + exacttime;
                    settings.RegistrationDate = Convert.ToDateTime(correctdateandtime);
                   
                    settings.CompanyName = token.CompanyName;
                    settings.Save();

                    SessionService.Instance.Settings = settings;

                    App.Log(string.Format("User {0}", SessionService.Instance.Settings));

                    NotificationState notificationState = _notificationService.HasRecievedNotification();

                    if (notificationState == NotificationState.Background)
                    {
                        _notificationService.StartVideoPlaybackIfNewNotificationExists();

                    }
                    else
                    {
                        var profile = new Profile();
                        Xamarin.Forms.NavigationPage.SetHasNavigationBar(profile, false);
                        Xamarin.Forms.Application.Current.MainPage = new Xamarin.Forms.NavigationPage(profile);
                    }
                });

            }
            catch (SimpleHttpResponseException ex)
            {
                //this._waitIndicator.IsRunning = false;

                App.Log(string.Format("ERROR on Login:\r\n\t{0} (Status Code: {1})", ex.Message, (ex as SimpleHttpResponseException).StatusCode));
                var json = JObject.Parse(ex.Message);

                var title = @"Login Error";
                var message = @"Please try again.";
                var okButton = @"OK";

                //Note: Could We Strongly Type The Following Or Implement Constants To Avoid Null Reference Exceptions In Case Of Changes (Case Sensitivity or anything)
                switch (json["error"].ToString())
                {
                    case "invalid_grant":
                        title = @"Invalid Login";
                        message = json["error_description"].ToString();
                        break;

                    //TODO: what other messages do we get back ?
                    default:
                        break;
                }

                //Display Error Alert
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Xamarin.Forms.Application.Current.MainPage.DisplayAlert(title, message, okButton);
                });
            }
            catch (Exception e)
            {
                App.Log(string.Format("UNKNOWN ERROR on Login: {0}", e.Message));

                //Display Error Alert
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Xamarin.Forms.Application.Current.MainPage.DisplayAlert("ERROR", "An Unhandled Exception Ocurred. Detail: " + e.Message.ToString(), "OK");
                });

                //this._waitIndicator.IsRunning = false;                
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                LoginButton.IsEnabled = true;
            });
        }

        private void OnSignupClicked(object sender, EventArgs args)
        {
            //this._waitIndicator.IsRunning = true;

            // Navigate To Welcome Page
            var welcome = new Welcome();
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(welcome, false);
            Xamarin.Forms.Application.Current.MainPage = new Xamarin.Forms.NavigationPage(welcome);
        }

    }
}
