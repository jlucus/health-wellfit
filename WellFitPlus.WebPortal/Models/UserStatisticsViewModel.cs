using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ComponentModel.DataAnnotations;

using WellFitPlus.Database.Entities;

namespace WellFitPlus.WebPortal.Models
{
    public class UserStatisticsViewModel
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public List<Activity> Activities { get; set; }

        public int CurrentStreak {
            get {

                // List from most recent to oldest activities.
                var activities = Activities
                    .Where(a => a.NotificationTime != DateTime.MinValue) // Filter out pure bonuses
                    .OrderByDescending(a => a.StartTime).ToList();

                int currentStreak = 0;

                // Set the current streak
                for (int i = 0; i < activities.Count; i++) {
                    var activity = activities[i];
                     
                    if (HasActivityBeenCompleted(activity))
                    {
                        currentStreak++;
                    }
                    else {
                        break; // We have come to the end of our current streak
                    }
                }

                return currentStreak;
            }
        }

        public int BestStreak
        {
            get
            {

                // List from most recent to oldest activities.
                var activities = Activities
                    .Where(a => a.NotificationTime != DateTime.MinValue) // Filter out pure bonuses
                    .OrderByDescending(a => a.StartTime).ToList();

                int largestStreak = 0;
                int currentCount = 0;

                // Go through the entire activities list and get each streak. Compare each streak with the last one found and save the largest
                // streak number.
                for (int i = 0; i < activities.Count; i++)
                {
                    var activity = activities[i];

                    if (HasActivityBeenCompleted(activity))
                    {
                        currentCount++;
                    }
                    else
                    {
                        currentCount = 0; // We have come to the end of this streak
                    }

                    largestStreak = Math.Max(currentCount, largestStreak);
                }

                return largestStreak;
            }
        }

        public int CompletedSessions
        {
            get
            {
                // Get all the completed activities for this week. NOT including pure bonuses.
                return Activities.Where(a => HasActivityBeenCompleted(a) == true).Count();
            }
        }

        public int ScheduledSessions
        {
            get
            {
                return Activities.Where(a => IsActivityPureBonus(a) == false).Count();
            }
        }

        /// <summary>
        /// Returns the number of bonuses for the activities
        /// 
        /// This includes both pure bonuses and bonuses from replaying a scheduled session.
        /// </summary>
        public int BonusSessions
        {
            get
            {
                return Activities.Where(a => a.Bonus == true).Count();
            }
        }

        public int BonusesPlusScheduledSessionsCompleted {
            get {
                int numBonuses = Activities.Where(a => a.Bonus == true).Count();
                int numScheduledSessionsCompleted = Activities.Where(a => HasActivityBeenCompleted(a) == true).Count();

                return numBonuses + numScheduledSessionsCompleted;
            }
        }

        /// <summary>
        /// Returns whether the activity has been fully completed.
        /// 
        /// NOTE: if this activity is a pure bonus then this will always return false since this method expects there to be a 
        ///       notification time.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private bool HasActivityBeenCompleted(Activity activity) {
           return  activity.StartTime > DateTime.MinValue
                        && activity.EndTime > DateTime.MinValue
                        && activity.NotificationTime > DateTime.MinValue;
        }

        // Pure Bonus activities are activities that were not scheduled by the system. I.e. the user went into the app and manually
        // started and watched a video. So these activities will not have a notification time.
        private bool IsActivityPureBonus(Activity activity) {
            return activity.NotificationTime == DateTime.MinValue; // If Bonus is not set then this was a history video that wasn't completed
        }
    }
}

