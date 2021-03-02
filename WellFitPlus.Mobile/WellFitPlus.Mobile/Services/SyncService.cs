using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WellFitPlus.Mobile.Helpers;
using WellFitMobile.FileSystem.Directory.Entities;
using WellFitMobile.FileSystem.File.Entities;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitPlus.Mobile.Models;
using Xamarin.Forms;

namespace WellFitPlus.Mobile.Services
{
    public class SyncService
    {

		#region Static Fields
		private static SyncService _instance;
		#endregion

		#region Static Properties
		public static SyncService Instance {
			get {
				if (_instance == null) {
					_instance = new SyncService();
				}
				return _instance;
			}
		}
		#endregion

		public static bool IsConnectedToWifi()
		{
			bool isConnected = false;
		#if __IOS__
		            isConnected = iOS.NetworkHelper.IsDeviceConnectedToWiFi();
		#endif
		#if __ANDROID__
		            isConnected = Droid.NetworkHelper.IsDeviceConnectedToWiFi();
		#endif
			return isConnected; 
		}
		 
		#region Private Fields
        private Timer _timer;
        private UserSettings _settings;
        private List<Video> _videos;
        private bool _isSyncing = false;
		private bool _isDownloadingVideos;
		private DateTime _lastSyncTime = DateTime.MinValue;
		private ActivitySessionRepository _activityRepo = ActivitySessionRepository.Instance;

		private int _syncDelay = 20; // In minutes. Period of time required between syncs.
		private int _videosToDownload = 0; // Keeps track of how many videos need to finish downloading to notify user.
		private string _videosDownloadingProgressString = "";
		#endregion

		#region Public Fields
        public static int SyncCheckCount = 0;
        public static int SyncCount = 0;
        public static int DeletedVideoCount = 0;
        public static int DownloadedVideoCount = 0;
        public static int SynchronizationExceptions = 0;
        #endregion

		public bool IsSyncing { 
			get { 
				
				lock(this)
				{
					return _isSyncing;
				}
			}
		}

		public bool IsDownloadingVideos { 
			get {
				lock(this)
				{
					return _isDownloadingVideos;
				}
			}
		}

        #region Initialization

        private SyncService()
        {
            App.Log("Sync Service Instance Created");
        }
                   
        #endregion

        #region Sync

		public bool CheckCanSync()
        {
            try
            {

                SyncCheckCount += 1;

                // Check Can Sync
                bool canSync = this.CheckCanSyncViaSettings();
                if (canSync == false) {
					App.Log("Syncing cancelled.");

					return canSync;
				}

                // Start Sync
                this.SyncWithServer();

				return canSync;
            }
            catch (Exception ex)
            {
                App.Log("An Error During Syncing. Detail: " + ex.ToString());
				return false;
            }

        }

        private bool CheckCanSyncViaSettings()
        {
			// Reload User Settings Every Interval To Ensure Latest Values
			_settings = UserSettings.GetExistingSettings();

            // Validation
			if (IsSyncing) { 
				return false; 
			}

			// TODO: PRODUCTION: Uncomment below check when Well Fit is ready to submit to the app store.
			// If the user is not within their trial period or has not purchased a subscription then skip the sync.
			//if (!IsSubscribedOrInTrialPeriod(_settings.RegistrationDate)){
			//	EmailLogger.Instance.AddLog("SyncService: User is not subscribed. Cancelling sync");

			//	return false;
			//}
            
            // Check Only Download On WiFi
            if (_settings.WifiDownloadOnly)
            {
                // If WiFi Is Not Connected
                if (IsConnectedToWifi() == false)
                {
                    App.Log("Syncing Set To False");
					SetIsSyncing(false);

                    return false;
                }
            }

			//// Limit the number of syncs a user can do in a given day.
			if (!IsSyncDelayComplete()) {
				return false;
			} 

			return true;
        }

        private async void SyncWithServer()
        {
         
			lock(this)
			{
				if (IsSyncing)
				{
					return;
				}
				SetIsSyncing(true);
			}

            App.Log("Starting Sync Service");

            // Create Empty Video
            var idVideo = new Video() { 
				ID = Guid.Empty, 
				UserID = UserSettings.GetExistingSettings().UserId, 
				Path = "", 
				DownloadedSuccessfully = true 
			};
            _videos = new List<Video>() { idVideo };
            _videos.AddRange(new VideoRepository().GetVideos());

			// Create Service
			WebApiService webService = new WebApiService(SessionService.Instance.Configuration);

			// Set up the request
			var request = new SyncRequest();

			// Set User settings data
			var settings = UserSettings.GetExistingSettings();
			request.UserID = settings.UserId;
			request.CacheSize = settings.CacheSize;

			// Set the list of activities not uploaded
			ActivitySessionRepository activityRepo = ActivitySessionRepository.Instance;
			var totalActivityCount = activityRepo.GetActivities(DateTime.MinValue).Count;// For debugging
			List<ActivitySession> nonUploadedActivities = activityRepo.GetActivitiesNotUploaded();

			// Make sure all activities actually have Guids (due to a SQLite database migration);
			nonUploadedActivities = ConvertOldActivitiesToNewActivities(nonUploadedActivities, activityRepo);

			try
			{
				// We need to convert the list of non-uploaded activities to something the server can use.
				// We also need to preserve the ActivitySession list above to be adjusted later, this is why we
				// don't directly cast the list here. If we did then the SQLite ID's would get wiped out from the list.
				request.ListOfNewActivities = 
					activityRepo.GetActivitiesNotUploaded().Select(a => (ServerActivitySession)a ).ToList();
			}
			catch {
				App.Log("SyncService: Could not cast activities to server activities");
			}

			// Set the list of current videos that are not deleted
			VideoRepository videoRepo = new VideoRepository();

			request.ListOfCurrentVideos = videoRepo.GetVideos().Where(v => 
			                                                          v.Deleted == false
			                                                          && v.DownloadedSuccessfully == true
		                                                         ).ToList();

			try
			{
				SyncResponse response = await webService.PostRetrieve<SyncRequest, SyncResponse>("Video/Sync", request);

				// If we got here that means the request was successfull. Mark the activities as uploaded.
				foreach (var activity in nonUploadedActivities)
				{
					activity.HasBeenUploaded = true;
					activityRepo.UpdateActivity(activity);
				}

				foreach (var video in response.VideosToDelete) {
					// Make sure that all videos have something for the Tags property so they'll actually
					// save in the local database.

					video.Tags = video.Tags == null ? "" : video.Tags;
				}

				this.DeleteVideos(response.VideosToDelete);

				if (response.VideosToDownload.Count > 0)
				{
					// The Bingo has been processed on the server.

					// Before going any further we need to make sure that any video that has failed to download previously
					// is marked as deleted. This is so we don't inadvertantly download those videos when the app checks
					// if there were any failed downloads. This would cause the app to download more than the cache size.
					// This is only the case if we got a new list of videos from the server. If the server isn't telling
					// us to download new videos then the videos that are marked as unsuccessfully downloaded are correct.
					var failedDownloadedVideos = videoRepo.GetVideos().Where(v => 
				                                                         v.DownloadedSuccessfully == false
					                                                         && v.Deleted == false
				                                                        ).ToList();

					foreach (var video in failedDownloadedVideos) {
						video.Deleted = true;
						videoRepo.UpdateVideo(video);
					}
				}

				// We need to explicitly save each video record we need to download in case there is an error
				// or in case the user closes the app before syncing is canceled. This allows the app to re-try
				// to download any non-downloaded videos.
				foreach (var video in response.VideosToDownload) {
					video.DownloadedSuccessfully = false;
					video.Tags = video.Tags == null ? "" : video.Tags;
					videoRepo.InsertOrUpdateVideo(video);
				}

				this.DownloadVideos(response.VideosToDownload);

				// This will only be updated if the server has actually recalculated the bingo.
				if (response.TopMostFrequentVideos.Count > 0) {
					var frequentVideoRepo = FrequentVideoRepository.Instance;
					frequentVideoRepo.DeleteAllRecords(); // We never want old records here.

					foreach (var video in response.TopMostFrequentVideos)
					{
						FrequentVideo videoToAdd = new FrequentVideo
						{
							Id = 0,
							VideoID = video.ID
						};

						frequentVideoRepo.AddFrequentVideo(videoToAdd);
					}
				}

				// Finally we need to synchronize the mobile device's ActivitySession table with the server's
				// Activity table.
				// This needs to be processed on a separate thread due to large number of calculations.
				await Task.Run(() => SyncServerActivitiesWithLocalActivities(response.AllUserActivities));
                
				SetLastSyncTime(DateTime.Now); // Only set last sync time upon success

			}
			catch (SimpleHttpResponseException ex)
			{
				SynchronizationExceptions += 1;
				App.Log("Error getting SyncResponse from server");
                SetIsSyncing(false); // Should normally be set inside DownloadVideos call
			}
        }

		/// <summary>
		/// Returns whether or not a user should have access to new data. The user should have access to new content
		/// if they are within the 14 day trial period or if they have purchased a subscription.
		/// </summary>
		/// <returns><c>true</c>, if user should have access to new content <c>false</c> otherwise.</returns> 
		private bool IsSubscribedOrInTrialPeriod(DateTime userRegistrationDate) {
			int trialPeriodLength = AppGlobals.TRIAL_PERIOD_DURATION; // In Days
			bool isWithinTrialPeriod = false;
			bool isSubscribed = false;

			// Check if the user is within their trial period
			DateTime now = DateTime.Now;
			DateTime trialExpirationDate = userRegistrationDate.AddDays(trialPeriodLength);
			if (now.CompareTo(trialExpirationDate) < 0) {
				isWithinTrialPeriod = true;
			}

			// Check to see if the user has puchased a subscription
			InAppPurchase iapService = DependencyService.Get<InAppPurchase>();
			isSubscribed = iapService.HasPurchasedSubscription();

			return isWithinTrialPeriod || isSubscribed;
		}
		#endregion

        #region Delete

		private AppGlobals.ResultType DeleteVideos(List<Video> videosToDelete)
		{
			AppGlobals.ResultType deleteResult = AppGlobals.ResultType.Success;

			try
			{
				// Get Videos To Download
				App.Log("Deleting Videos: \r\n" + String.Join("\r\n", videosToDelete.Select(v => v.Title).ToArray()));

				DeletedVideoCount += videosToDelete.Count;

				// Loop Videos
				// TODO: Fix error check. This func will not fail unless all videos fail to delete.
				foreach (var video in videosToDelete)
				{
					// Delete Video
					deleteResult = this.DeleteVideo(video);
				}

				if (videosToDelete.Count == 0)
				{
					App.Log("No Videos To Delete");
				}
				else
				{
					App.Log(videosToDelete.Count + " Videos Deleted Successfully");
				}
			}
			catch (Exception ex)
			{
				SynchronizationExceptions += 1;
				App.Log("ERROR Occurred Deleting The Videos. Detail: " + ex.ToString());
				deleteResult = AppGlobals.ResultType.Failure;
			}

			return deleteResult;
		}

        private AppGlobals.ResultType DeleteVideo(Video video)
        {
            try
            {
                // Load File
                string filename = Path.Combine(AppGlobals.Settings.MOBILE_DIRECTORY, video.Title.ToString());
                FileObject file = new FileObject(filename);

                // Delete File
                var result = file.Delete();

                // If Delete Successful
                if (result == AppGlobals.ResultType.Success)
                {
					// TODO: FIX, if a video file doesn't exist then the record will never be marked as deleted.
                    // Update Video Deleted Flag
                    VideoRepository repo = new VideoRepository();
                    video.Deleted = true;
                    video.FileName = filename;
                    repo.InsertOrUpdateVideo(video);
                }

                return result;
            }
            catch (Exception ex)
            {
                SynchronizationExceptions += 1;
                App.Log("ERROR Occurred Deleting The Video. Detail: " + ex.ToString());
                return AppGlobals.ResultType.Failure;
            }
        }

        #endregion

        #region Download Videos
        
        private async void DownloadVideos(List<Video> videosToDownload)
        {
			try
			{

				_videosToDownload = videosToDownload.Count;

				App.Log("Downloading Videos");

				MessagingCenter.Send<string, List<Video>>("", AppGlobals.Events.SYNC_DOWNLOAD_VIDEOS_STARTED, _videos);

				// Create Service
				WebApiService service = new WebApiService(SessionService.Instance.Configuration);

				DownloadedVideoCount += videosToDownload.Count;

				// If Theres Videos To Download
				if (videosToDownload.Count > 0)
				{
					SetIsDownloadingVideos(true);
					SyncCount += 1;
					App.Log("Downloading " + videosToDownload.Count + " Videos: \r\n\t\t" + String.Join("\r\n\t\t\t", videosToDownload.Select(video => video.Title)));

					// Get Filenames And Trigger Download Starting
					List<string> filenames = new VideoRepository().GetVideos().Select(v => v.Title).ToList();

					int sumOfVideosToDownload = videosToDownload.Count;
					for (var videoIndex = 0; videoIndex < sumOfVideosToDownload; videoIndex++) {
						var video = videosToDownload[videoIndex];
						await this.DownloadVideo(video, videoIndex + 1, sumOfVideosToDownload);
					}

					// Trigger Download Completed
					filenames = new VideoRepository().GetVideos().Select(v => v.Title).ToList();

					MessagingCenter.Send<string, List<Video>>("", AppGlobals.Events.SYNC_DOWNLOAD_VIDEOS_COMPLETED, _videos);
					App.Log("SYNC_DOWNLOAD_VIDEOS_COMPLETED");
				}

			}
			catch (Exception ex)
			{
				SynchronizationExceptions += 1;
				App.Log("ERROR Occurred Downloading The Videos. Detail: " + ex.ToString());
				MessagingCenter.Send<string, List<string>>("", AppGlobals.Events.SYNC_DOWNLOAD_VIDEOS_COMPLETED, new List<string>());
			}
			finally {
				// Reset Syncing Flag
				SetIsSyncing(false);
				SetIsDownloadingVideos(false);
				SetDownloadProgressString("");

				Device.BeginInvokeOnMainThread(() =>
				{
					MessagingCenter.Send<object>(this, AppGlobals.Events.NOTIFY_DOWNLOADING_COMPLETE);

				});
			}
        }
        
		/// <summary>
		/// Downloads the video.
		/// 
		/// NOTE: Download errors are handled by setting the "DownloadedSuccessfully" flag inside a Video object
		/// 	  to false and then attempting to re-download them from the profile page "asyncronously".
		/// </summary>
		/// <param name="video">Video.</param>
        private async Task DownloadVideo(Video video, int videoIndex, int sumOfVideosToDownload)
        {
			// Invoke On Main UI Thread
			Device.BeginInvokeOnMainThread(() =>
			{
				// Notify any listeners of the progress of the video downloads.
				string downloadProgressString =
					string.Format("{0} of {1}: {2}", videoIndex, sumOfVideosToDownload, video.Title);
				SetDownloadProgressString(downloadProgressString);
				MessagingCenter.Send<object, string>(this, AppGlobals.Events.NOTIFY_DOWNLOADING_PROGRESS, downloadProgressString);

			});
			App.Log("Downloading Video " + video.Title);

			string filename = video.Path.Split('/').Last().Trim();
			string localFilename = Path.Combine(AppGlobals.Settings.MOBILE_DIRECTORY, filename);

			using (var webClient = new WebClient()) {
				try
				{
					await webClient.DownloadFileTaskAsync(video.Path, localFilename);
					video.DownloadedSuccessfully = true;
					video.DownloadDate = DateTime.Now;
				}
				catch (WebException we)
				{
					video.DownloadedSuccessfully = false;
					SynchronizationExceptions += 1;
				}
				finally {
					// Change Path To Mobile Path

					video.FileName = localFilename;
					video.LastPlayed = DateTime.MinValue; // Needed for proper bingo functionality 
					video.Tags = video.Tags == null ? "" : video.Tags;
					video.Description = video.Description == null ? "" : video.Description;
					video.ViewCount = 0;
				}
			}
			// Set Video Uploaded Successful Flag And Update Model
			VideoRepository repo = new VideoRepository();
			var repoResult = repo.InsertOrUpdateVideo(video);

			// Validation
			if (repoResult <= 0)
			{
				App.Log("ERROR: Video " + video.Title + " Could Not Be Inserted Into Database");
			}

			return;
        }

		/// <summary>
		/// Tries to download the failed videos if any exist
		/// </summary>
		public async void TryDownloadFailedVideos()
		{

			if (!IsSyncing)
			{


				try
				{
					// Reload User Settings Every Interval To Ensure Latest Values
					_settings = UserSettings.GetExistingSettings();

					// Check Only Download On WiFi
					if (_settings.WifiDownloadOnly)
					{
						// If WiFi Is Not Connected
						if (IsConnectedToWifi() == false)
						{
							App.Log("Syncing Set To False");
							SetIsSyncing(false);

							return;
						}
					}
                    SetIsSyncing(true);
					SetIsDownloadingVideos(true);

					App.Log("Downloading Videos");


					List<Video> videosToDownload =
						new VideoRepository().GetVideos().Where(v => v.DownloadedSuccessfully == false && 
						                                        v.Deleted == false).ToList();

					_videosToDownload = videosToDownload.Count;

					// If Theres Videos To Downloa
					if (videosToDownload.Count > 0)
					{
						App.Log("Downloading " + videosToDownload.Count + " Videos: \r\n\t\t" + String.Join("\r\n\t\t\t", videosToDownload.Select(video => video.Title)));

						// Get Filenames And Trigger Download Startin
						List<string> filenames = new VideoRepository().GetVideos().Select(v => v.Title).ToList();

						int sumOfVideosToDownload = videosToDownload.Count;
						for (var videoIndex = 0; videoIndex < sumOfVideosToDownload; videoIndex++) {
							var video = videosToDownload[videoIndex];
							await this.DownloadVideo(video, videoIndex + 1, sumOfVideosToDownload);
						}

						// Trigger Download Complete
						filenames = new VideoRepository().GetVideos().Select(v => v.Title).ToList();
					}
					else {
						App.Log("No failed videos to download");
					}
				}
				catch (Exception ex)
				{
					App.Log("ERROR Occurred Downloading The Videos. Detail: " + ex.ToString());
				}
				SetIsSyncing(false);
				SetIsDownloadingVideos(false);
                SetDownloadProgressString("");

				Device.BeginInvokeOnMainThread(() =>
				{
					MessagingCenter.Send<object>(this, AppGlobals.Events.NOTIFY_RETRY_DOWNLOADING_COMPLETE);
				});
			}
		}

		#endregion

		public string GetDownloadProgressString() {
			lock(this)
			{
				return _videosDownloadingProgressString;
			}
		}

		private void SetDownloadProgressString(string progressString) { 
			lock(this) {
				_videosDownloadingProgressString = progressString;
			}
		}

		private bool IsSyncDelayComplete() {
			var now = DateTime.Now;
			var lastSync = _lastSyncTime.AddMinutes(_syncDelay);
			int status = now.CompareTo(lastSync);

			if (status >= 0) {
				return true;
			}

			return false;
		}

		private void SetLastSyncTime(DateTime syncTime) {
			// TODO: When simple value dependency service is set up save this to local storage.
			_lastSyncTime = syncTime;
		}

		private void SetIsSyncing(bool isSyncing) { 
			lock(this) {
				_isSyncing = isSyncing;
			}
		}

		private void SetIsDownloadingVideos(bool isDownloadingVideos) { 
			lock (this) {
				_isDownloadingVideos = isDownloadingVideos;
			}
		}

        private string GetVideoListInformationForDebug(List<Video> videos)
        {
            string debug = String.Join("\r\n\r\n\t", videos.Select(v => String.Join("\r\n\t",
                new List<string>()
                {
	                "Name: " + v.Title,

	                "ID: " + v.ID,
	                "Downloaded Successfully: " + (v.DownloadedSuccessfully ? " True" : "False"),
	                "Deleted: " + (v.Deleted ? " True" : "False"),
	                "Last Played: " + v.LastPlayed.ToString("hh:mm:ss tt")
                }.ToArray())).ToArray());

            return debug;
        }

		/// <summary>
		/// Syncs the server activities with local activities. Since the mobile device's Activity table and the server's
		/// Activity table could be out of sync we will need to manually check both of them and update the local
		/// table based upon what the server sends back during a sync event.
		/// 
		/// NOTE: At the time of this writing (5/8/2017) the mobile device's may have legacy table records within
		/// the ActivitySession table. These records might not have an ActivityId Guid record
		/// </summary>
		/// <param name="allUserServerActivities">Server activities.</param>
		private void SyncServerActivitiesWithLocalActivities(List<ServerActivitySession> allUserServerActivities) {

			List<ActivitySession> serverActivities = null;

			try {
				// We need to cast the ServerActivitySessions into something we can put into the SQLite database.{
				serverActivities = allUserServerActivities.Select(a => (ActivitySession)a).ToList();
			}
			catch (Exception e) {
				App.Log("SyncService: Could not cast server activities to ActivitySession objects");
				return; // We can't proceed without the list of Activities.
			}

			List<ActivitySession> localActivities = _activityRepo.GetActivities();

			for (int i = 0; i < serverActivities.Count; i++) {
				var serverActivity = serverActivities[i];
				ActivitySession matchedActivity = FindMatchingLocalActivity(serverActivity, localActivities);

				if (matchedActivity == null)
				{
					// This server activity is not in the local database. Add it
					_activityRepo.AddActivity(serverActivity);
				}
			}

			// Refresh the profile screen if we are currently looking at it.
            MessagingCenter.Send<string>("", AppGlobals.Events.REFRESH_PROFILE);
		}

		/// <summary>
		/// Finds the matching local activity based on a server activity.
		/// 
		/// This matches against the ActivityId field that comes from the server id first. If the local activity's
		/// "ActivityId" field is null then this will then try to check agains all other fields for a match.
		/// If this method finds a local activity match and the local activity's "ActivityId" field is null then
		/// it will update the local activity with the server's activity id.
		/// </summary>
		/// <returns>The matching local activity or null if no corresponding activity could be found.</returns>
		/// <param name="serverActivity">Server activity.</param>
		private ActivitySession FindMatchingLocalActivity(
			ActivitySession serverActivity, List<ActivitySession> localActivities) {

			ActivitySession matchedActivity = localActivities.Where(a => a.ActivityId == serverActivity.ActivityId)
															 .FirstOrDefault();

			if (matchedActivity == null) {
				// There was no activity that matched with the ActivityId. The local match's ActivityId could be null
				// try matching based on combined columns
				matchedActivity = localActivities.Where(a => (a.VideoId == serverActivity.VideoId)
														&& (a.StartTime == serverActivity.StartTime)
														&& (a.EndTime == serverActivity.EndTime)
														&& (a.NotificationTime == serverActivity.NotificationTime)
														&& (a.Bonus == serverActivity.Bonus)
													   ).FirstOrDefault();

				if (matchedActivity != null) {
					// There is a matching local activity without the server's activity id. Update the local record
					// with the server's activity id for future comparison.
					matchedActivity.ActivityId = serverActivity.ActivityId;
					_activityRepo.UpdateActivity(matchedActivity);
				}
			}

			return matchedActivity;
		}

		/// <summary>
		/// Converts the old activities to new activities. What this means is that before the update that validated
		/// and synced ActivitySession objects between the mobile app and the server that the ActivitySessions on
		/// mobile did not have a "Guid ActivityId".
		/// 
		/// This means that if we tried to upload those activities to the server that the server would have insertion
		/// issues because all of those old activities would have the exact same ID.
		/// 
		/// Thus, we need to check each activity on the mobile device to see if it has a set Guid and if not then we
		/// need to give it one and update it in the local database before sending it to the server.
		/// </summary>
		/// <returns>The old activities to new activities.</returns>
		/// <param name="activities">Activities.</param>
		/// <param name="repo">Repo.</param>
		private List<ActivitySession> ConvertOldActivitiesToNewActivities(List<ActivitySession> activities, 
		                                                                  ActivitySessionRepository repo) {

			foreach (var activity in activities) {
				if (activity.ActivityId == Guid.Empty) {
					// This activity is an old activity and needs an ActivityId

					activity.ActivityId = Guid.NewGuid();
					repo.UpdateActivity(activity);
				}
			}

			return activities;
		}

    }
}
