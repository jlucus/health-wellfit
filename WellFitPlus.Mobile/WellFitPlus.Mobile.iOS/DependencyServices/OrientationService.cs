using System;
using UIKit;
using Foundation;
using WellFitPlus.Mobile.iOS;


[assembly: Xamarin.Forms.Dependency (typeof (OrientationService))]
namespace WellFitPlus.Mobile.iOS
{
	public class OrientationService: IDeviceOrientation
	{
		// A parameterless constructor is needed in order to use DependencyService from Xamarin Forms
		public OrientationService()
		{
		}

		public bool IsPortrait() { 
			var currentOrientation = UIApplication.SharedApplication.StatusBarOrientation;
			bool isPortrait = currentOrientation == UIInterfaceOrientation.Portrait
				|| currentOrientation == UIInterfaceOrientation.PortraitUpsideDown;

			return isPortrait;
		}

		public void SetPortrait() { 
			((WellFitPlus.Mobile.iOS.AppDelegate)UIApplication.SharedApplication.Delegate).currentOrientation = UIInterfaceOrientationMask.Portrait;
			UIApplication.SharedApplication.SetStatusBarOrientation(UIInterfaceOrientation.Portrait, false);
		}

		public void SetLandscape() { 
			((WellFitPlus.Mobile.iOS.AppDelegate)UIApplication.SharedApplication.Delegate).currentOrientation = UIInterfaceOrientationMask.LandscapeRight;
			UIApplication.SharedApplication.SetStatusBarOrientation(UIInterfaceOrientation.LandscapeRight, false);
		}

	}
}

