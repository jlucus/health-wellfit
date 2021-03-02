using System;
using System.Collections.Generic;
using System.Linq;
using WellFitPlus.Mobile.Models;

using SQLitePCL;

namespace WellFitPlus.Mobile.Database.Repositories
{
    public class ActivitySessionRepository
    {

		#region Static Fields
		private static ActivitySessionRepository _instance;
		#endregion

		#region Static Properties
		public static ActivitySessionRepository Instance {

			get {
				if (_instance == null) {
					_instance = new ActivitySessionRepository();
				}
				return _instance;
			}
		}
		#endregion

        #region Private Fields
		private LocalDatabaseContext _context;

        #endregion

        #region Initialization

        private ActivitySessionRepository()
        {
			_context = new LocalDatabaseContext();
        }

        #endregion

        #region Methods

        public int AddActivity(ActivitySession session)
        {
            int result = -1;

            try
            {
                _context.Lock.WaitOne();

                result = _context.DB.Insert(session);
                if (result <= 0)
                {
                    App.Log("Error: Add Activity " + session.Id + " Failed");
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _context.Lock.ReleaseMutex();
            }

            return result;
        }

        public int GetCompletedActivities(DateTime date)
        {
            int count = -1;
            try
            {
                _context.Lock.WaitOne();

                if (_context.DB.Table<ActivitySession>().Count() == 0) { return 0; }
                count = _context.DB.Table<ActivitySession>()
                    .Count(s => s.EndTime.Date.Equals(date.Date));

                _context.Lock.ReleaseMutex();
                return count;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _context.Lock.ReleaseMutex();
            }

            return count;
        }

        public List<ActivitySession> GetActivities(DateTime notifyDate)
        {   
            List<ActivitySession> result = null;
            
            try
            {
                _context.Lock.WaitOne();
                result = _context.DB.Table<ActivitySession>()
                     .Where(s => s.NotificationTime >= notifyDate)
                     .OrderByDescending(v => v.NotificationTime)
                     .ToList();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _context.Lock.ReleaseMutex();
            }

            return result;
        }

		public List<ActivitySession> GetActivities() { 
			List<ActivitySession> result = null;
            
            try
            {
                _context.Lock.WaitOne();
                result = _context.DB.Table<ActivitySession>()
                     .OrderByDescending(v => v.NotificationTime)
                     .ToList();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _context.Lock.ReleaseMutex();
            }

            return result;
		}

        public List<ActivitySession> GetActivitiesNotUploaded()
        {
            List<ActivitySession> activities = null;
            try
            {
                _context.Lock.WaitOne();
                activities = _context.DB.Table<ActivitySession>()
                    .Where(s => s.HasBeenUploaded == false)
                    .OrderByDescending(v => v.NotificationTime)
                    .ToList();                
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _context.Lock.ReleaseMutex();
            }
            return activities;
        }

        public ActivitySession GetActivity(int activityId)
        {
            ActivitySession activity = null;
            try
            {
                _context.Lock.WaitOne();
                activity = _context.DB.Table<ActivitySession>()
                    .Where(s => s.Id == activityId).FirstOrDefault();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _context.Lock.ReleaseMutex();
            }

            return activity;
        }

        /// <summary>
        /// Gets all activities that were scheduled and not "pure" bonuses. This includes
        /// all pending and all finalized scheduled activities.
        /// </summary>
        /// <returns>The scheduled activities.</returns>
        public List<ActivitySession> GetAllScheduledActivities() {
            List<ActivitySession> result = null;

            try {
                _context.Lock.WaitOne();
                result = _context.DB.Table<ActivitySession>()
                                 .Where(a => a.NotificationTime != DateTime.MinValue)
                                 .OrderByDescending(a => a.NotificationTime)
                                 .ToList();
            } catch (Exception e) {
                return null; // Something weird happened
            } finally {
                _context.Lock.ReleaseMutex();
            }

            return result;
        }

        public int UpdateActivity(ActivitySession session)
        {
            int result = -1;
            try
            {
                _context.Lock.WaitOne();
                result = _context.DB.Update(session);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _context.Lock.ReleaseMutex();
            }
            return result;
        }

		public void RemoveActivity(ActivitySession session) { 
			_context.Lock.WaitOne();
			_context.DB.Delete(session);
		}

        #endregion
    }
}
