using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using WellFitPlus.Mobile.Helpers;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.PlatformViews;
using WellFitPlus.Mobile.Services;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace WellFitPlus.Mobile.SharedViews
{
    public partial class Account : ContentPage
    {

        private NotificationService _notificationService = NotificationService.Instance;
        private UserSettings _settings;

        #region Initialization

        public Account()
        {
            InitializeComponent();
            //Apply Safe Area
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);

            _settings = UserSettings.GetExistingSettings();

            menuBar.LeftButton.Clicked += MenuButton_Clicked;
            menuBar.RightButton.Clicked += SaveButton_Clicked;
            submitButton.Clicked += SubmitButton_Clicked;
            submitDeregisterButton.Clicked += DeregisterButton_Clicked;

            eEmail.Completed += new EventHandler((sender, e) => { ePassword.Focus(); });
            ePassword.Completed += new EventHandler((sender, e) => { eNewPassword.Focus(); });
            eNewPassword.Completed += new EventHandler((sender, e) => { eConfirmPassword.Focus(); });

            // Set the company registration views based on if the user is registered with a company.
            if (!string.IsNullOrEmpty(_settings.CompanyName))
            {
                SetCompanyRegistrationViews(true, _settings.CompanyName);
            }
            else
            {
                SetCompanyRegistrationViews(false);
            }

            // Load Settings
            this.LoadSettings();
        }

        #endregion

        #region Page Lifecycle methods
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // If we are sitting on this page we should still get notifications prompted
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

        private void SaveButton_Clicked(object sender, EventArgs e)
        {
            this.SaveSettings();
        }

        private async void SubmitButton_Clicked(object sender, EventArgs e)
        {
            string groupCode = groupCodeInput.Text;

            RegisterCompanyUserResponse response = await RegisterGroupCode(groupCode);

            if (response != null)
            {
                // The request was successful!
                groupCodeInput.Text = ""; // Make sure the group code is cleared out.

                Device.BeginInvokeOnMainThread(() =>
                {
                    // Notify user and set any other values here.
                    _settings.CompanyName = response.CompanyName;

                    SetCompanyRegistrationViews(true, response.CompanyName);

                    DisplayAlert("Success", "You are now registered with company " + response.CompanyName, "OK");
                });
            }
            else
            {
                // There was an error processing the request.

                Device.BeginInvokeOnMainThread(() =>
                {
                    // Notify user here.
                    DisplayAlert("Error", "Please check code and internet connection then try again", "OK");

                });
            }
        }

        private async void DeregisterButton_Clicked(object sender, EventArgs e)
        {

            string companyName = _settings.CompanyName;


            // Ask user if they're sure they would like to deregister from the company.
            var answer = await DisplayAlert("Leave Company?",
                                            "Would you like to deregister this account from company: " + companyName + "?",
                                            "OK", "CANCEL");

            if (answer == false)
            {
                return; // The user made a mistake. Do not go further.
            }

            bool response = await DeregisterFromCompany();

            if (response)
            {
                // The request was successful!

                Device.BeginInvokeOnMainThread(() =>
                {
                    // Notify user and set any other values here.
                    SetCompanyRegistrationViews(false);

                    DisplayAlert("Success", "You have left company: " + companyName, "OK");
                });
            }
            else
            {
                // There was an error processing the request.

                Device.BeginInvokeOnMainThread(() =>
                {
                    // Notify user here.
                    DisplayAlert("Error", "Please check code and internet connection then try again", "OK");

                });
            }

        }

        #endregion

        #region Functions

        private void LoadSettings()
        {
            UserCredentials credentials = new UserCredentials();

            this.eEmail.Text = SessionService.Instance.User.Username;
            this.ePassword.Text = credentials.Password;
        }

        private async void SaveSettings()
        {
            try
            {
                // Validate Form
                bool isValid = await this.IsFormValid();
                if (isValid == false) { return; }

                // Create Web Service
                WebApiService service = new WebApiService(SessionService.Instance.Configuration);

                // Set Password
                ChangePassword pwd = new ChangePassword()
                {
                    OldPassword = this.ePassword.Text.Trim(),
                    NewPassword = this.eNewPassword.Text.Trim(),
                    ConfirmPassword = this.eConfirmPassword.Text.Trim()
                };

                // Change Password
                var info = await service.Post<ChangePassword>("Account/ChangePassword", pwd);

                if (info == System.Net.HttpStatusCode.OK)
                {
                    // Save User Credentials
                    UserCredentials credentials = new UserCredentials();
                    credentials.DeleteCredentials();
                    credentials.SaveCredentials(this.eEmail.Text.Trim(), pwd.NewPassword);

                    await Xamarin.Forms.Application.Current.MainPage.Navigation.PopAsync().ConfigureAwait(false);
                }
                else
                {
                    await DisplayAlert("Error", "An Error Occurred. Your settings have not been saved.", "OK");
                    await Xamarin.Forms.Application.Current.MainPage.Navigation.PopAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "An Error Occurred. Your settings have not been saved. Detail: " + ex.Message, "OK");
            }
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }

        private async Task<bool> IsFormValid()
        {
            // Check Passwords Match
            string firstPassword = this.eNewPassword.Text == null ? "" : this.eNewPassword.Text.Trim();
            string secondPassword = this.eConfirmPassword.Text == null ? "" : this.eConfirmPassword.Text.Trim();
            if (firstPassword != secondPassword)
            {
                await DisplayAlert("Error", "Passwords do not match", "OK");
                return false;
            }
            else if (firstPassword.Length == 0 || secondPassword.Length == 0)
            {
                await DisplayAlert("Error", "Password cannot be empty", "OK");
                return false;
            }

            // Validate Email
            string email = this.eEmail.Text == null ? "" : this.eEmail.Text.Trim();
            bool isValidEmail = this.IsValidEmail(email);
            if (isValidEmail == false)
            {
                await DisplayAlert("Error", "'" + email + "' is not a valid email address", "OK");
                return false;
            }

            // Check Name Supplied
            if (this.ePassword.Text == null || this.ePassword.Text.Trim().Length == 0)
            {
                await DisplayAlert("Error", "Password is required", "OK");
                return false;
            }

            return true;
        }

        private async Task<RegisterCompanyUserResponse> RegisterGroupCode(string groupCode)
        {
            var settings = UserSettings.GetExistingSettings();

            if (settings.UserId == Guid.Empty)
            {
                // Most likely this won't happen but we need to check anyway.
                return null;
            }

            var request = new RegisterCompanyUserRequest();

            request.UserId = settings.UserId;
            request.GroupCode = groupCode;

            string path = "Company/RegisterExistingUser";

            try
            {
                WebApiService service = new WebApiService(SessionService.Instance.Configuration);
                RegisterCompanyUserResponse response =
                    await service.PostRetrieve<RegisterCompanyUserRequest, RegisterCompanyUserResponse>(path, request);

                return response;
            }
            catch (Exception e)
            {
                App.Log("There was an error registering company code: " + e);
                return null;
            }

        }

        private async Task<bool> DeregisterFromCompany()
        {
            var settings = UserSettings.GetExistingSettings();

            if (settings.UserId == Guid.Empty)
            {
                // Most likely this won't happen but we need to check anyway.
                return false;
            }

            var request = new RegisterCompanyUserRequest();

            request.UserId = settings.UserId;
            request.GroupCode = null;

            string path = "Company/DeregisterUser";

            try
            {
                WebApiService service = new WebApiService(SessionService.Instance.Configuration);
                var response =
                    await service.Post<RegisterCompanyUserRequest>(path, request);

                if (response == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception e)
            {
                App.Log("There was an error registering company code: " + e);
                return false;
            }

        }

        private void SetCompanyRegistrationViews(bool isRegisteredWithGroup, string companyName = "")
        {

            groupSubscriptionLabel.IsVisible = !isRegisteredWithGroup;
            enterCodeLabel.IsVisible = !isRegisteredWithGroup;
            companyCodeInput.IsVisible = !isRegisteredWithGroup;
            submitButton.IsVisible = !isRegisteredWithGroup;

            subscribedWithGroupLabel.IsVisible = isRegisteredWithGroup;
            registeredCompanyNameLabel.IsVisible = isRegisteredWithGroup;
            submitDeregisterButton.IsVisible = isRegisteredWithGroup;
            registeredCompanyNameLabel.Text = companyName;

        }
        #endregion
    }
}
