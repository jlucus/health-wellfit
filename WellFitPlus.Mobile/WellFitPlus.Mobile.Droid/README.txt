If you are working in Visual Studio please remove the reference to System.Configuration.dll within the .Net Assemblies as this dependency should be taken care of.

If you are working within Xamarin Studio Code on a Mac then you will need to add the System.Configuration.dll file to your local project packages folder and then reference it within the Droid project.

All of the above is due to somebody checking in something they weren’t supposed to because the System.Configuration.dll is actually not part of Mono, which is what Xamarin Studio uses. This is also only an issue on Android.