using System;
using System.Collections.Generic;
using System.Text;

using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Services;

namespace WellFitPlus.Mobile.Abstractions
{

    public interface INotificationScheduler
    {
		// NOTE: notifications need to be ordered by closest time to furthest time
		void RegisterNotificationsWithOS(List<ScheduledNotification> notificationsToSchedule);

        void CancelOSRegisteredNotifications();

		void AlertUserIfNewNotificationExists();

		void StartVideoPlaybackIfNewNotificationExists();

		bool HasNotificationAuthorization();

		NotificationState HasRecievedNotification();
    }
}
