using log4net;
using System;
using System.Data.Entity;
using System.Linq;
using WellFitPlus.Database.Entities;
using System.Collections.Generic;

namespace WellFitPlus.Database.Repositories
{
    public class UserVideoRepository : DataRepositoryBase
    {

        /// <summary>
        /// Gets a UserVideo record based upon it's composite key.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="videoId"></param>
        /// <returns>The UserVideo record if exists or null if a record could not be found.</returns>
        public UserVideo Get(Guid userId, Guid videoId) {
            return _context.UserVideos
                .Where(uv => uv.UserId == userId && uv.VideoId == videoId)
                .SingleOrDefault();
        }

        /// <summary>
        /// Gets all unwatched records that can be used for the "bingo". This includes filtering out
        /// the records by the corresponding Video record where the Video is not marked as deleted and is
        /// also active.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>A list of UserVideo </returns>
        public List<UserVideo> GetFilteredUnwatchedRecords(Guid userId) {
            return _context.UserVideos
                .Where(uv => uv.IsWatched == false && uv.UserId == userId)
                .Where(uv => uv.Video.Deleted == false && uv.Video.Active == true)
                .ToList();
        }

        public List<UserVideo> GetAll() {
            return _context.UserVideos.ToList();
        }

        public List<UserVideo> GetAllForUser(Guid userId) {
            return _context.UserVideos
                .Where(uv => uv.UserId == userId)
                .ToList();
        }

        public List<UserVideo> GetByUserId(Guid userId) {
            return _context.UserVideos
                .Where(uv => uv.UserId == userId)
                .ToList();
        }

        public void Add(UserVideo userVideo) {
            _context.UserVideos.Add(userVideo);
            _context.SaveChanges();
        }

        public void Edit(UserVideo userVideo) {
            var dbRecord = Get(userVideo.UserId, userVideo.VideoId);

            if (dbRecord != null) {
                dbRecord.IsWatched = userVideo.IsWatched;
            }

            _context.SaveChanges();
        }

        /// <summary>
        /// Sets the "IsWatched" flag to false for all records for the specified user. This should be used when a user has
        /// watched all of their videos and they need to start over.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>List of all the user's UserVideo objects that have been set to unwatched.</returns>
        public List<UserVideo> MarkAllUserVideosAsUnwatched(Guid userId) {
            var allUserVideos = GetAllForUser(userId);

            foreach (var userVideo in allUserVideos) {
                userVideo.IsWatched = false;
            }

            _context.SaveChanges();

            return allUserVideos;
        }

        /// <summary>
        /// Takes the full list of videos currently offered in the admin (including deleted and inactive) and adds them
        /// to the list of UserVideos for a specific user.
        /// 
        /// This will only add videos that don't exist yet in the user's UserVideo list. Only call this method when the user has
        /// fully watched the entire list of UserVideos to prevent bingo issues.
        /// 
        /// ex of issue. If the admin upload single videos at the right interval then it is possible for the mobile app
        /// to only get one or two unwatched videos from the server until the full list has been watched. Waiting until the 
        /// user has watched all videos to call this method prevents this issue.
        /// </summary>
        /// <param name="userId">The User ID of the user who's UserVideo records will be updated.</param>
        /// <param name="fullVideoList">
        /// The entire list of Video objects currently in the database. (includes deleted and inactive records, so if the admin
        /// decides to un-delete or make a video active again then it will be included out-of-the-box rather than making more functionality)
        /// </param>
        public void UpdateUserVideoListForUser(Guid userId, List<Video> fullVideoList) {
            var allUserVideos = GetAllForUser(userId);

            foreach (var video in fullVideoList) {
                var checkVideo = allUserVideos.Where(uv => uv.VideoId == video.Id).SingleOrDefault();

                if (checkVideo == null) {
                    // We need to add this video to the UserTable
                    Add(new UserVideo() {
                        UserId = userId,
                        VideoId = video.Id,
                        IsWatched = false
                    });
                }
            }

        }


    }
}
