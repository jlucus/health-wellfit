using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

using System.Web.Mvc;
using System.Web;
using System.IO;
using System.ComponentModel;

using WellFitPlus.WebPortal.Models;
using WellFitPlus.WebPortal.Attributes;
using WellFitPlus.Database.Repositories;
using WellFitPlus.Database.Entities;

namespace WellFitPlus.WebPortal.Controllers
{
    [AuthorizeRoles("admin")]
    public class StatisticsController : Controller
    {

        [HttpGet]
        public ActionResult Index() {
            return View();
        }

        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Index(DateRangeViewModel model)
        {

            try
            {
                int dateTest = model.StartDate.CompareTo(model.EndDate);
                if (dateTest >= 0)
                {
                    // The start date is not earlier than the end date
                    ModelState.AddModelError("Date_Error", "The End Date must be later than the Start Date");
                }
                else if (ModelState.ContainsKey("Date_Error")) {
                    ModelState["Date_Error"].Errors.Clear();
                }


                if (ModelState.IsValid)
                {
                    

                    var activityRepo = new ActivityRepository();
                    var userRepo = new UserRepository();

                    var allDatabaseActivities = activityRepo.GetActivities();

                    // Get all User ids
                    var userIds = userRepo.GetAll().Select(u => u.Id).ToList();

                    var userStatistics = new List<UserStatisticsViewModel>();
                    
                    foreach (var userId in userIds)
                    {
                        // Filter out what data we need for each user using the date range inputted from the view.
                        // NOTE: Some missed videos will have no StartTime and all pure bonuses will always have a start time with 
                        //       no notificaiton time.
                        //       This means that we need to query both with the StartDate and Notification date to cover both scenarios
                        List<Activity> userActivities = allDatabaseActivities.Where(a => (a.UserID == userId)
                                                &&
                                                ((a.StartTime >= model.StartDate && a.StartTime <= model.EndDate)
                                                 || (a.NotificationTime >= model.StartDate && a.NotificationTime <= model.EndDate))).ToList();
                        UserProfile user = userRepo.Get(userId);

                        UserStatisticsViewModel userStats = new UserStatisticsViewModel();
                        userStats.UserId = user.Id;
                        userStats.FirstName = user.FirstName;
                        userStats.LastName = user.LastName;
                        userStats.Email = user.Email;
                        userStats.Activities = userActivities;

                        userStatistics.Add(userStats);
                    }

                    Response.ClearContent();
                    Response.AddHeader("content-disposition", "attachment;filename=Statistics.csv");
                    Response.AddHeader("Content-Type", "application/vnd.ms-excel");
                    WriteTsv(userStatistics, Response.Output, model.StartDate, model.EndDate);
                    Response.End();

                    return RedirectToRoute("/Statistics/Index");
                }

                return View(model);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);
                return View(model);
            }

        }

        public void WriteTsv(List<UserStatisticsViewModel> userStatistics, TextWriter output, DateTime startDate, DateTime endDate)
        {
            string dateRangeString = startDate.ToString("MM/dd/yyyy") + "-" + endDate.ToString("MM/dd/yyyy");

            // Write the first row of the spreadsheet i.e. the header row
            output.Write("DateRange: " + dateRangeString);
            output.Write(",");
            output.Write("Streak");
            output.Write(",");
            output.Write("Best Streak");
            output.Write(",");
            output.Write("# of scheduled sessions completed");
            output.Write(",");
            output.Write("# of scheduled session");
            output.Write(",");
            output.Write("% of scheduled sessions");
            output.Write(",");
            output.Write("# of bonus sessions completed");
            output.Write(",");
            output.Write("# of bonus & scheduled sessions completed");
            output.Write(",");
            output.Write("% of scheduled & bonus sessions completed");

            output.WriteLine(); // Set the spreadsheet to a new line

            userStatistics = userStatistics.OrderBy(u => u.FirstName).ToList();

            var companyRepo = new CompanyRepository();
            var userRepo = new UserRepository();
            var companies = companyRepo.GetAll().OrderBy(c => c.Name).ToList(); // TODO: Filter by company when company management is set up.

            // Get all the users for each company and print the entire company's statistics to the CSV file
            foreach (var company in companies) {
                List<Guid> usersIds = userRepo.GetByCompany(company.Id)
                    .Select(u => u.Id).ToList();

                List<UserStatisticsViewModel> companyUsers = userStatistics.Where(u => usersIds.Contains(u.UserId)).ToList();

                WriteCompanyUsersStatisticsToOutput(company.Name, companyUsers, output);
            }
            
        }

        private void WriteCompanyUsersStatisticsToOutput(string companyName, List<UserStatisticsViewModel> companyUsers, TextWriter output) {

            // Set averages variables
            float streakSum = 0;
            float bestStreakSum = 0;
            float scheduledSessionsCompletedSum = 0;
            float scheduledSessionsSum = 0;
            float percentScheduledSessionsCompletedSum = 0;
            float bonusesCompletedSum = 0;
            float bonusesPlusCompletedScheduledSessionsSum = 0;
            float percentScheduledPlusBonusSessionsCompletedSum = 0;

            output.Write(companyName);
            output.WriteLine();

            // Print each company user's statistics. Each iteration is one row in the exported CSV file.
            for (int i = 0; i < companyUsers.Count; i++)
            {
                UserStatisticsViewModel user = companyUsers[i];
                output.Write("Anonymous Employee #" + (i + 1));
                output.Write(",");
                output.Write(user.CurrentStreak);
                output.Write(",");
                output.Write(user.BestStreak);
                output.Write(",");
                output.Write(user.CompletedSessions.ToString());
                output.Write(",");
                output.Write(user.ScheduledSessions.ToString());
                output.Write(",");

                float percentScheduledComplete = ((float)user.CompletedSessions / (float)user.ScheduledSessions) * 100;
                
                output.Write(NormalizeDivisionOutput(percentScheduledComplete) + "%");

                output.Write(",");
                output.Write(user.BonusSessions.ToString());
                output.Write(",");
                output.Write(user.BonusesPlusScheduledSessionsCompleted.ToString());
                output.Write(",");

                float percentScheduledPlusBonusByTotalScheduled =
                    ((float)user.BonusesPlusScheduledSessionsCompleted / (float)user.ScheduledSessions) * 100;
                
                output.Write(NormalizeDivisionOutput(percentScheduledPlusBonusByTotalScheduled) + "%");
                output.Write(",");

                output.WriteLine(); // Set the spreadsheet to a new line.


                // Add to the sums for the averages calculations
                streakSum += user.CurrentStreak;
                bestStreakSum += user.BestStreak;
                scheduledSessionsCompletedSum += user.CompletedSessions;
                scheduledSessionsSum += user.ScheduledSessions;
                percentScheduledSessionsCompletedSum += percentScheduledComplete;
                bonusesCompletedSum += user.BonusSessions;
                bonusesPlusCompletedScheduledSessionsSum += user.BonusesPlusScheduledSessionsCompleted;
                percentScheduledPlusBonusSessionsCompletedSum += percentScheduledPlusBonusByTotalScheduled;
            }

            float totalUsers = companyUsers.Count();

            // Print "averages" row
            output.WriteLine();
            output.Write("Average");
            output.Write(",");
            output.Write(NormalizeDivisionOutput(streakSum / totalUsers));
            output.Write(",");
            output.Write(NormalizeDivisionOutput(bestStreakSum / totalUsers));
            output.Write(",");
            output.Write(NormalizeDivisionOutput(scheduledSessionsCompletedSum / totalUsers));
            output.Write(",");
            output.Write(NormalizeDivisionOutput(scheduledSessionsSum / totalUsers));
            output.Write(",");
            output.Write(NormalizeDivisionOutput(percentScheduledSessionsCompletedSum / totalUsers) + "%");
            output.Write(",");
            output.Write(NormalizeDivisionOutput(bonusesCompletedSum / totalUsers));
            output.Write(",");
            output.Write(NormalizeDivisionOutput(bonusesPlusCompletedScheduledSessionsSum / totalUsers));
            output.Write(",");
            output.Write(NormalizeDivisionOutput(percentScheduledPlusBonusSessionsCompletedSum / totalUsers) + "%");

            // Skip a couple rows to get ready for the next company.
            output.WriteLine();
            output.Write("============");
            output.WriteLine();
            output.Write(",");
            output.WriteLine();
            output.WriteLine();

        }


        /// <summary>
        /// After a float division calculation we need to check if we were dividing by zero or if some other erroneous output occured
        /// 
        /// This will check for the above situation and return "0" if one of those situations occured.
        /// </summary>
        /// <param name="outputCalculation"></param>
        /// <returns></returns>
        private string NormalizeDivisionOutput(float outputCalculation)
        {
            if (float.IsNaN(outputCalculation) == false && float.IsInfinity(outputCalculation) == false)
            {
                return outputCalculation.ToString("0.0");
            }

            return "0";
        }
    }
    
    public class UserStatistics {
        public string Active_Streak;
        public string Best_Streak;
        public string Weeks_Completed_Sessions;
        public string Weeks_Scheduled_Sessions;
        public string Weeks_Bonus_Sessions;
    }


}
