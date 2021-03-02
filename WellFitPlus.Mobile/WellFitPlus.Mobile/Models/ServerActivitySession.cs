using System;
using SQLite;

using WellFitPlus.Mobile.Models;

// Used for sending mobile created activities to the server.
// Since the pre-populated SQLite database uses auto-incrementing integers for the primary keys of the ActivitySessions
// they are not able to be directly uploaded to the server since they have different primary keys.
namespace WellFitPlus.Mobile
{
	public class ServerActivitySession
	{
		public ServerActivitySession()
		{
		}

		#region Properties

		// NOTE:    this is used for SQLite's internal management of objects, NOT for the
		//          server database's or WebAPI's management of corresponding items/entries
		public Guid Id { get; set; }

		public Guid UserId { get; set; }

		public Guid VideoId { get; set; }

		public DateTime NotificationTime { get; set; }

		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }

		public bool Bonus { get; set; }

		public bool Acknowledged { get; set; }

		public bool HasBeenUploaded { get; set; }

		public bool IsPending { get; set; }

		#endregion


		public static explicit operator ActivitySession(ServerActivitySession serverAct) {
			ActivitySession activity = new ActivitySession();

			// NOTE: We cannot set the actual activity ID here since it only exists in the local database.
			activity.ActivityId = serverAct.Id;
			activity.UserId = serverAct.UserId;
			activity.VideoId = serverAct.VideoId;
			activity.NotificationTime = serverAct.NotificationTime;
			activity.StartTime = serverAct.StartTime;
			activity.EndTime = serverAct.EndTime;
			activity.Bonus = serverAct.Bonus;
			activity.Acknowledged = true;
			activity.HasBeenUploaded = true;
			activity.IsPending = false;

			return activity;
		
		}
	}
}
