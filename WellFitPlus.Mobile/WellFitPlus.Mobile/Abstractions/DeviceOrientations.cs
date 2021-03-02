using System;
using Xamarin.Forms.Platform;


/* This File is used to define functionality that will be used with the Xamarin DependencyService
 * This File defines functionality that will be implemented on each native platform. This is how the DependencyService
 * works; you define abstractions in the shared project and then implement them in each native platform. You then
 * use DependencyService from the shared project along with these abstractions and Xamarin will handle which platform
 * code is called.
*/ 
namespace WellFitPlus.Mobile { 

	public enum DeviceOrientations
	{
		Undefined,
		Landscape,
		Portrait
	}

	public interface IDeviceOrientation
	{

		bool IsPortrait();
		void SetPortrait();
		void SetLandscape();

	}

}
