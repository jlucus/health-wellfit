using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using WellFitPlus.Database.Entities;

namespace WellFitPlus.WebAPI.Models
{
    public class SyncResponseViewModel
    {
        [Required]
        public List<UserVideoViewModel> VideosToDelete { get; set; }

        [Required]
        public List<UserVideoViewModel> VideosToDownload { get; set; }

        /// <summary>
        /// A list of videos to mark as a most frequently watched or "favorite". The mobile app does not download these, rather, 
        /// it marks already existing records of videos as a "favorite" or not. 
        /// If a developer would like to add a favorite video to be downloaded add the video to the "VideosToDownload" list in this class.
        /// 
        /// A new "favorite" that is not downloaded will take this flow on the app. It will set the record of the needed video to download
        /// in its mobile device's database (from the VideosToDownload property). Then it will set it as a "favorite" in the frequentVideo 
        /// table in the mobile device's database. After that the mobile device will then download the actual video file.
        /// </summary>
        [Required]
        public List<UserVideoViewModel> TopMostFrequentVideos { get; set; }

        /// <summary>
        /// This will populate with all of the user's Activities. This is to keep both server and app in sync
        /// with each other as well as keeping the statistics on each user's phone consistent.
        /// This will also be populated with the most recently updloaded/updated Activities after a sync request.
        /// </summary>
        [Required]
        public List<Activity> AllUserActivities { get; set; }
    }
}