using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Helpers;
using WellFitPlus.Mobile.Services;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace WellFitPlus.Mobile.SharedViews
{
    public partial class Register : ContentPage
    {
        #region Properties

        UserRegistration _userRegistration;

        #endregion

        #region Initialization

        public Register(UserRegistration userRegistration)
        {
            InitializeComponent();
            //Apply Safe Area
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
            this._userRegistration = userRegistration;

            this.termsLabel.GestureRecognizers.Add(new TapGestureRecognizer((view) =>
                OnLoginClicked(null, null)
            ));

            ePassword.Completed += new EventHandler((sender, e) => { eConfirmPassword.Focus(); });

            this.backButton.Clicked += this.OnBackClicked;
            this.registerButton.Clicked += this.OnRegisterClicked;
        }

        #endregion

        #region Events

        private async void OnRegisterClicked(object sender, EventArgs args)
        {

            // Validate Form
            bool isValid = await this.IsFormValid();
            if (isValid == false) { return; }

            // Create Web Service
            WebApiService service = new WebApiService(SessionService.Instance.Configuration);

            // Create User
            this._userRegistration.Password = this.ePassword.Text.Trim();
            this._userRegistration.ConfirmPassword = this.eConfirmPassword.Text.Trim();
            this._userRegistration.Role = "user";

            try
            {
                // Register User
                var httpCode = await service.Register(this._userRegistration);

                // Validate HTTP Code
                if (httpCode == System.Net.HttpStatusCode.OK)
                {
                    // Save User Credentials
                    UserCredentials credentials = new UserCredentials();
                    credentials.SaveCredentials(this._userRegistration.Email, this._userRegistration.Password);

                    // Save User Registration Date
                    UserSettings newSettings = UserSettings.GetExistingSettings();
                    newSettings.RegistrationDate = DateTime.Now;
                    newSettings.Save();

                    await DisplayAlert("Success!", "Thank you for Registering. You can now login using your new account.", "OK");
                }
                else
                {
                    await DisplayAlert("Registration Error", "Please check internet connection or try again later.", "OK");
                }
            }
            catch (SimpleHttpResponseException ex)
            {
                App.Log(string.Format("ERROR on Login:\r\n\t{0} (Status Code: {1})",
                                      ex.Message, (ex as SimpleHttpResponseException).StatusCode));
                var json = JObject.Parse(ex.Message);

                var title = @"Registration Error";
                var message = json["modelState"].Last.Last.Last.ToString();

                var okButton = @"OK";

                if (message == null)
                {
                    await DisplayAlert(title, message, okButton);
                }
                else
                {
                    await Xamarin.Forms.Application.Current.MainPage.DisplayAlert(title, message, okButton);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Registration Error",
                    "There was a problem with registering account. Please try again later",
                    "OK");
            }

            var login = new Login();
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(login, false);
            Xamarin.Forms.Application.Current.MainPage = new Xamarin.Forms.NavigationPage(login);
        }

        private void OnLoginClicked(object sender, EventArgs args)
        {
            var login = new Login();
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(login, false);
            Xamarin.Forms.Application.Current.MainPage = new Xamarin.Forms.NavigationPage(login);
        }

        private async void OnBackClicked(object sender, EventArgs args)
        {
            await Navigation.PopAsync(true);
        }

        #endregion

        #region Functions

        private async Task<bool> IsFormValid()
        {
            // Check Passwords Match And Not Empty
            string firstPassword = this.ePassword.Text == null ? "" : this.ePassword.Text.Trim();
            string secondPassword = this.eConfirmPassword.Text == null ? "" : this.eConfirmPassword.Text.Trim();
            if (firstPassword != secondPassword)
            {
                await DisplayAlert("Error", "Passwords do not match", "OK");
                return false;
            }
            else if (firstPassword.Length == 0)
            {
                await DisplayAlert("Error", "Password cannot be empty", "OK");
                return false;
            }
            else if (firstPassword.Length < 6)
            {
                await DisplayAlert("Error", "Password must be at least 6 characters long", "OK");
                return false;
            }
            else if (firstPassword.Length > 100)
            {
                await DisplayAlert("Error", "Password cannot be greater than 100 characters long", "OK");
                return false;
            }

            // Display Disclaimer
            bool result = await DisplayAlert(AppGlobals.Messages.DISCLAIMER_TITLE, AppGlobals.Messages.DISCLAIMER_MESSAGE, "I AGREE", "CANCEL");

            // Validate User Agrees
            if (result == false) { return false; }

            return true;
        }

        #endregion
    }
}
