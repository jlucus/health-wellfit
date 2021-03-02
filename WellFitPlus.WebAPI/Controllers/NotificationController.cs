using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using log4net;
using WellFitPlus.WebAPI.BindingModels;
using WellFitPlus.Common;
using WellFitPlus.Database.Entities;
using WellFitPlus.Database.Repositories;

namespace WellFitPlus.WebAPI.Controllers {
    [RoutePrefix("api/Notification")]
    [Authorize]
    public class NotificationController : ApiController
    {
        public static readonly ILog log = LogManager.GetLogger(typeof(NotificationController));

        private NotificationRepository _notificationRepo = new NotificationRepository();
        
        [HttpPost]
        [Route("Add")]
        public bool Add(NotificationBindingModel notificationView) {
            try {
                NotificationSetting notification = new NotificationSetting();

                UpdateModel(ref notification, notificationView);

                _notificationRepo.Add(notification);

            } catch (Exception ex) {
                log.Error(ex);
                return false;
            }
            return true;
        }

        [HttpPost]
        public void Edit(NotificationBindingModel notificationView) {
            try {
                NotificationSetting notification = _notificationRepo.GetNotification(notificationView.UserID);
                if (notification != null) {

                    UpdateModel(ref notification, notificationView);
                    _notificationRepo.Edit(notification);
                }
            } catch (Exception ex) {
                log.Error(ex);
            }
        }

        private void UpdateModel(ref NotificationSetting notification, NotificationBindingModel notificationView) {

            notification.Active = notificationView.Active;
            notification.BeginTime = notificationView.BeginTime;
            notification.EndTime = notificationView.EndTime;
            notification.Frequency = (NotificationFrequency)Enum.Parse(typeof(NotificationFrequency), notificationView.Frequency);
            notification.ResumeOn = notificationView.ResumeOn;
            notification.UserID = notificationView.UserID;
            notification.Days = notificationView.Days;
        }
    }
}
