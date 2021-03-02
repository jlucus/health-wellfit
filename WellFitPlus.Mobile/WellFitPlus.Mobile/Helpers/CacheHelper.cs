using System;
using System.Linq;
using System.Collections.Generic;

using Xamarin.Forms;

using WellFitPlus.Mobile;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitMobile.FileSystem.Directory.Entities;


namespace WellFitPlus.Mobile
{
	public class CacheHelper
	{

		public static async void ClearCache()
		{
            try
            {
                int videosDeleted = 0;

                // Create Directory
                DirectoryObject dir = new DirectoryObject(AppGlobals.Settings.MOBILE_DIRECTORY);

                // Get Video Records
                VideoRepository repo = new VideoRepository();
                var videos = repo.GetPlayableVideos();

                // Get Video Files
                var videoFiles = dir.Files.Where(file => file.Exists == true
                    && AppGlobals.Settings.PERMITTED_VIDEO_EXTENSIONS.Any(ext => ext.ToLower() == file.Extension.ToLower())
                    && videos.Any(video => video.GetFileNameWithFullPath() == file.FilePath)).ToList();

                // If No Videos To Delete
                if (videos.Count == 0 || videoFiles.Count == 0)
                {
                   // await Application.Current.MainPage.DisplayAlert("Notice", "No videos to delete", "OK").ConfigureAwait(false);
                    return;
                }

                // Loop Videos
                foreach (var video in videos)
                {
                    try
                    {
                        // Get Video File
                        var file = videoFiles.Where(f => f.FilePath == video.GetFileNameWithFullPath()).FirstOrDefault();

                        if (file == null)
                        {
                            continue;
                        }

                        // Delete File
                        AppGlobals.ResultType deleteFileResult = file.Delete();

                        if (deleteFileResult != AppGlobals.ResultType.Success)
                        {
                            throw new Exception("Could not delete file");
                        }

                        // Update Video Record
                        video.Deleted = true;
                        int recordResult = repo.UpdateVideo(video);

                        if (recordResult > 0)
                        {
                            videosDeleted += 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        App.Log("An Error Occurred Deleting Video " + video.Title);
                    }
                }
            }
            catch (Exception ex)
            {

            }

		}

	}
}

