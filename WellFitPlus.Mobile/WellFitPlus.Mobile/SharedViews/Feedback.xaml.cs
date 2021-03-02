using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

using WellFitPlus.Mobile.Services;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace WellFitPlus.Mobile.SharedViews
{
	public partial class Feedback : ContentPage
	{

		private NotificationService _notificationService = NotificationService.Instance;

		#region Initialization

		public Feedback()
        {
            InitializeComponent();
			//Apply Safe Area
			On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
			this.BackgroundImage = AppGlobals.Images.BLUE_GRADIENT_IMAGE;

            this.menuBar.LeftButton.Clicked += this.MenuButton_Clicked;
            this.SendFeedbackButton.Clicked += this.SendFeedback_Clicked;
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

        private void SendFeedback_Clicked(object sender, EventArgs e)
        {
            string url = "mailto:" + AppGlobals.Feedback.FEEDBACK_EMAIL_ADDRESS_TO
                + "?subject=" + System.Uri.EscapeUriString(AppGlobals.Feedback.FEEDBACK_EMAIL_SUBJECT)
                + "&body=" + System.Uri.EscapeUriString(eInput.Text);

            Device.OpenUri(new Uri(url));
        }

        #endregion
    }
}
