using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using WellFitPlus.Mobile.Models;


namespace WellFitPlus.Mobile
{
	public interface IThumbnailGetter
	{

		// iOS gracefully fetches the thumbnails but Android needs to be explicitly fetched on 
		// a separate thread
		#if __IOS__
		ImageSource GetThumbnail(Video video, int secondsIntoVideo);
		#elif __ANDROID__
		void SetThumbnail(WeakReference<Image> imageToSetThumbnail, Video video, int secondsIntoVideo);
		#endif
		
	}
}

