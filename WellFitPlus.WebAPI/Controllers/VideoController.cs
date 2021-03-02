using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Text;

using System.Web.Http;

using Newtonsoft.Json;

using log4net;
using WellFitPlus.WebAPI.Models;
using WellFitPlus.Common;
using WellFitPlus.Database.Entities;
using WellFitPlus.Database.Repositories;
using System.Net.Http.Headers;
using Microsoft.AspNet.Identity;
using WellFitPlus.WebAPI.Controllers;
using WellFitPlus.WebAPI.BindingModels;
using WellFitPlus.WebAPI.Helpers;

namespace WellFitPlus.WebAPI.Controllers
{
    [RoutePrefix("api/Video")]
    public partial class VideoController : ApiController
    {
        public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private VideoRepository _videoRepo = new VideoRepository();

        #region API Methods

        [HttpPost]
        [Route("Add")]
        public bool Add(VideoViewModel videoView)
        {
            try
            {

                Video video = new Video();

                UpdateModel(ref video, videoView);

                video.Active = true;
                video.DateUploaded = DateTime.Now;
                video.DateModified = DateTime.Now;
                video.Type = VideoType.None;

                int result = _videoRepo.Add(video);
                return result > -1;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

        [HttpPost]
        [Route("Edit")]
        public bool Edit(VideoViewModel videoView)
        {
            try
            {
                Video video = _videoRepo.GetVideo(videoView.ID);
                if (video != null)
                {

                    UpdateModel(ref video, videoView);
                    video.DateModified = DateTime.Now;
                    video.Type = VideoType.None;

                    int result = _videoRepo.Edit(video);
                    return result > -1;
                }
                return false;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

        [HttpPost]
        [Route("Delete")]
        public bool Delete(VideoViewModel videoView)
        {
            try
            {
                int result = _videoRepo.Delete(videoView.ID);
                return result > -1;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetList")]
        public List<VideoViewModel> GetList()
        {
            List<VideoViewModel> videoViews = new List<VideoViewModel>();
            List<Video> videos = new List<Video>();

            try
            {
                videos = _videoRepo.GetVideos();

                foreach (Video vid in videos)
                {
                    VideoViewModel vidView = new VideoViewModel();
                    vidView.DateModified = vid.DateModified;
                    vidView.DateUploaded = vid.DateUploaded;
                    vidView.Active = vid.Active;
                    vidView.Path = vid.Path;
                    vidView.Tags = vid.Tags;
                    vidView.Title = vid.Title;
                    vidView.Type = vid.Type.ToString();
                    vidView.ID = vid.Id;

                    videoViews.Add(vidView);
                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return videoViews;
        }

        [HttpGet]
        [Route("GetVideo")]
        public VideoViewModel GetVideo([FromUri] VideoViewModel videoView)
        {
            VideoViewModel vidView = new VideoViewModel();
            Video video = new Video();

            try
            {
                video = _videoRepo.GetVideo(videoView.ID);

                vidView.Active = video.Active;
                vidView.DateModified = video.DateModified;
                vidView.DateUploaded = video.DateUploaded;
                vidView.Description = video.Description;
                vidView.Path = video.Path;
                vidView.Tags = video.Tags;
                vidView.Title = video.Title;
                vidView.Type = video.Type.ToString();
                vidView.ID = video.Id;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return vidView;
        }

        [HttpPost]
        [Route("DownloadVideo")]
        public HttpResponseMessage DownloadVideo(VideoViewModel videoView)
        {
            try
            {
                // Get Files
                MemoryStream responseStream = new MemoryStream();
                Stream fileStream = File.Open(videoView.Path, FileMode.Open);
                bool fullContent = true;
                if (this.Request.Headers.Range != null)
                {
                    fullContent = false;

                    // Currently we only support a single range.
                    RangeItemHeaderValue range = this.Request.Headers.Range.Ranges.First();


                    // From specified, so seek to the requested position.
                    if (range.From != null)
                    {
                        fileStream.Seek(range.From.Value, SeekOrigin.Begin);

                        // In this case, actually the complete file will be returned.
                        if (range.From == 0 && (range.To == null || range.To >= fileStream.Length))
                        {
                            fileStream.CopyTo(responseStream);
                            fullContent = true;
                        }
                    }
                    if (range.To != null)
                    {
                        // 10-20, return the range.
                        if (range.From != null)
                        {
                            long? rangeLength = range.To - range.From;
                            int length = (int)Math.Min(rangeLength.Value, fileStream.Length - range.From.Value);
                            byte[] buffer = new byte[length];
                            fileStream.Read(buffer, 0, length);
                            responseStream.Write(buffer, 0, length);
                        }
                        // -20, return the bytes from beginning to the specified value.
                        else
                        {
                            int length = (int)Math.Min(range.To.Value, fileStream.Length);
                            byte[] buffer = new byte[length];
                            fileStream.Read(buffer, 0, length);
                            responseStream.Write(buffer, 0, length);
                        }
                    }
                    // No Range.To
                    else
                    {
                        // 10-, return from the specified value to the end of file.
                        if (range.From != null)
                        {
                            if (range.From < fileStream.Length)
                            {
                                int length = (int)(fileStream.Length - range.From.Value);
                                byte[] buffer = new byte[length];
                                fileStream.Read(buffer, 0, length);
                                responseStream.Write(buffer, 0, length);
                            }
                        }
                    }
                }
                // No Range header. Return the complete file.
                else
                {
                    fileStream.CopyTo(responseStream);
                }
                fileStream.Close();
                responseStream.Position = 0;

                HttpResponseMessage response = new HttpResponseMessage();
                response.StatusCode = fullContent ? HttpStatusCode.OK : HttpStatusCode.PartialContent;
                response.Content = new StreamContent(responseStream);
                return response;
            }
            catch (IOException Ex)
            {
                //throw new HttpResponseException("A generic error occured. Please try again later.", HttpStatusCode.InternalServerError);
                return null;
            }
        }

        private void UpdateModel(ref Video videoModel, VideoViewModel videoView)
        {
            videoModel.Active = videoView.Active;
            videoModel.Description = videoView.Description;
            videoModel.Path = videoView.Path;
            videoModel.Type = (VideoType)Enum.Parse(typeof(VideoType), videoView.Type);
            videoModel.Tags = videoView.Tags;
            videoModel.Title = videoView.Title;
            videoModel.DateModified = videoView.DateModified;
            videoModel.DateUploaded = videoView.DateUploaded;
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

        #region Sync 
       
        [HttpPost]
        [Route("Sync")]
        public HttpResponseMessage Sync(SyncRequestViewModel request)
        {
            // NOTE: We are explicitly throwing exceptions that will be sent back to the mobile app. Allow them to bubble to the app.
            // This should really be in it's own controller

            var activityRepo = new ActivityRepository();

            List<Video> videosOnDevice;

            try
            {
                videosOnDevice = ConvertUserVideoViewModelToVideos(request.ListOfCurrentVideos);
            }
            catch (Exception e) {
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.Content = new StringContent(e.ToString());

                throw new HttpResponseException(message);
            }
            
            // Update the user settings first.
            // TODO: Determin if we even need to do this. We aren't using the cache size anywhere else. We can probably remove it
            //       from the Settings model.
            if (!UpdateCacheSize(request.CacheSize, request.UserID)) {
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.Content = new StringContent("Error with saving user settings");

                throw new HttpResponseException(message);
            }

            // Set the Id's on any empty activity ID's. There should only be empty activity ID's if a user is still
            // using an old version of the mobile app. This foreach loop will do nothing if the newest version of the app
            // is used to send the request to the server.
            // TODO: App Store V1 Delete this foreach loop.
            foreach (var activityBindingModel in request.ListOfNewActivities)
            {
                if (activityBindingModel.Id == Guid.Empty)
                {
                    activityBindingModel.Id = Guid.NewGuid();
                }
            }

            AddOrUpdateActivities(request.ListOfNewActivities, request.UserID);

            List<UserVideo> unwatchedVideos = 
                ProcessBingoAndGetUnwatchedVideos(request.ListOfNewActivities, videosOnDevice, request.UserID);

            float percentUnwatchedOnDevice = GetUnwatchedVideoPercentageOnDevice(videosOnDevice, unwatchedVideos);
            
            // Set up response lists
            SyncResponseViewModel responseObject = new SyncResponseViewModel();
            responseObject.VideosToDelete = new List<UserVideoViewModel>();
            responseObject.VideosToDownload = new List<UserVideoViewModel>();
            responseObject.TopMostFrequentVideos = new List<UserVideoViewModel>();
            responseObject.AllUserActivities = activityRepo.GetActivities(request.UserID); // For statistical purposes on the device.

            // Does the user have <=20% unwatched videos on the device?
            if (percentUnwatchedOnDevice <= 20) {
                // Get all the necessary data for the "bingo" process.
                List<Activity> allUserActivities = activityRepo.GetActivities(request.UserID);

                List<UserVideoViewModel> topMostFrequentVideos = GetMostFrequentlyWatchedVideoList(request.UserID, allUserActivities);
                List<UserVideoViewModel> videosToDelete =
                    GetVideosToDelete(request.UserID, topMostFrequentVideos, videosOnDevice, unwatchedVideos);
                List<UserVideoViewModel> videosToDownload = new List<UserVideoViewModel>();

                try
                {
                    videosToDownload =
                        GetVideosToDownload(request.UserID, request.CacheSize, 
                            topMostFrequentVideos, videosToDelete, videosOnDevice, unwatchedVideos);
                }
                catch (Exception e)
                {
                    // There was an error with getting videos to download. Most likely from getting the file size (check there first)
                    var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    message.Content = new StringContent("Error getting videos to download: " + e.ToString());

                    throw new HttpResponseException(message);
                }

                responseObject.TopMostFrequentVideos = topMostFrequentVideos;
                responseObject.VideosToDelete = videosToDelete;
                responseObject.VideosToDownload = videosToDownload;
            }

            // Format the result manually to ignore reference loops
            JsonSerializerSettings jsSettings = new JsonSerializerSettings();
            jsSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            var json = JsonConvert.SerializeObject(responseObject, Formatting.None, jsSettings);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return response;
        }

        #endregion

        #region Sync Methods

        /// <summary>
        /// Updates the user's cache size.
        /// 
        /// </summary>
        /// <param name="cacheSize"></param>
        /// <param name="userId"></param>
        /// <returns>true upon success or false if an error occured</returns>
        private bool UpdateCacheSize(float cacheSize, Guid userId)
        {
            // Get User Settings And Update Cache Size
            SettingRepository settingsRepo = new SettingRepository();
            Setting settings = settingsRepo.GetSettings(userId);

            // There was a bug where newly created users would not have settings created. 
            // Check for that here for existing users and create default settings for them
            if (settings == null) {
                settings = SettingRepository.GetNewDefaultSettings();
                settings.UserID = userId;
                settings.CacheSize = (long) cacheSize;
                settingsRepo.Add(settings);
            }

            // In case of error we need to exit this method. The caller should
            if (settings.UserID != userId) {
                return false; //
            }

            // The mobile app is currently sending the cache size as a float yet the server database is saving it as a long.
            settings.CacheSize = Convert.ToInt64(cacheSize);

            settingsRepo.Edit(settings);

            return true;
        }

        /// <summary>
        /// Takes a list of new (or edited) activities uploaded by a mobile device and adds/updates the records in the database.
        /// </summary>
        /// <param name="activitiesToProcess">
        /// List of ActivityBindingModel objects to either add to the database or update existing records
        /// </param>
        /// <param name="userId">The UserID of the person that these activity records belong to.</param>
        private void AddOrUpdateActivities(List<ActivityBindingModel> activitiesToProcess, Guid userId)
        {
            // Get User Activities
            var activityRepo = new ActivityRepository();

            // Convert all the ActivityBindingModels to Activities using an explicit operator.
            List<Activity> mobileUploadedList = ConvertActivityBindingModelList(activitiesToProcess);

            activityRepo.AddOrUpdateActivities(userId, mobileUploadedList);
        }

        /// <summary>
        /// Updates the list of UserVideo objects attached to a user. This will set the "IsWatched" flag if a video was watched.
        /// This sets the flag based off of the list of Activities that were uploaded from the mobile device.
        /// </summary>
        /// <returns>List of videos that are available for the user to download from the server.</returns>
        private List<UserVideo> ProcessBingoAndGetUnwatchedVideos(List<ActivityBindingModel> mobileUploadedActivities, 
            List<Video> videosOnDevice, Guid userId) {

            var userVideoRepo = new UserVideoRepository();
            var videoRepo = new VideoRepository();

            List<Activity> mobileActivities = ConvertActivityBindingModelList(mobileUploadedActivities);

            #region Loop through each video and determine whether it has been watched.
            // If the video has been watched then set its "IsWatched" flag to true.
            foreach (var video in videosOnDevice) {
                var videoActivities = mobileActivities.Where(a => a.VideoID == video.Id).ToList();

                // The number of completed activities for this video.
                // An activity is complete when the following datetime fields are set (notificationtime, starttime, endtime)
                int numCompletedActivities = videoActivities.Where(a => (a.NotificationTime != DateTime.MinValue) 
                                                            && (a.StartTime != DateTime.MinValue)
                                                            && (a.EndTime != DateTime.MinValue)).Count();

                if (numCompletedActivities > 0) {
                    // This video has been watched.
                    var userVideo = userVideoRepo.Get(userId, video.Id);
                    
                    if (userVideo == null)
                    {
                        // TODO: Remove this check upon V1 of the app since new users will never need to be checked.
                        // The user has watched a video that doesn't have a corresponding UserVideo record. This is usually
                        // because a user has a video on their device before this code was migrated to the live server.

                        var newUserVideo = new UserVideo()
                        {
                            UserId = userId,
                            VideoId = video.Id,
                            IsWatched = true
                        };

                        userVideoRepo.Add(newUserVideo);
                        continue;
                    }
                    else {
                        userVideo.IsWatched = true;

                        userVideoRepo.Edit(userVideo);
                    }
                }
            }
            #endregion

            var unwatchedUserVideos = userVideoRepo.GetFilteredUnwatchedRecords(userId);

            if (unwatchedUserVideos.Count == 0) {
                // The user has watched all videos. Mark all videos for this user as unwatched.
                userVideoRepo.MarkAllUserVideosAsUnwatched(userId);

                // Since the user has watched all videos we will now update the user's UserVideo list
                var allVideos = videoRepo.GetVideos();
                userVideoRepo.UpdateUserVideoListForUser(userId, allVideos);

                // update the unwatchedUserVideo list
                unwatchedUserVideos = userVideoRepo.GetFilteredUnwatchedRecords(userId);

                // Lastly we need to remove any videos that are already on the device. This is to prevent an issue where
                // the server recognizes all the videos currently on the phone as fresh due to the rollover. Remove them
                // from this filtered list but do not mark them as watched (only for rollover events).
                unwatchedUserVideos = unwatchedUserVideos.Where(v => 
                                                                    videosOnDevice.Find(dv => dv.Id == v.VideoId) == null
                                                                ).ToList();
            }

            return unwatchedUserVideos;

        }

        private float GetUnwatchedVideoPercentageOnDevice(List<Video> videosOnDevice, List<UserVideo> unwatchedUserVideos) {

            int numFreshVideosOnDevice = GetUnwatchedVideosOnDevice(videosOnDevice, unwatchedUserVideos).Count;

            if (videosOnDevice.Count == 0) {
                return 0; // Avoid mathmatical errors
            }

            return ((float)numFreshVideosOnDevice / (float)videosOnDevice.Count) * 100;
        }

        private List<UserVideoViewModel> GetMostFrequentlyWatchedVideoList(Guid userId, List<Activity> allUserActivities) {
            // first filter out any activities that refer to deleted or inactive videos
            var filteredActivities = allUserActivities
                .Where(a => a.Video.Active == true && a.Video.Deleted == false) // Is the video active and not deleted?
                .Where(a => a.EndTime != DateTime.MinValue) // Only include completed videos (this includes bonuses)
                .ToList();

            // Get a list of grouped activities which are grouped by videoId's. Each group is a list of activities
            // that refer to the same video ID. The root list is ordered by which list contains the most activities.
            // The list is ordered by each sub-lists count ordered from max->min count
            var groupedActivities = filteredActivities
                .GroupBy(a => a.VideoID)
                .Select(grp => grp.ToList())
                .OrderByDescending(grp => grp.Count())
                .ThenBy(grp => grp[0].Video.Title)
                .ToList();

            List<Video> mostFrequentlyWatchedVideos = new List<Video>();

            // NOTE: As of 6/7/17 we need to return only the top 3 watched videos
            for (int i = 0; i < 3; i++)
            {
                if (i >= groupedActivities.Count)
                {
                    // There are less than 3 videos watched.
                    break;
                }

                Video video = groupedActivities[i][0].Video; // There will always be at least one record in each group

                mostFrequentlyWatchedVideos.Add(video);
            }

            // Convert the list to UserVideoViewModels
            return ConvertVideosToUserVideoViewModels(userId, mostFrequentlyWatchedVideos);
            
        }

        private List<UserVideoViewModel> GetVideosToDelete(Guid userId, List<UserVideoViewModel> topFrequentVideos, 
            List<Video> videosOnDevice, List<UserVideo> unwatchedUserVideos)
        {

            // The starting list to filter from. Convert to a list of videos while we're at it.
            List<Video> videosToDelete = GetWatchedVideosOnDevice(userId, videosOnDevice, unwatchedUserVideos)
                                                .Select(uv => uv.Video)
                                                .ToList();

            // Filter out any topFrequentVideos from the list. -1 result in linq statment means the video does not exist in frequent list.
            videosToDelete = videosToDelete.Where(v => 
                                         topFrequentVideos.FindIndex(tv => tv.ID == v.Id) < 0
                                    ).ToList();

            // Add any videos that are on the device that were recently marked as inactive or deleted in admin portal
            foreach (var video in videosOnDevice) {
                if (video.Deleted || !video.Active) {
                    int indexCheck = videosToDelete.FindIndex(v => v.Id == video.Id);

                    if (indexCheck < 0) {
                        // This video does not exist in the watched list so we need to add it.
                        videosToDelete.Add(video);
                    }
                }
            }

            return ConvertVideosToUserVideoViewModels(userId, videosToDelete);

        }

        private List<UserVideoViewModel> GetVideosToDownload(Guid userId, float cacheSize, List<UserVideoViewModel> topMostFrequentVideos,
            List<UserVideoViewModel> videosToDelete, List<Video> videosOnDevice, List<UserVideo> unwatchedUserVideos)
        {
            // Convert topMostFrequentVideos to a list of Video objects.
            List<Video> mostFrequentVideos = ConvertUserVideoViewModelToVideos(topMostFrequentVideos);

            // Convert unwatchedUserVideos to a list of Video objects. We are doing this because if a user had no UserVideo objects (or new ones)
            // then the userVideo.Video parameter will be null here. So as a workaround for that edge case we'll just manually convert them here.
            var videoRepo = new VideoRepository();
            List<Video> unwatchedVideos = new List<Video>();
            foreach (var video in unwatchedUserVideos) {
                Video dBVideo = videoRepo.GetVideo(video.VideoId);
                unwatchedVideos.Add(dBVideo);
            }


            // First get any top most frequent videos that the mobile device does not have
            List<Video> topMostFrequentVideosToDownload = mostFrequentVideos.Where(v => 
                                                                                videosOnDevice.Find(dv => dv.Id == v.Id) == null
                                                                            ).ToList();

            // Next, get the list of videos that will be on the phone after deletion so we can calculate space left on phone.
            List<Video> videosOnDeviceAfterDeletion = videosOnDevice.Where(v =>
                                                                                videosToDelete.Find(dv => dv.ID == v.Id) == null
                                                                          ).ToList();

            // Now we need to get the space left on the device
            float storageUsedOnDevice = 0;

            try
            {
                // Add the size of the videos that will be left over after deleting videos from the phone.
                foreach (var video in videosOnDeviceAfterDeletion)
                {
                    storageUsedOnDevice += GetVideoSizeInMB(video.Path);
                }

                // Add the size of any top most frequent videos that we will be downloading to the phone.
                foreach (var video in topMostFrequentVideosToDownload)
                {
                    storageUsedOnDevice += GetVideoSizeInMB(video.Path);
                }
            }
            catch (Exception e) {
                // There was an error with finding the video or with creating the path for the video. Bubble the exception
                throw e;
            }

            float freeStorageOnDevice = cacheSize - storageUsedOnDevice; // Storage we can use to add unwatched bingo videos

            // Now we need to randomly get videos from the unwatched videos list that all add up to the freeStorageOnDevice
            List<Video> unwatchedVideosToDownload = new List<Video>();

            Random random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            List<Video> copyUnwatchedVideos = new List<Video>(unwatchedVideos); // So we can mutate the list safely

            while (freeStorageOnDevice > 0) {
                if (copyUnwatchedVideos.Count == 0) {
                    // There are no more videos to pick from 
                    break;
                }

                int index = random.Next(0, copyUnwatchedVideos.Count);

                Video video = copyUnwatchedVideos[index]; // The randomly selected video from the unwatched list.

                float videoSize = GetVideoSizeInMB(video.Path);

                // Check if we have enough space to add this video. If not then break out of loop.
                if ((freeStorageOnDevice - videoSize) <= 0) {
                    // There is no more space left on the phone to add this video. Really this could vary between videos
                    // but that is extra logic that the customer did not pay for.
                    break;
                }

                unwatchedVideosToDownload.Add(video); // If we made it here it means we can add this video to the list to download.

                copyUnwatchedVideos.RemoveAt(index); // Remove this video from the list we are picking from.

                // Finally we need to adjust the freeStorage size for the next loop
                freeStorageOnDevice -= videoSize;
            }

            // Since the client only wants there to be at most 15 videos on each device we need to remove any extra videos as needed.
            int totalVideos = videosOnDeviceAfterDeletion.Count + topMostFrequentVideosToDownload.Count + unwatchedVideosToDownload.Count;
            if (totalVideos > 15) {
                // More than 15 videos will be on the phone. Remove videos from unwatchedVideosToDownload list to get at/under 15 videos
                int difference = totalVideos - 15;

                for (int i = 0; i < difference; i++) {
                    if (unwatchedVideosToDownload.Count == 0) {
                        break; // Sanity Check
                    }

                    unwatchedVideosToDownload.RemoveAt(0);
                }
            }

            // FINALLY (bout time, whoa), add all the videos needed to download together for the final list
            unwatchedVideosToDownload.AddRange(topMostFrequentVideosToDownload);

            List<Video> videosToDownload = unwatchedVideosToDownload; // For clarity

            return ConvertVideosToUserVideoViewModels(userId, videosToDownload);
        }
        
        #endregion

        #region Sync Helper Methods

        private List<UserVideo> GetWatchedVideosOnDevice(Guid userId, List<Video> videosOnDevice, List<UserVideo> unwatchedUserVideos)
        {
            var userVideoRepo = new UserVideoRepository();
            var watchedVideosOnDevice = new List<UserVideo>();

            foreach (var video in videosOnDevice)
            {
                var userVideo = unwatchedUserVideos.Where(uv => uv.VideoId == video.Id).SingleOrDefault();

                if (userVideo == null)
                {
                    // This video is watched since it's not in the unwatched user video list
                    userVideo = userVideoRepo.Get(userId, video.Id);
                    watchedVideosOnDevice.Add(userVideo);
                }
            }

            return watchedVideosOnDevice;
        }

        private List<UserVideo> GetUnwatchedVideosOnDevice(List<Video> videosOnDevice, List<UserVideo> unwatchedUserVideos)
        {
            var unwatchedVideosOnDevice = new List<UserVideo>();

            foreach (var video in videosOnDevice)
            {
                var userVideo = unwatchedUserVideos.Where(uv => uv.VideoId == video.Id).SingleOrDefault();

                if (userVideo == null)
                {
                    continue; // The video on the phone is watched and not fresh.
                }

                // This video exists in the fresh video list meaning the user still has not watched it.
                unwatchedVideosOnDevice.Add(userVideo);
            }

            return unwatchedVideosOnDevice;
        }

        /// <summary>
        /// Converts a list of ActivityBindingModels to a list of Activity objects.
        /// </summary>
        /// <param name="listToConvert">The list of ActivityBindingModels to convert</param>
        /// <returns>A list of Activity objects.</returns>
        private List<Activity> ConvertActivityBindingModelList(List<ActivityBindingModel> listToConvert)
        {
            List<Activity> convertedList = new List<Activity>();
            foreach (var bindingModel in listToConvert)
            {
                convertedList.Add((Activity)bindingModel);
            }

            return convertedList;
        }

        private List<UserVideoViewModel> ConvertVideosToUserVideoViewModels(Guid userId, List<Video> videosToConvert)
        {
            var convertedVideos = new List<UserVideoViewModel>();

            foreach (var video in videosToConvert)
            {
                var videoViewModel = new UserVideoViewModel();

                // Properties from VideoViewModel
                videoViewModel.ID = video.Id;
                videoViewModel.Path = video.Path;
                videoViewModel.Type = video.Type.ToString();
                videoViewModel.Tags = video.Tags;
                videoViewModel.Title = video.Title;
                videoViewModel.Description = video.Description;
                videoViewModel.Active = video.Active;
                videoViewModel.DateModified = video.DateModified;
                videoViewModel.DateUploaded = video.DateUploaded;

                // Properties from UserVideoViewModel
                videoViewModel.UserID = userId;
                videoViewModel.LastPlayed = DateTime.MinValue; // This will be set to MinValue again on the mobile side as a Sanity check.
                videoViewModel.DownloadDate = DateTime.MinValue; // Also set on mobile side
                videoViewModel.IsFavorite = false; // This isn't used yet on either side (i.e. mobile or server)
                videoViewModel.ViewCount = 0; // This also isn't used
                videoViewModel.DownloadedSuccessfully = false; // Also set on mobile side.
                videoViewModel.Deleted = video.Deleted;
                videoViewModel.FlaggedForDeletion = !video.Deleted; // Pretty sure this is not used either.

                convertedVideos.Add(videoViewModel);
            }

            return convertedVideos;
        }

        private List<Video> ConvertUserVideoViewModelToVideos(List<UserVideoViewModel> videosToConvert)
        {
            var videoRepo = new VideoRepository();

            List<Video> videos = new List<Video>();

            foreach (var videoView in videosToConvert)
            {
                var dBVideo = videoRepo.GetVideo(videoView.ID);

                if (dBVideo == null)
                {
                    throw new Exception("Could not find video record for video " + videoView.Title);
                }

                videos.Add(dBVideo);
            }

            return videos;
        }

        /// <summary>
        /// Takes a video URL path and returns the size of the video file in megabytes if it exists.
        /// </summary>
        /// <param name="path">The URL path of the video we need the size of.</param>
        /// <returns>The size of the video in megabytes</returns>
        private float GetVideoSizeInMB(string path)
        {

            int startIndex = path.LastIndexOf("/") + 1;
            string title = path.Substring(startIndex);

            string filePath = "";

            try
            {
                filePath = HttpContext.Current.Server.MapPath("/Portal/Content/Video/" + title);
            }
            catch (Exception e) {
                // there was an error mapping the path. Continue to throw the exception to the caller.
                throw e;
            }

            if (System.IO.File.Exists(filePath) == false)
            {
                throw new Exception("File " + title + " does not exist. Could not get size.");
            }

            return new FileInfo(filePath).Length / 1000000; // In MB
        }
        #endregion
    }
}
