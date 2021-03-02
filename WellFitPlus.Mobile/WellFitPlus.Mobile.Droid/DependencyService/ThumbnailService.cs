using System;
using System.IO;
using Xamarin.Forms;
using Android.Media;
using Android.Graphics;
using Android.Provider;
using WellFitPlus.Mobile.Droid;
using WellFitPlus.Mobile.Models;
using System.Linq;
using Java.IO;
using static Android.Graphics.Bitmap;

[assembly: Xamarin.Forms.Dependency(typeof(ThumbnailService))]
namespace WellFitPlus.Mobile.Droid
{
	public class ThumbnailService: IThumbnailGetter
	{
        #region Initialization

        public ThumbnailService()
		{
		}

        #endregion

        #region Functions

        public ImageSource GetThumbnail(Video video, int secondsIntoVideo) {

            try
            {
                // ImageSource To Return
                ImageSource imageSource = null;

                // Get Thumbnail Save Directory Via Video Absolute Path
                string dir = string.Join("/", video.FileName.Split('/').Take(video.FileName.Split('/').Count() - 1));

                // Create Thumbnail Path Using Directory And Video Title
                string bitmapPath = System.IO.Path.Combine(dir, video.Title + ".png");

                // If Thumbnail Already Exists For Video
                //if (System.IO.File.Exists(bitmapPath))
                //{
                //    // Return Existing Video Thumbnail
                //    imageSource = ImageSource.FromFile(bitmapPath);
                //}
                //else
                {
                    // Create Video Thumbnail
                    bool bitmapCreated = CreateBitmap(video);

                    // Validation
                    if (bitmapCreated == true)
                    {
                        // Set ImageSource From Video Thumbnail Absolute Path
                        imageSource = ImageSource.FromFile(bitmapPath);
                    }
                }

                return imageSource;
            }
            catch (Exception ex)
            {
                App.Log("A Critical Exception Occurred Creating Thumbnail For Video '" + video.Title + "'. Detail: " + ex.ToString());
                return null;
            }
		}

        private bool CreateBitmap(Video video)
        {
            try
            {
                // Get Thumbnail Save Directory Via Video Absolute Path
                string dir = string.Join("/", video.FileName.Split('/').Take(video.FileName.Split('/').Count() - 1));

                // Create Thumbnail Path Using Directory And Video Title
                string bitmapPath = System.IO.Path.Combine(dir, video.Title + ".png");

                // Create Thumbnail FileStream
                System.IO.FileStream streamThumbnail = new System.IO.FileStream(bitmapPath, FileMode.OpenOrCreate);
                Bitmap thumb;

                // Create Media Retriever
                MediaMetadataRetriever retriever = new MediaMetadataRetriever();
                try
                {
                    // Set Retriever Datasource
                    retriever.SetDataSource(video.FileName);
                    int timeInSeconds = 1;

                    // Get Thumbnail 1 Second Into Video
                    thumb = retriever.GetFrameAtTime(timeInSeconds * 1000000, MediaMetadataRetriever.OptionClosestSync);

                    // Compress Stream Into PNG Format
                    //thumb.Compress(CompressFormat.Png, 80, streamThumbnail);
                    thumb.Compress(CompressFormat.Png, 40, streamThumbnail);

                    // Recycle/Dispose Thumbnail And Stream 
                    thumb.Recycle();                                        
                    streamThumbnail.Close();

                    return true;
                }
                catch (Exception ex)
                {
                    App.Log("A Critical Exception Occurred Creating Thumbnail For Video '" + video.Title + "'. Detail: " + ex.ToString());
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

            return false;
        }

        #endregion
    }
}

