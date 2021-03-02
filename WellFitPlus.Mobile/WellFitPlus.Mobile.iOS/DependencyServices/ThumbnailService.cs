using System;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using WellFitPlus.Mobile;
using WellFitPlus.Mobile.iOS;
using WellFitPlus.Mobile.Models;
using Foundation;
using AVFoundation;
using UIKit;
using CoreGraphics;
using CoreMedia;

[assembly: Xamarin.Forms.Dependency(typeof(ThumbnailService))]
namespace WellFitPlus.Mobile.iOS
{
	public class ThumbnailService: IThumbnailGetter
	{

		public static string DocumentsPath
		{
			get
			{
				var documentsDirUrl = 
					NSFileManager.DefaultManager.GetUrls(
						NSSearchPathDirectory.DocumentDirectory, 
						NSSearchPathDomain.User
					).Last();
				return documentsDirUrl.Path;
			}
		}

		public bool FileExists(string filename)
		{
			string path = NSSearchPath.GetDirectories(NSSearchPathDirectory.DocumentDirectory,NSSearchPathDomain.User)[0];
			path = path + "/" + filename;

			bool test = NSFileManager.DefaultManager.FileExists(path);

			return File.Exists(CreatePathToFile(filename));
		}

		public static string CreatePathToFile(string fileName)
		{
			return Path.Combine(DocumentsPath, fileName);
		}

		public ImageSource GetThumbnail(Video video, int secondsIntoVideo) {

			if (video.FileName == null) {
				// Most likely this video hasn't downloaded yet.
				return null;
			}
			int lastForwardSlashIndex = video.FileName.LastIndexOf('/');
			string filename = video.FileName.Substring(lastForwardSlashIndex + 1);

			string pathToFile = CreatePathToFile(filename);

			AVAsset asset = AVAsset.FromUrl(NSUrl.FromFilename(pathToFile));

			var imgGenerator = new AVAssetImageGenerator(asset);
			var error = new NSError();
			var actualTime = new CMTime(); // need for function but we aren't using
			CGImage cgImage = imgGenerator.CopyCGImageAtTime(new CMTime(secondsIntoVideo, 1), out actualTime ,out error);

			if (error == null)
			{
				UIImage uiImage = new UIImage(cgImage);
				ImageSource source = ImageSource.FromStream(() => uiImage.AsPNG().AsStream());
				return source;
			}

			return null;
		}
	}
}
