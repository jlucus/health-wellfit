using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using WellFitPlus.Database.Entities;

namespace WellFitPlus.Database.Repositories {
    public class ActivityRepository : DataRepositoryBase {

        public static readonly ILog log = LogManager.GetLogger(typeof(ActivityRepository));
        private static readonly DateTime MIN_USABLE_DATABASE_DATE = new DateTime(1753, 1, 1);

        public void Add(Activity activity) {
            try {
          
                _context.Activities.Add(activity);
                _context.SaveChanges();

            } catch (Exception ex) {
                log.Error(ex);
            }
        }

        public int Delete(Guid id)
        {
            try
            {
                Activity activity = _context.Activities.Where(a => a.Id == id).FirstOrDefault();

                _context.Activities.Remove(activity);
                return _context.SaveChanges();

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return -1;
            }
        }

        public List<Activity> GetActivities()
        {
            List<Activity> actList = new List<Activity>();

            try
            {
                actList = _context.Activities.ToList();

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return actList;
        }

        public List<Activity> GetActivities(Guid userID) {
            List<Activity> actList = new List<Activity>();

            try {
                actList = _context.Activities.Where(a => a.UserID == userID).ToList();
            } catch (Exception ex) {
                log.Error(ex);
            }
            return actList;
        }

        public List<Activity> GetActivities(Guid userID, DateTime startDate, DateTime endTime) {
            List<Activity> actList = new List<Activity>();
            try {

                actList = _context.Activities.Where(a => a.UserID == userID &&
                                       a.StartTime >= startDate &&
                                       a.EndTime <= endTime).ToList();
            } catch (Exception ex) {
                log.Error(ex);
            }
            return actList;
        }

        /// <summary>
        /// Adds or updates each Activity in the database.
        /// 
        /// NOTE: As of now (4/10/17) the mobile devices do no store the ID's of activities. This means that the activities
        ///       uploaded by the phone cannot really be updated fully in the server database since we need to check to see if it
        ///       exists based upon mutable values. This is alright for now since the mobile devices NEVER re-use activities but needs
        ///       to be kept in mind if any new features are added.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="activities"></param>
        public void AddOrUpdateActivities(Guid userID, List<Activity> activities) {
            // Get all activities for the user
            List<Activity> allUserActivities = GetActivities(userID);
            try
            {
                //Add or update each activity
                foreach (var currActivity in activities)
                {

                    Activity test = allUserActivities.Where(a => a.Id == currActivity.Id).FirstOrDefault();

                    if (test != null)
                    {
                        // This record already exists in the database. Really the only thing we can update is whether it has a bonus
                        // attached. See the method note above for this reason.
                        test.Bonus = currActivity.Bonus;
                        _context.SaveChanges();
                    }
                    else
                    {
                        // This is a new activity so we need to add it
                        Add(currActivity);
                    }
                }

            }
            catch (Exception e) {
                log.Error(e);
            }

        }



    }
}
