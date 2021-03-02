//	This class is used to handle the "ScheduledNotification" table. These notifications are just records
//	of what notifications are currently planned on being alerted to the user. The main reasoning behind this
//	is to better keep track of what notifications have been sent and what still needs to be sent. It will
// 	allow us to track notifications better and to see if the user has not swiped any notification.
// 	So far the main use is with iOS

using System;
using System.Linq;
using System.Collections.Generic;
using WellFitPlus.Mobile.Models;

namespace WellFitPlus.Mobile.Database.Repositories
{
	public class NotificationRepository
	{

		#region Static Fields
		private static NotificationRepository _instance;
		#endregion

		#region Static Properties
		public static NotificationRepository Instance { 
			get
			{
				if (_instance == null) {
					_instance = new NotificationRepository();
				}
				return _instance;
			}
		}
		#endregion

		private readonly LocalDatabaseContext _context;

		public NotificationRepository()
		{
			_context = new LocalDatabaseContext();
		}

		public void AddNotification(ScheduledNotification notification)
		{
			if (notification.Id == 0) { 
				_context.DB.Insert(notification);
			}
		}

		public void AddNotifications(List<ScheduledNotification> notifications) { 
			var newNotifications = notifications.Where(n => n.Id == 0).ToList(); // Sanity Check

			_context.Lock.WaitOne();
			_context.DB.InsertAll(newNotifications);
			_context.Lock.ReleaseMutex();
		}

		public ScheduledNotification GetNotification(DateTime notificationTime)
		{
			_context.Lock.WaitOne();

			var notification = _context.DB.Table<ScheduledNotification>()
						   .Where(n => n.ScheduledTimestamp == notificationTime)
						   .FirstOrDefault();

			_context.Lock.ReleaseMutex();

			return notification;
		}

		public List<ScheduledNotification> GetNotifications() {
			_context.Lock.WaitOne();

			var notifications = _context.DB.Table<ScheduledNotification>().OrderBy(n => n.ScheduledTimestamp).ToList();

			_context.Lock.ReleaseMutex();

			return notifications;
		}

		public void UpdateNotifications(List<ScheduledNotification> notifications) {
			var newNotifications = notifications.Where(n => n.Id == 0).ToList();
			var existingNotifications = notifications.Where(n => n.Id != 0).ToList();

			_context.Lock.WaitOne();

			_context.DB.InsertAll(newNotifications);
			_context.DB.UpdateAll(existingNotifications);

			_context.Lock.ReleaseMutex();
		}

		public bool DeleteNotification(ScheduledNotification notification) {
			if (notification.Id != 0)
			{
				_context.Lock.WaitOne();

				_context.DB.Delete(notification);

				_context.Lock.ReleaseMutex();
				return true;
			}
			else {
				return false;
			}
		}

		public void DeleteNotifications(List<ScheduledNotification> notifications) {
			var filteredList = notifications.Where(n => n.Id != 0);

			_context.Lock.WaitOne();

			foreach (ScheduledNotification notification in filteredList) {
				_context.DB.Delete(notification);
			}

			_context.Lock.ReleaseMutex();
		}

		public void DeleteAllRecords() {

			_context.Lock.WaitOne();

			_context.DB.DeleteAll<ScheduledNotification>();

			_context.Lock.ReleaseMutex();
		}

	}
}

