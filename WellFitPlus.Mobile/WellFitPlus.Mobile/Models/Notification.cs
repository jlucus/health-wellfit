using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

// This Notification Model is used only with Android.
namespace WellFitPlus.Mobile.Models
{
    public class Notification
    {
        public Guid UserID { get; set; }
        public Guid VideoID { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Icon { get; set; }        
        public PriorityType Priority { get; set; }
        public CategoryType Category { get; set; }
        public bool Vibrate { get; set; }
        public bool Sound { get; set; }
        public bool LargeIcon { get; set; }
        public bool Bonus { get; set; }
        public DateTime NotificationTime { get; set; }

        public enum PriorityType : int
        {
            Unknown = 0,
            High = 1,
            Low = 2,
            Maximum = 3,
            Minimum = 4,
            Default = 5
        }

        public enum CategoryType : int
        {
            Unknown = 0,
            CategoryCall = 1,
            CategoryMessage = 2,
            CategoryAlarm = 3,
            CategoryEmail = 4,
            CategoryEvent = 5,
            CategoryPromo = 6,
            CategoryProgress = 7,
            CategorySocial = 8,
            CategoryError = 9,
            CategoryTransport = 10,
            CategorySystem = 11,
            CategoryService = 12,
            CategoryRecommendation = 13,
            CategoryStatus = 14
        }        
    }
}
