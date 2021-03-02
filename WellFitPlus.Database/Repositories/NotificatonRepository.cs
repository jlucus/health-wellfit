using log4net;
using System;
using System.Linq;
using WellFitPlus.Database.Entities;

namespace WellFitPlus.Database.Repositories {
    public class NotificationRepository : DataRepositoryBase {

        public static readonly ILog log = LogManager.GetLogger(typeof(NotificationRepository));

        public Guid Add(NotificationSetting notification) {
            try {

                _context.NotificationSettings.Add(notification);
                _context.SaveChanges();

            } catch (Exception ex) {
                log.Error(ex);
                return Guid.Empty;
            }
            return notification.Id;
        }

        public void Edit(NotificationSetting notification) {
            try {
                _context.SaveChanges();

            } catch (Exception ex) {
                log.Error(ex);
            }
        }

        public NotificationSetting GetNotification(Guid userID) {
            NotificationSetting notification = new NotificationSetting();

            try {
                notification = _context.NotificationSettings.Where(s => s.UserID == userID).FirstOrDefault();

            } catch (Exception ex) {
                log.Error(ex);
                return null;
            }
            return notification;
        }
    }
}
