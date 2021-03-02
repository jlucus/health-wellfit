using System;
using System.Linq;
using System.Collections.Generic;
using WellFitPlus.Mobile.Database;
using WellFitPlus.Mobile.Database.Repositories;
using SQLite;

namespace WellFitPlus.Mobile.Models
{
    public class ActivitySession
    {
         #region Properties

        // NOTE:    this is used for SQLite's internal management of objects, NOT for the
        //          server database's or WebAPI's management of corresponding items/entries
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

		// Unfortunately since there is pre-existing data that can't be overridden or cleared out
		// we cannot place the [NotNull] attribute on this property.
		public Guid ActivityId { get; set; } // The ID used on the server. This will also be created on the app.

        [NotNull]
        public Guid UserId { get; set; }

        [NotNull]
        public Guid VideoId { get; set; }

        [NotNull]
        public DateTime NotificationTime { get; set; }
        
        [NotNull]
        public DateTime StartTime { get; set; }

        [NotNull]
        public DateTime EndTime { get; set; }

        [NotNull]
        public bool Bonus { get; set; }

        [NotNull]
        public bool Acknowledged { get; set; }

        [NotNull]
        public bool HasBeenUploaded { get; set; }

        [NotNull]
        public bool IsPending { get; set; }

        public bool IsCompleted
        {
            get
            {
                bool completed = 
                    this.StartTime > DateTime.MinValue &&
                    this.EndTime > DateTime.MinValue &&
                    this.NotificationTime > DateTime.MinValue  &&
                    this.IsPending == false && 
                    this.Acknowledged == true;
				
                return completed;
            }
        }

		// If this instance is a bonus activity that was never scheduled.
		[Ignore]
		public bool IsBonusAndNotScheduled { 
			get {
				return this.NotificationTime == DateTime.MinValue;
			}
		}

		#endregion

		#region Initialization

		// Needed to use for SQLite
		public ActivitySession() {
			StartTime = DateTime.Now;
			ActivityId = Guid.NewGuid();
		}

		#endregion

		public static explicit operator ServerActivitySession(ActivitySession activity) {
			ServerActivitySession serverAct = new ServerActivitySession();

			serverAct.Id = activity.ActivityId;

			serverAct.UserId = activity.UserId;
			serverAct.VideoId = activity.VideoId;
			serverAct.NotificationTime = activity.NotificationTime;
			serverAct.StartTime = activity.StartTime;
			serverAct.EndTime = activity.EndTime;
			serverAct.Bonus = activity.Bonus;
			serverAct.Acknowledged = activity.Acknowledged;
			serverAct.HasBeenUploaded = activity.HasBeenUploaded;
			serverAct.IsPending = activity.IsPending;

			return serverAct;
		}
    }
}
