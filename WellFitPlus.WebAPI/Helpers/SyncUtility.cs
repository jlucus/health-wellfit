using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using WellFitPlus.Database.Entities;
using WellFitPlus.Database.Repositories;
using WellFitPlus.WebAPI.BindingModels;
using WellFitPlus.WebAPI.Controllers;
using WellFitPlus.WebAPI.Models;

namespace WellFitPlus.WebAPI.Helpers
{
    public class SyncUtility
    {
        #region Properties

        public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SyncRequestViewModel _Request;
        private SettingView _Settings;
        private string _UserName;
        private DateTime _MostRecentActivityDate;

        private static Random _rng = new Random();

        #endregion

        #region Sync Collections

        private List<Activity> _AllActivities;
        private List<Activity> _CompletedRolloverActivities;
        private List<Video> _AllVideos;
        private List<UserVideoViewModel> _TopUserVideos;

        #endregion

        #region Constants

        private const int FRESH_VIDEO_SPACE_THRESHOLD = 20;
        private const int MAX_VIDEO_COUNT = 15;
        private const int USER_TOP_N_VIDEO_COUNT = 3;

        #endregion


        #region Initialization

        public SyncUtility(SyncRequestViewModel request)
        {
            this._Request = request;
            WellFitPlus.Database.Contexts.WellFitAuthContext context = new Database.Contexts.WellFitAuthContext();
            var user = context.Users.FirstOrDefault(usr => usr.Id == this._Request.UserID.ToString());
            this._UserName = user != null ? user.FirstName + " " + user.LastName : "Unknown";
        }        
        #endregion

        #region Sync - Entry Point

        public SyncResponseViewModel GetSyncResponse()
        {
            try
            {
                #region Logging

                log.Info(">>>>>>>>>> Sync Starting For " + this._UserName + " <<<<<<<<<<");

                #endregion

                SyncResponseViewModel response = new SyncResponseViewModel();
                ActivityRepository activityRepo = new ActivityRepository();

                // Set the Id's on any empty activity ID's. There should only be empty activity ID's if a user is still
                // using an old version of the mobile app. This foreach loop will do nothing if the newest version of the app
                // is used to send the request to the server.
                // TODO: App Store V1 Delete this foreach loop.
                foreach (var activityBindingModel in _Request.ListOfNewActivities) {
                    if (activityBindingModel.Id == Guid.Empty) {
                        activityBindingModel.Id = Guid.NewGuid();
                    }
                }

                // Update User Settings
                this.UpdateSettings();      

                // Get Videos
                this._AllVideos = new VideoRepository().GetVideos();

                // Remove File Extension From Video Titles (For Logging)
                 if (this._AllVideos.Count > 0) { this._AllVideos.ForEach(video => video.Title = video.Title.Split('.').First()
                    .Replace("WF_0", "").Replace("WF_", "")); }

                // Add New Activities
                this.AddOrUpdateActivitiesActivities();
                
                // Get Users Top Videos
                this._TopUserVideos = this.GetUserTopVideosBasedOnActivities();

                // Get Videos To Delete
                response.VideosToDelete = this.GetVideosToDelete();

                // Check Videos Should Rollover
                bool rollover = this.CheckVideosShouldRollover();
                
                // Get Only Fresh Videos To Download
                response.VideosToDownload = this.GetFreshBingoVideosToDownload();
                
                // Set Response Top Played Videos
                response.TopMostFrequentVideos = this._TopUserVideos;

                // Get the full list of Activities to return to the user to keep mobile and server in sync.
                response.AllUserActivities = activityRepo.GetActivities(_Request.UserID);

                #region Logging

                log.Info(">>>>>>>>>> Sync Ending For " + this._UserName + " <<<<<<<<<<");

                #endregion

                return response;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        #endregion

        #region Settings

        private void UpdateSettings()
        {
            // Get User Settings And Update Cache Size
            SettingController settingController = new SettingController();
            this._Settings = settingController.GetSetting(new SettingView() { UserID = this._Request.UserID });
            this._Settings.CacheSize = (long)this._Request.CacheSize;
            this._Settings.RolloverDate = 
                this._Settings.RolloverDate < new DateTime(2000,1,1) ? DateTime.Today.AddMonths(-1) : this._Settings.RolloverDate;

            settingController.AddOrUpdate(this._Settings);
        }
        
        private bool CheckVideosShouldRollover()
        {
            // Get All Active Server Videos
            // TODO: SYNCFIX: filter out deleted videos.
            var activeVideos = this._AllVideos.Where(v => v.Active == true).ToList();

            // Check Whether Current Iteration Should Rollover All Active Videos Must Have:
            // --> Been Watched Previously
            // --> OR Currently Reside On The Phone
            var rollover = activeVideos.All(video => _CompletedRolloverActivities.Any(activity => activity.VideoID == video.Id) 
                                            || this._Request.ListOfCurrentVideos.Any(vid => vid.ID == video.Id));

            #region Logging

            var activeVideoTitles = activeVideos.Select(v => v.Title)
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            var completedRolloverVideoTitles = activeVideos.Where(v => _CompletedRolloverActivities.Any(a => a.VideoID == v.Id))
                .Select(v => v.Title)
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            var currentVideoTitles = this._Request.ListOfCurrentVideos
                .Where(v => _CompletedRolloverActivities.Any(a => v.ID == a.VideoID) == false)
                .Select(v => v.Title)
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            var missingVideoTitles = activeVideos.Where(video => 
                            _CompletedRolloverActivities.Any(activity => activity.VideoID == video.Id) == false 
                            && this._Request.ListOfCurrentVideos.Any(vid => vid.ID == video.Id) == false)
                .Select(v => v.Title)
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            // Logging
            var uniqueVideoIdsWatched = _CompletedRolloverActivities.Select(activity => activity.VideoID).Distinct().ToList();
            log.Info("~~~~~~~~~~ Checking Video Rollover Status For " + this._UserName + ". Last Rollover Date: " + this._Settings.RolloverDate.ToString("MM-dd-yyyy hh:mm:ss tt") + " ~~~~~~~~~~");
            log.Info("---> Active Server Videos (" + activeVideoTitles .Count() + "): " + String.Join(", ", activeVideoTitles));
            log.Info("---> Videos Completed This Iteration (" + completedRolloverVideoTitles.Count() + "): " + String.Join(", ", completedRolloverVideoTitles));
            log.Info("---> Unwatched Fresh Videos Currently On Users Phone (" + currentVideoTitles.Count() + "): " + String.Join(", ", currentVideoTitles));

            #endregion

            // If Time To Rollover
            if (rollover == true)
            {
                log.Info("###### Rolling Over User Video Date #####");

                // Update Rollover Date
                SettingController settingController = new SettingController();

                // Get Max Activity End Time
                var rolloverDate = this._CompletedRolloverActivities.Count == 0 || this._CompletedRolloverActivities.Max(activity => activity.EndTime) == null
                    ? DateTime.Now : (DateTime)this._CompletedRolloverActivities.Max(activity => activity.EndTime);

                // Set Rollover Date After Last Completed Activity In This Iteration
                this._Settings.RolloverDate = rolloverDate;
                settingController.AddOrUpdate(this._Settings);

                // Update Completed Activities (After Rollver Date)
                this.AddOrUpdateActivitiesActivities();
            }
            else
            {
                log.Info("---> Current Iteration Will Rollover When The Following " + missingVideoTitles.Count() + " Videos Are Downloaded: " + String.Join(", ", missingVideoTitles) + " ~~~~~~~~~~");
                log.Info("~~~~~~~~~~ Rollover Criteria Not Currently Met ~~~~~~~~~~");
            }
            

            return rollover;
        }

        #endregion
        
        #region Activities

        private void AddOrUpdateActivitiesActivities()
        {
            // Filter New Activities To Only Those With Video IDs In Current Video List
            // We will keep this in here just in case historical data needs this. It will not affect
            // normal functionality of the sync.
            this._Request.ListOfNewActivities = this._Request.ListOfNewActivities
                .Where(act => this._AllVideos.Any(v => v.Id == act.VideoID)).ToList();

            #region Logging

            var currentVideoTitles = this._Request.ListOfCurrentVideos.Select(v => v.Title + (v.LastPlayed > v.DownloadDate && v.LastPlayed > this._Settings.RolloverDate ? " P" : " NP")).ToList();
            log.Info(this._UserName + " Currently Has " + currentVideoTitles.Count + " Videos: " + String.Join(", ", currentVideoTitles));

            #endregion

            // Get User Activities
            var activityRepo = new ActivityRepository();

            // Convert all the ActivityBindingModels to Activities using an explicitk operator.
            List<Activity> mobileUploadedList = new List<Activity>();
            foreach (var bindingModel in _Request.ListOfNewActivities) {
                mobileUploadedList.Add((Activity)bindingModel);
            }

            activityRepo.AddOrUpdateActivities(_Request.UserID, mobileUploadedList);


            // Update All Activities List
            this._AllActivities = new ActivityRepository().GetActivities(this._Request.UserID).ToList();
            
            // Get All Completed Rollover Activities
            this._CompletedRolloverActivities = this._AllActivities.Where(activity => 
                activity.EndTime > activity.StartTime && activity.EndTime > this._Settings.RolloverDate).ToList();

            #region Logging

            log.Info(this._UserName + " Has Completed " + this._CompletedRolloverActivities.Count + " Activities This Iteration.");

            #endregion
        }

        #endregion

        #region Download
         
        private List<UserVideoViewModel> GetFreshBingoVideosToDownload()
        {
            List<UserVideoViewModel> videosToDownload = new List<UserVideoViewModel>();

            try
            {
                // Get Videos User Currently Has That Are Not Flagged For Deletion
                var remainingUserVideos = this._Request.ListOfCurrentVideos.Where(v => v.FlaggedForDeletion == false).ToList();

                // If Percent Fresh Video Space Below The Threshold - Return Empty List
                 var percentSpaceFree = this.VideoSpaceAvailableInPercentage(remainingUserVideos);

                // Validation
                if (percentSpaceFree <= FRESH_VIDEO_SPACE_THRESHOLD) { return new List<UserVideoViewModel>(); }

                // Get All Active Server Videos That The User Has Not Watched During The Current Iteration
                var userVideoIds = this._AllVideos.Where(v => this._CompletedRolloverActivities.Any(act => act.VideoID == v.Id) == false)
                    .Select(v => v.Id)
                    .Distinct()
                    .ToList();

                var serverVideosUserHasNotWatchedThisIteration = this._AllVideos
                    .Where(v => v.Active == true && userVideoIds.Contains(v.Id))
                    .Where(v => remainingUserVideos.Any(video => video.ID == v.Id) == false).ToList();

                #region Logging

                var serverVideosUserHasNotWatchedThisIterationTitles = serverVideosUserHasNotWatchedThisIteration.Select(v => v.Title).Distinct().OrderBy(v => v).ToList();
                log.Info("Server Videos " + this._UserName + " Has Not Watched In The Current Iteration (" + serverVideosUserHasNotWatchedThisIterationTitles.Count + "): " + String.Join(", ", serverVideosUserHasNotWatchedThisIterationTitles));

                #endregion

                // Get Videos User Has Never Seen Before That Are Also Not In Their Remaining Video List
                Shuffle<Video>(serverVideosUserHasNotWatchedThisIteration);

                // Get Available Space
                long availableSpace = this.AvailableVideoSpaceInBytes(remainingUserVideos);

                // Loop Videos
                foreach (Video vid in serverVideosUserHasNotWatchedThisIteration)
                {
                    // If Max Video Count Reached - Break                    
                    if (remainingUserVideos.Count + videosToDownload.Count >= MAX_VIDEO_COUNT) { continue; }
                    // If Video Already Exists In Users Remaining Videos Or In List Of Videos To Download - Continue
                    else if (videosToDownload.Any(v => v.ID == vid.Id) == true
                       || remainingUserVideos.Any(v => v.ID == vid.Id) == true) { continue; }

                    // Create Video Return Model
                    UserVideoViewModel vidView = new UserVideoViewModel() { UserID = this._Request.UserID };
                    UpdateUserVideoModel(vid, vidView);

                    // Get Video Size
                    var size = this.VideoSizeInBytes(vidView.Path);

                    // If Less Than Max Cache
                    if (availableSpace - size > 0)
                    {
                        videosToDownload.Add(vidView);
                        availableSpace -= size;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            if (videosToDownload.Count > 0)
            {
                log.Info(videosToDownload.Count + " New Fresh Bingo Videos Are Being Sent To " + this._UserName + " To Download: " + String.Join(", ", videosToDownload.OrderBy(v => v.Title).Select(v => v.Title.Replace(".mp4", ""))));
            }

            return videosToDownload;
        }

        #endregion

        #region Delete

        private List<UserVideoViewModel> GetVideosToDelete()
        {
            List<UserVideoViewModel> videosToDelete = new List<UserVideoViewModel>();

            try
            {
                // Get Videos The User Has That Are Inactive On The Server
                var inactiveUserVideos = this.GetUserVideosThatAreInactiveOnServer();

                // Get Videos The User Has That Have Been Deleted From The Server
                var userVideosDeletedFromServer = this.GetUserVideosThatHaveBeenDeletedFromServer();

                #region Logging

                if (inactiveUserVideos.Count > 0)
                {
                    log.Info(this._UserName + " Has (" + inactiveUserVideos.Count + ") Videos That Are Flagged For Deletion Because They Are Inactive On The Server: " + String.Join(", ", inactiveUserVideos.OrderBy(v => v.Title).Select(v => v.Title.Replace(".mp4", ""))));
                }

                if (userVideosDeletedFromServer.Count > 0)
                {
                    log.Info(this._UserName + " Has (" + userVideosDeletedFromServer.Count + ") Videos That Are Flagged For Deletion Because They Have Been Deleted From The Server: " + String.Join(", ", userVideosDeletedFromServer.OrderBy(v => v.Title).Select(v => v.Title.Replace(".mp4", ""))));
                }

                #endregion

                // Add Inactive And Deleted Videos To Return List
                videosToDelete.AddRange(inactiveUserVideos);
                videosToDelete.AddRange(userVideosDeletedFromServer);

                // Get Non-Favorite Videos
                var nonFavoriteVideos = this._Request.ListOfCurrentVideos.Where(v => v.IsFavorite == false).ToList();

                // Get Count Of Fresh Videos The User Has
                var freshVideoCount = nonFavoriteVideos
                    .Where(v => v.LastPlayed == DateTime.MinValue || v.LastPlayed < v.DownloadDate).Count();

                // Get Percent Of Fresh Videos The User Has
                var percentCountFresh = ((double)freshVideoCount / (double)MAX_VIDEO_COUNT) * 100;

                // Check Percent Free Space
                var percentSpaceFree = this.VideoSpaceAvailableInPercentage(this._Request.ListOfCurrentVideos.Where(v => v.LastPlayed > v.DownloadDate).ToList());
                if (percentSpaceFree <= FRESH_VIDEO_SPACE_THRESHOLD || percentCountFresh <= FRESH_VIDEO_SPACE_THRESHOLD)
                {
                    // Get Videos To Delete
                    var playedUserVideosToDelete = this.GetUserVideosWatchedBasedOnActivities(nonFavoriteVideos);

                    // Remove Any Videos In User Top Three From Played User Videos To Delete
                    var playedUserVideosToDeleteNotInTopThree = playedUserVideosToDelete.Where(v => this._TopUserVideos.Any(vid => vid.ID == v.ID) == false).ToList();

                    #region Logging

                    if (playedUserVideosToDeleteNotInTopThree.Count > 0)
                    {
                        log.Info(this._UserName + " Has (" + playedUserVideosToDeleteNotInTopThree.Count + ") Watched Videos (Not In Top 3 Or Favorites) Flagged For Deletion: " + String.Join(", ", playedUserVideosToDeleteNotInTopThree.OrderBy(v => v.Title).Select(v => v.Title.Replace(".mp4", ""))));
                    }

                    #endregion

                    // Add Videos That Have Been Played To Return List
                    videosToDelete.AddRange(playedUserVideosToDeleteNotInTopThree);
                }

                // Loop Videos To Delete
                foreach (var videoToDelete in videosToDelete)
                {
                    // Set Deletion Flag
                    this._Request.ListOfCurrentVideos.First(video => video.ID == videoToDelete.ID).FlaggedForDeletion = true;
                }

                #region Logging

                // Logging
                if (videosToDelete.Count > 0)
                {
                    log.Info(this._UserName + " Has (" + videosToDelete.Count + ") Total Videos That Are Flagged For Deletion");
                }

                #endregion
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return videosToDelete;
        }

        private List<UserVideoViewModel> GetUserVideosThatHaveBeenDeletedFromServer()
        {
            List<UserVideoViewModel> videosToDelete = new List<UserVideoViewModel>();

            try
            {
                // Get All Video IDs
                var allServerVideos = this._AllVideos.Select(v => v.Id).ToList();

                // Get All Videos That The User Have That No Longer Exist On The Server 
                // (Videos That Have Been Deleted From The Server)
                var videosUserHasThatHaveBeenDeletedFromTheServer = this._Request.ListOfCurrentVideos
                    .Where(video => allServerVideos.Any(id => id == video.ID) == false).ToList();

                // Loop Videos User Has That No Longer Exist On The Server
                foreach (var deletedVideo in videosUserHasThatHaveBeenDeletedFromTheServer)
                {
                    // Flag The Video For Deletion
                    var videoToDelete = this._Request.ListOfCurrentVideos.First(video => video.ID == deletedVideo.ID);
                    videosToDelete.Add(videoToDelete);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return videosToDelete;
        }

        private List<UserVideoViewModel> GetUserVideosThatAreInactiveOnServer()
        {
            List<UserVideoViewModel> videosToDelete = new List<UserVideoViewModel>();

            try
            {
                // Get Inactive Server Videos
                var inactiveServerVideos = this._AllVideos.Where(video => video.Active == false).ToList();

                // Loop Inactive Server Videos
                foreach (var inactiveVideo in inactiveServerVideos)
                {
                    // Check If User Has This Inactive Video
                    bool userHasInactiveVideo = this._Request.ListOfCurrentVideos.Any(video => video.ID == inactiveVideo.Id);
                    if (userHasInactiveVideo == true)
                    {
                        // Flag The Video For Deletion
                        var videoToDelete = this._Request.ListOfCurrentVideos.First(video => video.ID == inactiveVideo.Id);
                        videosToDelete.Add(videoToDelete);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return videosToDelete;
        }

        #endregion

        #region Get Videos 
        
        private List<UserVideoViewModel> GetUserTopVideosBasedOnActivities()
        {
            try
            {
                // Get Played Video Count - Join On Activities
                var videosPlayed = this._Request.ListOfCurrentVideos
                    .Where(v => v.LastPlayed > v.DownloadDate && v.IsFavorite == false)
                    .Join(this._CompletedRolloverActivities, v => v.ID, a => a.VideoID, (v, a) => new
                    {
                        UserID = this._Request.UserID,
                        Active = v.Active,
                        DateModified = v.DateModified,
                        DateUploaded = v.DateUploaded,
                        Description = v.Description,
                        ID = v.ID,
                        Path = v.Path,
                        Tags = v.Tags,
                        Title = v.Title,
                        Type = v.Type,
                        a.EndTime,
                        a.StartTime,
                        a.NotificationTime,
                        v.DownloadDate
                    }).Distinct()
                    .Where(v => v.EndTime > v.DownloadDate)
                    .GroupBy(v => new
                    {
                        v.UserID,
                        v.Active,
                        v.DateModified,
                        v.DateUploaded,
                        v.ID,
                        v.Path,
                        v.Tags,
                        v.Title,
                        v.Type,
                        v.Description,
                        v.DownloadDate
                    })
                    .Select(v => new UserVideoViewModel()
                    {
                        UserID = this._Request.UserID,
                        Active = v.Key.Active,
                        DateModified = v.Key.DateModified,
                        DateUploaded = v.Key.DateUploaded,
                        Description = v.Key.Description,
                        ID = v.Key.ID,
                        Path = v.Key.Path,
                        Tags = v.Key.Tags,
                        Title = v.Key.Title,
                        Type = v.Key.Type,
                        ViewCount = v.Count(),
                        DownloadDate = v.Key.DownloadDate
                    })
                    .OrderByDescending(v => v.ViewCount)
                    .ToList();

                videosPlayed = videosPlayed.Count > USER_TOP_N_VIDEO_COUNT
                    ? videosPlayed.Take(USER_TOP_N_VIDEO_COUNT).OrderBy(v => v.Title).ToList() : videosPlayed.OrderBy(v => v.Title).ToList();

                if (videosPlayed.Count > 0)
                {
                    log.Info(this._UserName + " - Top Played Videos This Iteration (" + videosPlayed.Count + "): " + String.Join(", ", videosPlayed.Select(v => v.Title + " (" + v.ViewCount + " Views)")));
                }

                return videosPlayed;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private List<UserVideoViewModel> GetUserVideosWatchedBasedOnActivities(List<UserVideoViewModel> videos)
        {
            try
            {
                // Get Played Video Count - Join On Activities
                var videosPlayed = this._Request.ListOfCurrentVideos
                    .Where(v => v.LastPlayed > v.DownloadDate)
                    .Join(this._CompletedRolloverActivities, v => v.ID, a => a.VideoID, (v, a) => new
                    {
                        UserID = this._Request.UserID,
                        Active = v.Active,
                        DateModified = v.DateModified,
                        DateUploaded = v.DateUploaded,
                        Description = v.Description,
                        ID = v.ID,
                        Path = v.Path,
                        Tags = v.Tags,
                        Title = v.Title,
                        Type = v.Type,
                        a.EndTime,
                        a.StartTime,
                        a.NotificationTime,
                        v.DownloadDate
                    })
                    .Distinct()
                    .Where(v => v.EndTime > v.DownloadDate)
                    .GroupBy(v => new { v.UserID, v.Active, v.DateModified, v.DateUploaded, v.ID, v.Path, v.Tags, v.Title, v.Type, v.Description, v.DownloadDate })
                    .Select(v => new UserVideoViewModel()
                    {
                        UserID = this._Request.UserID,
                        Active = v.Key.Active,
                        DateModified = v.Key.DateModified,
                        DateUploaded = v.Key.DateUploaded,
                        Description = v.Key.Description,
                        ID = v.Key.ID,
                        Path = v.Key.Path,
                        Tags = v.Key.Tags,
                        Title = v.Key.Title,
                        Type = v.Key.Type,
                        DownloadDate = v.Key.DownloadDate
                    })
                    .OrderBy(v => v.Title).ToList();

                if (videosPlayed.Count > 0)
                {
                    log.Info(this._UserName + " Has Seen " + videosPlayed.Count + " Videos This Iteration: " + String.Join(", ", videosPlayed.Select(v => v.Title)));
                }

                return videosPlayed;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void UpdateUserVideoModel(Video videoModel, UserVideoViewModel videoView)
        {
            videoView.ID = videoModel.Id;
            videoView.Active = videoModel.Active;
            videoView.Description = videoModel.Description;
            videoView.Path = videoModel.Path;
            videoView.Type = videoModel.Type.ToString();
            videoView.Tags = videoModel.Tags;
            videoView.Title = videoModel.Title;
            videoView.DateModified = videoModel.DateModified;
            videoView.DateUploaded = videoModel.DateUploaded;
            videoView.FlaggedForDeletion = false;
        }

        #endregion
        
        #region Compute Video Size / Space

        private double VideoSpaceAvailableInPercentage(List<UserVideoViewModel> currentVideos)
        {
            // Validation
            if (currentVideos.Count == 0) { return 100; }

            // Get Settings
            var spaceAllowedInBytes = this.GetCacheSizeLimitInBytes();

            // Get Not Played Video Space
            var availableSpaceInBytes = this.AvailableVideoSpaceInBytes(currentVideos);
            var percentage = (double)availableSpaceInBytes / (double)spaceAllowedInBytes;
            
            log.Info(this._UserName + " Has " 
                + Math.Round(((double)this.TotalVideoSpaceConsumedInBytes(currentVideos) / 1024 / 1024), 2) 
                    + " MB Consumed By " + currentVideos.Count + " Videos. " +
                this._UserName + " Has " + Math.Round((percentage * 100), 2) 
                    + " % Space Available Out Of " + this._Settings.CacheSize + " MB (" + currentVideos.Count + " Videos)");
            
            return percentage * 100;
        }

        private long TotalVideoSpaceConsumedInBytes(List<UserVideoViewModel> currentVideos)
        {
            // Get Video IDs
            List<Guid> ids = currentVideos.Select(v => v.ID).ToList();

            // Get Videos
            var paths = this._AllVideos.Where(v => ids.Contains(v.Id) == true).ToList().Select(v => v.Path);
            long totalSpaceInBytes = 0;

            // Loop Paths
            foreach (var path in paths)
            {
                // Add Each Vizeo Size To Total
                totalSpaceInBytes += this.VideoSizeInBytes(path);
            }
            
            return totalSpaceInBytes;
        }

        private long AvailableVideoSpaceInBytes(List<UserVideoViewModel> userCurrentVideos)
        {
            // Get Settings
            var spaceAllowedInBytes = this.GetCacheSizeLimitInBytes();

            // Validation
            if (userCurrentVideos.Count == 0) { return spaceAllowedInBytes; }

            // Get Size Of All Remaining User Videos
            var remainingVideos = userCurrentVideos.Where(video => video.FlaggedForDeletion == false).ToList();
            var spaceConsumedInBytes = this.TotalVideoSpaceConsumedInBytes(remainingVideos);

            // Get Total Space Allowed Minus Space Consumed
            var spaceRemainingInBytes = spaceAllowedInBytes - spaceConsumedInBytes;

            return spaceRemainingInBytes;
        }

        private long VideoSizeInBytes(string path)
        {
            try
            {
                // Get Short File Name
                var shortFileName = path.Split('/').Last().Split('\\').Last();

                // Get Full Server Video File Name
                var filename = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Video"), shortFileName);
                filename = filename.Replace("\\WellFitPlus.WebAPI\\", "\\WellFitPlus.WebPortal\\");

                // If File Doesnt Exist - Continue
                if (System.IO.File.Exists(filename) == false) { return 0; }

                // Get File Size
                var spaceConsumedInBytes = new System.IO.FileInfo(filename).Length;

                return spaceConsumedInBytes;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return 0;
            }
        }

        private long GetCacheSizeLimitInBytes()
        {
            var spaceAllowed = this._Settings.CacheSize * 1024 * 1024;
            return spaceAllowed;
        }

        #endregion

        #region Shuffle

        private static void Shuffle<T>(List<T> list)
        {
            System.Security.Cryptography.RNGCryptoServiceProvider provider = new System.Security.Cryptography.RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        #endregion
    }
}