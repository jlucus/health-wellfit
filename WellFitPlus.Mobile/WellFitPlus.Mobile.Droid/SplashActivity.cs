using Android.App;
using Android.Content;
using Android.OS;
using Android.Content.PM;
using Java.Lang;

namespace WellFitPlus.Mobile.Droid
{
    [Activity(Label = "WellFitPlus.Mobile", 
	          Icon = "@drawable/icon", 
	          MainLauncher = true, 
	          NoHistory = true,
	          Name = "com.asgrp.Mobile.WellFitPlus.SplashActivity", 
	          ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, 
	          Theme = "@android:style/Theme.NoTitleBar")]
    public class SplashActivity : Activity
    {
        private readonly int SPLASH_DISPLAY_LENGTH = 2000;

        private bool mHandlerFlag = true;

        private Handler mHandler = new Handler();
        private Runnable mRun;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_splash);

			mRun = new Runnable(() =>
			{
				if (mHandlerFlag)
				{
								// Finish the current Activity and start HomePage Activity
								Intent home = new Intent(this, typeof(MainActivity));
					StartActivity(home);
				}
			});


            if (Intent.Flags.HasFlag(ActivityFlags.BroughtToFront))
            {
                // Activity was brought to front and not created,
                // Thus finishing this will get us to the last viewed activity
                base.Finish();
            }

            

            /**
             * Run the code inside the runnable after the display time is finished
             * using a handler
             */
            mHandler.PostDelayed(mRun, SPLASH_DISPLAY_LENGTH);
        }

        public override void OnBackPressed()
        {
            /**
             * Disable the handler to execute when user presses back when in
             * SplashPage Activity
             */
            mHandlerFlag = false;
            base.OnBackPressed();
        }
    }
}