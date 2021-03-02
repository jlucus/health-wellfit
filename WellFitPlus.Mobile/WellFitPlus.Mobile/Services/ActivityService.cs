using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Services;
using Xamarin.Forms;

namespace WellFitPlus.Mobile.Services
{
    public class ActivityService
    {

		#region Static Fields
		private static ActivityService _instance;
		#endregion

		#region Static Properties
		public static ActivityService Instance { 
			get {
				if (_instance == null) {
					_instance = new ActivityService();
				}
				return _instance;
			}
		}
		#endregion

        #region Private Fields
        private  DateTime _lastNotifyDate;
        private  DateTime _lastAcknowledgeDate;
        private  DateTime _startDate = DateTime.Now;
        private  bool _isBusy = false;
        private static bool _hasServiceStarted = false;
		#endregion

		#region Properties
		public DateTime ServiceStartDate
		{
			get
			{
				return _startDate;
			}
		}
		#endregion

		#region Public Fields
		public UserSettings UserSettings;
        public int NotificationCheckCount = 0;
        public int NotificationCount = 0;
        #endregion

        #region Initialization

        private ActivityService()
        {
            // Default Dates
			_lastNotifyDate = DateTime.MinValue;
			_lastAcknowledgeDate = DateTime.MinValue;

		   _hasServiceStarted = true;
		    // Subscribe To The Update Activity Event
		    MessagingCenter.Subscribe<ActivitySession>(this, AppGlobals.Events.COMPLETE_ACTIVITY, (activity) =>
		   {
			   // Update Activity
			   //Task.Run(() => this.CompleteActivity(activity));
			   this.CompleteActivity(activity);
		   });

		    // Subscribe To The Acknowledge Activity Event
		    MessagingCenter.Subscribe<ActivitySession>(this, AppGlobals.Events.ACTIVITY_ACKNOWLEDGED, (activity) =>
		   {
		        // Update Activity
		        this.AcknowledgeActivity(activity);
		   });

        }

        #endregion

        #region Check Notify

        public bool CheckNotify()
        {
            // If Notification Active
            if (_isBusy == true) { return false; }

            NotificationCheckCount += 1;

            try
            {
                // Get Count Of Videos Downloaded Successfully That Have Not Been Deleted
				var videosDownloadedSuccessfully = new VideoRepository().GetPlayableVideos().Count();

                // Validation
                if (videosDownloadedSuccessfully == 0) { return false; }

                MessagingCenter.Send<string>("", AppGlobals.Events.REFRESH_PROFILE);

                _isBusy = true;
                // Reload User Settings Every Interval To Ensure Latest Values
                this.UserSettings = UserSettings.GetExistingSettings();

                #region Check User Can Be Notified (Settings)

                // Get The Day Of The Week
                UserSettings.DaysOfWeek today = this.GetDayOfWeek();

                // Check If The Service Can Send Notifications Today
                bool canNotifyToday = this.UserSettings.NotificationDays.HasFlag(today);
                if (canNotifyToday == false) { return false; }

                var now = DateTime.Now.Hour;

                // Check Can Notify In This Hour
                bool canNotifyCurrentHour = (this.UserSettings.BeginHour < this.UserSettings.EndHour || now >= this.UserSettings.BeginHour || now <= this.UserSettings.EndHour); //|| this.UserSettings.EndHour <= 1;

                // If Can Notify In This Hour
                if (canNotifyCurrentHour)
                {
                    // If No Notifications Have Been Created Yet
                    if (_lastNotifyDate == DateTime.MinValue
                        && _startDate.AddMinutes(this.UserSettings.Frequency) <= DateTime.Now
                        && _lastAcknowledgeDate.AddMinutes(this.UserSettings.Frequency) <= DateTime.Now)
                    {
                        _lastNotifyDate = DateTime.Now;
                    }
                    // If Interval Has Elapsed Since Last Notification
                    else if (_lastNotifyDate.AddMinutes(this.UserSettings.Frequency) < DateTime.Now)

                    {
                        _lastNotifyDate = DateTime.Now;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                #endregion
                
                App.Log("User Can Now Be Notified For Workout");
                                
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                _isBusy = false;
            }
        }

        private UserSettings.DaysOfWeek GetDayOfWeek()
        {
            // Determine Day Of Week
            switch (DateTime.Today.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return UserSettings.DaysOfWeek.Monday;
                case DayOfWeek.Tuesday:
                    return UserSettings.DaysOfWeek.Tuesday;
                case DayOfWeek.Wednesday:
                    return UserSettings.DaysOfWeek.Wednesday;
                case DayOfWeek.Thursday:
                    return UserSettings.DaysOfWeek.Thursday;
                case DayOfWeek.Friday:
                    return UserSettings.DaysOfWeek.Friday;
                case DayOfWeek.Saturday:
                    return UserSettings.DaysOfWeek.Saturday;
                case DayOfWeek.Sunday:
                    return UserSettings.DaysOfWeek.Sunday;
            }

            return UserSettings.DaysOfWeek.None;
        }

        #endregion

        #region Create

        public Notification CreateNotification(string title, string message, string icon)
        {
            // Create Notification
            var notification = new Notification()
            {
                Title = title,
                Icon = icon,
                Message = message,
                Vibrate = true,
                Priority = Models.Notification.PriorityType.Maximum,
                Sound = false,
                Category = Models.Notification.CategoryType.CategoryService,
                LargeIcon = true,
                NotificationTime = DateTime.Now
            };
            
            return notification;
        }

		/// <summary>
		/// Creates an ActivitySession and adds it to the Database.
		/// </summary>
		/// <returns>The activity added to the database.</returns>
        public ActivitySession CreateActivity()
        {
            try
            {
                // Validation
                if (this.UserSettings == null)
                {
                    this.UserSettings = UserSettings.GetExistingSettings();
                }

                // Create Repo
				ActivitySessionRepository activityRepo = ActivitySessionRepository.Instance;
                
                // Get Video ID
                var videoId = this.GetNextVideoIdToPlay();

                // Validation
                if (videoId == Guid.Empty)
                {
                    return null;
                }

                // Create Activity
				ActivitySession activity = new ActivitySession()
                {
                    VideoId = videoId == Guid.Empty ? Guid.NewGuid() : videoId,
                    StartTime = DateTime.Now,
                    NotificationTime = DateTime.Now,
                    Bonus = false,
                    UserId = this.UserSettings.UserId,
                    Acknowledged = false,
                    IsPending = true
                };

                // Add Session
                activityRepo.AddActivity(activity);

                App.Log("Activity Created. ID: " + activity.Id.ToString());

                // Trigger Create Activity Event
                MessagingCenter.Send<string>(activity.Id.ToString(), AppGlobals.Events.REFRESH_PROFILE);

                return activity;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

		/// <summary>
		/// Gets the new activity without adding it to the repo. Used for bonus videos mostly.
		/// </summary>
		/// <returns>The new activity.</returns>
		public ActivitySession GetNewActivity()
		{
			try
			{
				// Validation
				if (this.UserSettings == null)
				{
					this.UserSettings = UserSettings.GetExistingSettings();
				}

				// Get Video ID
				var videoId = this.GetNextVideoIdToPlay();

				// Validation
				if (videoId == Guid.Empty)
				{
					return null;
				}

				// Create Activity
				ActivitySession activity = new ActivitySession()
				{
					VideoId = videoId,
					StartTime = DateTime.Now,
					NotificationTime = DateTime.Now, // To show that this hasn't been scheduled.
					Bonus = false,
					UserId = this.UserSettings.UserId,
					Acknowledged = true,
					IsPending = false
				};

				return activity;
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the next video identifier to play. This uses the Bingo to determine the best video to watch next.
		/// If for some reason any playable videos cannot be found then this will return a Guid.Empty as an error.
		/// </summary>
		/// <returns>Video Guid to play or Guid.Empty upon error or if no playable videos exist.</returns>
        private Guid GetNextVideoIdToPlay()
        {
          
			// This is the list we will actually use to pick from.
			var filteredVideos = new List<Video>();

			var freshBingoVideos = new VideoRepository().GetFreshBingoVideos();

			// Sanity check
			if (freshBingoVideos.Count == 0) {
				return Guid.Empty;
			}

			// Newly downloaded videos will have a MinValue for the Last Played date. If any of these exist
			// then they will take first priority since they are guaranteed to have never been watched.
			var priorityVideos = freshBingoVideos.Where(v => v.LastPlayed == DateTime.MinValue).ToList();

			if (priorityVideos.Count > 0)
			{
				// We have guaranteed unwatched videos. Use those for selection.
				filteredVideos = priorityVideos;
			}
			else {
				// default to using the unfiltered fresh bingo videos
				filteredVideos = freshBingoVideos;
			}

			// Seeded random number generator. Seeded based off of time to make it more random.
			Random random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
			int randomIndex = random.Next(0, filteredVideos.Count); // second parameter value is exclusive

			return filteredVideos[randomIndex].ID;
        }

        #endregion

        #region Action

        public bool AcknowledgeActivity(ActivitySession activity)
        {
            try
            {
                App.Log("Activity Acknowledged. ID: " + activity.Id.ToString());

                // Get Activity
                ActivitySessionRepository activityRepo = ActivitySessionRepository.Instance;
                activity.Acknowledged = true;
                activity.IsPending = false;

                // Update Activity
                var updateActivityResult = activityRepo.UpdateActivity(activity);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        
        private int CompleteActivity(ActivitySession activity)
        {
            try
            {
                App.Log("Activity Completed. ID: " + activity.Id.ToString());

                // Get Activity
                ActivitySessionRepository activityRepo = ActivitySessionRepository.Instance;
                activity.Acknowledged = true;
                activity.IsPending = false;

                // Update Activity
                activityRepo.UpdateActivity(activity);

                MessagingCenter.Send<string>("", AppGlobals.Events.REFRESH_PROFILE);

                // Update Video Last Played
                VideoRepository videoRepo = new VideoRepository();
				var video = videoRepo.GetVideo(activity.VideoId);

                // If Video Exists
                if (video != null)
                {
                    // Update Last Played Date
                    video.LastPlayed = activity.EndTime;
                    video.ViewCount += 1;

					// Set video as watched for the local phone bingo
					video.IsWatched = true;

					videoRepo.InsertOrUpdateVideo(video);
                }

                MessagingCenter.Send<string>("", AppGlobals.Events.REFRESH_PROFILE);

                return 1;                                                                  
            }
            catch (Exception ex)
            {
                return -1;
            } 
        }

        #endregion
    }
}
