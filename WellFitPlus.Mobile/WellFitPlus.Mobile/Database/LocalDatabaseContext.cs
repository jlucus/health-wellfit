using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using WellFitMobile.FileSystem.Directory.Entities;
using WellFitMobile.FileSystem.File.Entities;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Services;
using Xamarin.Forms;
using System.Threading;

using SQLite;

namespace WellFitPlus.Mobile.Database
{
    public class LocalDatabaseContext 
    {
        #region Attributes
        private static Mutex _mutex = new Mutex();
		#endregion

		private SQLiteConnection db;

        #region Properties
       	public Mutex Lock
        {
            get { return _mutex;  }
        }

		public SQLiteConnection DB { 
			get {
				return db;
			}
		}

		//AppGlobals.Database.DatabasePath // Database path

        #endregion

        #region Constructor
        public LocalDatabaseContext() {
            try
            {
                db = new SQLiteConnection(AppGlobals.Database.DatabasePath);
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region Methods

        public void Initialize(bool drop = false) {

            // If Drop Tables
            if (drop)
            {
               	db.DropTable<Video>();
               	db.DropTable<ActivitySession>();
				db.DropTable<ScheduledNotification>();
				db.DropTable<FrequentVideo>();
            }

            // Execute "Create If Not Exists" For DB Tables
            db.CreateTable<Video>();
            db.CreateTable<ActivitySession>();
			db.CreateTable<ScheduledNotification>();
			db.CreateTable<FrequentVideo>();
        }

        public override string ToString() {
            return string.Format("Local Database:\r\n\tActivity Sessions: {0}\r\n\tVideos: {1}",
                db.Table<ActivitySession>().Count(),  db.Table<Video>().Count());
        }

        #endregion                   
    }
}
