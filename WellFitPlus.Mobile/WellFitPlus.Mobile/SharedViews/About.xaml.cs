using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace WellFitPlus.Mobile.SharedViews
{
	public partial class About : ContentPage
	{
        #region Initialization

        public About ()
		{
			InitializeComponent();
            //Apply Safe Area
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
            this.BackgroundImage = AppGlobals.Images.BLUE_GRADIENT_IMAGE;

            this.menuBar.LeftButton.Clicked += this.MenuButton_Clicked;
        }

        #endregion
        
        #region Events

        private async void MenuButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(true);
        }
        
        #endregion
    }
}
