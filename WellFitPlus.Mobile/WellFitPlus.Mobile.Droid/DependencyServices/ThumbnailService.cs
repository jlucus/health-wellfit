using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Android;
using Android.Media;
using Android.Graphics;
using WellFitPlus.Mobile.Droid;
using WellFitPlus.Mobile.Models;
using System.Linq;
using Android.OS;

[assembly: Xamarin.Forms.Dependency(typeof(ThumbnailService))]
namespace WellFitPlus.Mobile.Droid
{
	public class ThumbnailService: IThumbnailGetter
	{

		private string _folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

        public void SetThumbnail(
			WeakReference<Xamarin.Forms.Image> weakImage, 
			Video video, 
			int secondsIntoVideo
		) {

			Task.Run(() => { 
			
				MemoryStream memoryStream = new MemoryStream();
				Bitmap thumb = null;

				try
				{
					// Create Media Retriever
					MediaMetadataRetriever retriever = new MediaMetadataRetriever();

					// Set Retriever Datasource
					retriever.SetDataSource(video.FileName);
					int timeInSeconds = secondsIntoVideo;

					byte[] bitmapData;
					using (var stream = new MemoryStream())
					{
						// Get Thumbnail 1 Second Into Video
						thumb = retriever.GetFrameAtTime(timeInSeconds * 1000000, Android.Media.Option.ClosestSync);
						thumb.Compress(Bitmap.CompressFormat.Png, 40, stream);
						bitmapData = stream.ToArray();
					}

					ImageSource imageSource = ImageSource.FromStream(() => new MemoryStream(bitmapData));

					// Recycle/Dispose Thumbnail And Stream 
					thumb.Recycle();
					memoryStream.Close();

					thumb = null;
					memoryStream = null;

					if (weakImage != null)
					{
						Xamarin.Forms.Image uiImage;

						bool isSuccess = weakImage.TryGetTarget(out uiImage);

						if (isSuccess)
						{
							Device.BeginInvokeOnMainThread(() =>
							{
								uiImage.Source = imageSource;
							});
						}
					}
				}
				catch (Java.IO.FileNotFoundException e)
				{
					App.Log("File Not Found Exception : check directory path. Detail: " + e.ToString());
				}
				catch (Java.IO.IOException e)
				{
					App.Log("IOException while closing the stream. Detail: " + e.ToString());
				}
				catch (Exception e)
				{
					App.Log("General exception while creating thumbnail. Detail: " + e.ToString());

				}
				finally
				{
					if (memoryStream != null)
					{
						memoryStream.Close();
					}
					if (thumb != null)
					{
						thumb.Recycle();
					}
				}
			});
		}
    }
}

