using System;
using System.ComponentModel;
using WellFitPlus.Mobile.Database;
using Xamarin.Forms;
using SQLite;

namespace WellFitPlus.Mobile.Models
{
    public class Video
    {
        [PrimaryKey]
        public Guid ID { get; set; }       // maps back to server's ID field

        [NotNull]
        public Guid UserID { get; set; }

        [NotNull]
        public string Title { get; set; }       // displayed on screen

        public string FileName { get; set; }    // maps to local file storage

		// Only used to determine whether a video was EVER played. Newly downloaded video will have a MinValue for this
        public DateTime LastPlayed { get; set; }

		public DateTime DownloadDate { get; set; }

		[NotNull]
        public bool IsFavorite { get; set; }
        
        [NotNull]
        public string Path { get; set; } // The url path to download the video from.

		[NotNull]
        public VideoType Type { get; set; }

		[NotNull]
        public string Tags { get; set; }

        [NotNull]
        public string Description { get; set; }

        [NotNull]
        public bool Active { get; set; } // Only used on the server side.

        [NotNull]
        public DateTime DateCreated { get; set; }

        [NotNull]
        public DateTime DateModified { get; set; }

        [NotNull]
        public DateTime DateUploaded { get; set; }

        [NotNull]
        public bool DownloadedSuccessfully { get; set; }

        //[NotNull]
        public bool Deleted { get; set; }

		[NotNull]
        public int ViewCount { get; set; }

		[NotNull, DefaultValue(false)]
		public bool IsWatched { get; set; }

        public enum VideoType
        {
            [Description("None")]
            None = -1,

            [Description("Intro")]
            Intro = 0,

            [Description("Trailer")]
            Trailer = 1,

            [Description("Exercise")]
            Exercise = 2
        }

        public Video() {
            IsFavorite = false;
            UserID = UserSettings.GetExistingSettings().UserId;
            ViewCount = 0;
        }

		public String GetFileNameWithFullPath()
		{

			// Due to the absolute file path changing each build/update on iOS we need to re-create
			// the file path each time and not rely on what is in the database.
			int lastForwardSlashIndex = this.FileName.LastIndexOf('/');
			string filename = FileName.Substring(lastForwardSlashIndex + 1);

#if __IOS__
			return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + filename;
#elif __ANDROID__
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/" + filename;
#endif
		}
    }
}
