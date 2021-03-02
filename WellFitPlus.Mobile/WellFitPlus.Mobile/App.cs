using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using WellFitMobile.FileSystem.Directory.Entities;
using WellFitMobile.FileSystem.File.Entities;
using WellFitPlus.Mobile.Database;
using WellFitPlus.Mobile.Database.Repositories;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.PlatformViews;
using WellFitPlus.Mobile.Services;
using WellFitPlus.Mobile.SharedViews;
using WellFitPlus.Mobile.Abstractions;
using Xamarin.Forms;

namespace WellFitPlus.Mobile
{
    public class App : Application
    {

		#region Static Fields
		public static bool UserIsSubscribed = false;
		public static double ScreenWidth;
		public static double ScreenHeight;

		private static bool _subscribedToVideo;
        private static bool _logFileInitialized;
        private static FileObject _logFile;
		private static bool _appHasInitialized = false;
		#endregion

		#region Static Properties

		public static string AppName
		{
			get
			{
				return "WellFitPlus";
			}
		}
		#endregion

        public App() {

            try
            {
                if (_appHasInitialized == false)
                {
                    _appHasInitialized = true;

					Login loginPage = new Login();
					NavigationPage.SetHasNavigationBar(loginPage, false);

                    // The root page of your application
                    MainPage = loginPage;

                    // make sure the local database exists
                    var db = new LocalDatabaseContext();
                    db.Initialize(false);// True if you would like to drop the tables

                    App.Log(string.Format("database initialized: {0}", db));
                }

            }
            catch (Exception ex)
            {
                App.Log("Critical Exception Thrown Loading Main App. Detail: " + ex.ToString());
            }
        }

        protected override void OnStart() {
			// If Have Not Subscribed To Video Page Event
			if (_subscribedToVideo == false)
            {
                _subscribedToVideo = true;

                MessagingCenter.Subscribe<string, int>(this, AppGlobals.Events.VIDEO_PAGE, (message, activityId) =>
                {
                    // Get Activity
                    var activity = ActivitySessionRepository.Instance.GetActivity(activityId);


                    // Invoke On Main UI Thread
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        // Create New Video Page And Navigate To It
						var video = new VideoPlaybackPage(activity);
                        NavigationPage.SetHasNavigationBar(video, false);
						try
                        {
                            // Try To Push Page To Top Of Navigation Stack
                            MainPage.Navigation.PushAsync(video, false);
                        }
                        catch (Exception ex)
                        {
                            // If Error, Reset Main Page - This Happens Sometimes With iOS
                            this.MainPage = new NavigationPage(video);
                        }
                    });
                });
            }
        }

        public static void Log(string message)
        {
            try
            {
                message = DateTime.Now.ToString("hh:mm:ss tt") + " - " + message;

                // Check File Initialized
                if (_logFile == null || _logFileInitialized == false)
                {
                    _logFileInitialized = true;
                    string path = Path.Combine(AppGlobals.Settings.MOBILE_DIRECTORY, "Log.txt");
                    _logFile = new FileObject(path);
                    if (_logFile.Exists == false)
                    {
                        var result = _logFile.Create("");
                        if (result == AppGlobals.ResultType.Failure) { return; }
                    }
                    else
                    {
                        _logFile.Write("");
                    }
                }
                var writeResult = _logFile.Append(message, true);
                string value = _logFile.Content.Value;

                Console.WriteLine(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("hh:mm:ss tt") + " - " + ex.ToString());
            }
        }
    }
}
