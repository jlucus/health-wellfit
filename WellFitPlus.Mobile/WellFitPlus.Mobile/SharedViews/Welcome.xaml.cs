using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WellFitPlus.Mobile.Models;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace WellFitPlus.Mobile.SharedViews
{
    public partial class Welcome : ContentPage
    {
        #region Initialization

        public Welcome()
        {
            InitializeComponent();
            //Apply Safe Area
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
            this.loginButton.Clicked += this.OnLoginClicked;

            eFirstname.Completed += new EventHandler((sender, e) => { eLastname.Focus(); });
            eLastname.Completed += new EventHandler((sender, e) => { eEmail.Focus(); });
            eEmail.Completed += new EventHandler((sender, e) => { groupCodeInput.Focus(); });
            groupCodeInput.Completed += new EventHandler((sender, e) => { nextButton.Focus(); });
        }

        #endregion

        #region Events

        private async void OnNextClicked(object sender, EventArgs args)
        {
            // Check First Name Supplied
            if (this.eFirstname.Text == null || this.eFirstname.Text.Trim().Length == 0)
            {
                await DisplayAlert("Error", "First name is required", "OK");
                return;
            }
            else if (this.eFirstname.Text.Trim().Length > 50)
            {
                await DisplayAlert("Error", "First name cannot be greater than 50 characters long", "OK");
                return;
            }
            else if (this.eLastname.Text == null || this.eLastname.Text.Trim().Length == 0)
            {
                await DisplayAlert("Error", "Last name is required", "OK");
                return;
            }
            else if (this.eLastname.Text.Trim().Length > 50)
            {
                await DisplayAlert("Error", "Last name cannot be greater than 50 characters long", "OK");
                return;
            }

            // Validate Email
            string email = this.eEmail.Text == null ? "" : this.eEmail.Text.Trim();
            bool isValidEmail = this.IsValidEmail(email);
            if (isValidEmail == false)
            {
                await DisplayAlert("Error", "'" + email + "' is not a valid email address", "OK");
                return;
            }

            string name = this.eFirstname.Text.ToString();
            UserRegistration userRegistration = new UserRegistration()
            {
                FirstName = this.eFirstname.Text.Trim(),
                LastName = this.eLastname.Text.Trim(),
                Email = this.eEmail.Text.Trim(),
                GroupCode = groupCodeInput.Text.Trim()
            };

            // Navigate to Register Page
            var register = new Register(userRegistration);
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(register, false);
			await Navigation.PushAsync(register);
        }

        private async void OnLoginClicked(object sender, EventArgs args)
        {
            var login = new Login();
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(login, false);
            Xamarin.Forms.Application.Current.MainPage = new Xamarin.Forms.NavigationPage(login);
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }

        #endregion
    }
}
