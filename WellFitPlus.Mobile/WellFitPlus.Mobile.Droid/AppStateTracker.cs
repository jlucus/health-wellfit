using System;

using WellFitPlus.Mobile.Droid;

namespace WellFitPlus.Mobile.Droid
{

	public enum AppState { 
		Active,
		Background,
		Closed
	}

	/// <summary>
	/// Used to keep track of what state the MainActivity is in (i.e. Background/Closed state or Active state)
	/// 
	/// If the MainActivity is not in the foreground then it is considered to be in the Background/Closed state.
	/// This means that notification will go through and we will not prompt the user to watch a video within the 
	/// app.
	/// </summary>
	public class AppStateTracker
	{
		#region Static Fields
		private static AppStateTracker _instance;
		#endregion

		#region Static Properties
		public static AppStateTracker Instance { 
			get {
				if (_instance == null) {
					_instance = new AppStateTracker();
				}

				return _instance;
			}
		}
		#endregion

		private AppState _appState;
		public MainActivity _mainActivity; // Needed to check if the app is in the background/closed

		private AppStateTracker()
		{
			_appState = AppState.Active;
		}

		/// <summary>
		/// Sets the state of the app.
		/// 
		/// NOTE: 
		/// </summary>
		/// <param name="state">State.</param>
		/// <param name="mainActivity">The MainActivity of the app. This is needed to tell if the app is in a
		/// closed state or in a backgrounded/active state. null if in a closed state.
		/// 
		/// </param>
		public void SetAppState(AppState state, MainActivity mainActivity = null) {
			_appState = state;
			_mainActivity = mainActivity;
		}

		public AppState GetAppState() {

			// If there is no main activity then the app is closed.
			if (_mainActivity == null) {
				return AppState.Closed;
			}

			// Both the Background/Active states are handled by the _appState variable.
			return _appState;
		}

	}
}
