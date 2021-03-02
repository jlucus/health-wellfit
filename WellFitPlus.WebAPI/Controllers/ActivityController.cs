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

namespace WellFitPlus.WebAPI.Controllers
{
    [RoutePrefix("api/Activity")]
    public class ActivityController : ApiController
    {
        public static readonly ILog log = LogManager.GetLogger(typeof(ActivityController));

        private ActivityRepository _activityRepo = new ActivityRepository();
        private VideoRepository _videoRepo = new VideoRepository();

        [HttpPost]
        [Route("Add")]
        public bool Add(ActivityBindingModel actView) {
            try {
                Activity activity = new Activity();
                
                activity.Bonus = actView.Bonus;
                activity.EndTime = (DateTime)actView.EndTime;
                activity.NotificationTime = actView.NotificationTime;
                activity.StartTime = (DateTime)actView.StartTime;
                activity.VideoID = actView.VideoID;
                activity.UserID = actView.UserID;
                activity.Id = actView.Id;
                
                _activityRepo.Add(activity);

            } catch (Exception ex) {
                log.Error(ex);
                return false;
            }
            return true;
        }

        [HttpPost]
        [Route("GetByUser")]
        public List<ActivityBindingModel> GetActivities(ActivityBindingModel activity) {
            List<Activity> activities = new List<Activity>();
            try {
                if (activity.StartTime.Equals(DateTime.MinValue) && activity.EndTime.Equals(DateTime.MinValue)) {
                    activities = _activityRepo.GetActivities(activity.UserID);
                } else {
                    activities = _activityRepo.GetActivities(activity.UserID, (DateTime)activity.StartTime, (DateTime)activity.EndTime);
                }
                return CreateActivityViews(activities);

            } catch (Exception ex) {
                log.Error(ex);
                return null;
            }
        }

        private List<ActivityBindingModel> CreateActivityViews(List<Activity> actList) {
            List<ActivityBindingModel> activityViews = new List<ActivityBindingModel>();

            foreach (Activity act in actList) {
                ActivityBindingModel actView = new ActivityBindingModel();

                actView.Bonus = act.Bonus;
                actView.EndTime = act.EndTime;
                actView.StartTime = act.StartTime;
                actView.NotificationTime = act.NotificationTime;

                actView.UserID = act.UserID;
                actView.Id = act.Id;
                actView.VideoID = act.VideoID;

                activityViews.Add(actView);
            }
            return activityViews;
        }
    }
}
