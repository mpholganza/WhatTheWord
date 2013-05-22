using System;
using System.Diagnostics;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

#if DEBUG
using MockIAPLib;
using Store = MockIAPLib;
#else
using Windows.ApplicationModel.Store;
#endif

using WhatTheWord.Resources;
using System.Xml.Linq;
using System.Net;
using WhatTheWord.Model;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Windows.Storage;

namespace WhatTheWord
{
	public partial class App : Application
	{
		/// <summary>
		/// Provides easy access to the root frame of the Phone Application.
		/// </summary>
		/// <returns>The root frame of the Phone Application.</returns>
		public static PhoneApplicationFrame RootFrame { get; private set; }

		public GameConfig ConfigData { get; set; }
		public GameState StateData { get; set; }

		public DownloadManager Downloader { get; set; }

		public List<string> LocalFolderFiles { get; set; }

		public static string PuzzlePicturesPath = "Assets/PuzzlePictures/";

		public static new App Current
		{
			get { return Application.Current as App; }
		}

		/// <summary>
		/// Constructor for the Application object.
		/// </summary>
		public App()
		{
			// Global handler for uncaught exceptions.
			UnhandledException += Application_UnhandledException;

			// Standard XAML initialization
			InitializeComponent();

			// Phone-specific initialization
			InitializePhoneApplication();

			// Language display initialization
			InitializeLanguage();

			// Show graphics profiling information while debugging.
			// if (Debugger.IsAttached)
			// {
				// Display the current frame rate counters.
				// Application.Current.Host.Settings.EnableFrameRateCounter = true;

				// Show the areas of the app that are being redrawn in each frame.
				//Application.Current.Host.Settings.EnableRedrawRegions = true;

				// Enable non-production analysis visualization mode,
				// which shows areas of a page that are handed off to GPU with a colored overlay.
				//Application.Current.Host.Settings.EnableCacheVisualization = true;

				// Prevent the screen from turning off while under the debugger by disabling
				// the application's idle detection.
				// Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
				// and consume battery power when the user is not using the phone.
				// PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
			// }
			SetupMockIAP();

			// Set up DownloadManager
			Downloader = DownloadManager.GetInstance();
		}



		private void LoadGameConfigFromWeb()
		{
			//string udid = DeviceExtendedProperties.GetValue("DeviceUniqueId").ToString();
			//string flight = "test";
			//string password = "pjcfizp12fzviwx";
			//string appName = "com.kooapps.guessthisword";
			//string version = "1.0";
			//string hash = MD5(MD5(udid) + "com.kooapps.guessthisword" + flight + password + version + udid);

			// Download config info from web, if valid save to file
			//string uri = "http://www.google.com";
			Uri uri = new Uri("http://www.kooappsservers.com/kooappsFlights/getData.php?appName=com.kooapps.guessthisword&hash=a3819daec9ab5b3243abf7d8731e4d77&udid=testudid&version=1.0&flight=test&password=pjcfizp12fzviwx");
			//string uri = String.Format("http://www.kooappsservers.com/kooappsFlights/getData.php?appName={0}&hash={1}&udid={2}&version={3}&flight={4}&password={5}",
			//	appName, hash, udid, version, flight, password);

			WebClient client = new WebClient();
			client.DownloadStringCompleted += client_DownloadStringCompleted;
			client.DownloadStringAsync(uri);
		}

		void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs args)
		{
			if (!args.Cancelled && args.Error == null)
			{
				string gameData = args.Result;
				GameConfig tempGameConfig = new GameConfig();

				try
				{
					tempGameConfig.Deserialize(gameData); // test that the gamedata deserializes correctly
					GameConfig.Save(args.Result);
				}
				catch (ApplicationException)
				{
					// deserialized incorrectly. fail quietly
					// TODO: report to server of failed deserialization
					Console.WriteLine("Failed deserialization:\n" + gameData);
				}

			}
		}

		public void UpdatePictures(GameConfig tempGameConfig)
		{
			Dictionary<string, string> filesToDownload = new Dictionary<string, string>();
			foreach (KeyValuePair<int, Puzzle> puzzleKvp in tempGameConfig.Puzzles) {
				Puzzle puzzle = puzzleKvp.Value;
				filesToDownload.Add(puzzle.Picture1.Path, tempGameConfig.picturesFilenamePath + puzzle.Picture1.Path);
				filesToDownload.Add(puzzle.Picture2.Path, tempGameConfig.picturesFilenamePath + puzzle.Picture2.Path);
				filesToDownload.Add(puzzle.Picture3.Path, tempGameConfig.picturesFilenamePath + puzzle.Picture3.Path);
				filesToDownload.Add(puzzle.Picture4.Path, tempGameConfig.picturesFilenamePath + puzzle.Picture4.Path);
			}

			List<string> fileNamesToDownload = new List<string>();
			foreach (KeyValuePair<string, string> file in filesToDownload) { fileNamesToDownload.Add(file.Key); }

			foreach (string fileName in fileNamesToDownload)
			{
				string pictureFileName = fileName;
				pictureFileName = pictureFileName.Replace("zip", "jpg");
				if (LocalFolderFiles.Contains(pictureFileName) || FileAccess.ExistsInApplicationPackage(PuzzlePicturesPath + pictureFileName))
				{
					filesToDownload.Remove(pictureFileName.Replace("jpg", "zip"));
				}
			}

			Downloader.DownloadAndUnzipJpgFiles(filesToDownload);
		}

        private void SetupMockIAP()
        {
#if DEBUG
            MockIAP.Init();

            var sri = App.GetResourceStream(new Uri("MockupLicenseInfo.xml", UriKind.Relative));
            XDocument doc = XDocument.Load(sri.Stream);
            string xml = doc.ToString();

            MockIAP.RunInMockMode(true);
            MockIAP.SetListingInformation(1, "en-us", "In-App Purchase sample app", "Free", "In-App Purchase Test");

            MockIAP.PopulateIAPItemsFromXml(xml);

            MockIAP.ClearCache();
#endif
        }

		// Code to execute when the application is launching (eg, from Start)
		// This code will not execute when the application is reactivated
		private void Application_Launching(object sender, LaunchingEventArgs e)
		{
			LoadGameConfigFromWeb();
		}

		// Code to execute when the application is activated (brought to foreground)
		// This code will not execute when the application is first launched
		private void Application_Activated(object sender, ActivatedEventArgs e)
		{
			LoadGameConfigFromWeb();
		}

		// Code to execute when the application is deactivated (sent to background)
		// This code will not execute when the application is closing
		private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            Instrumentation.GetInstance().sendInstrumentation(
                "Heartbeat", "Exit", "deactivated", null, null);
		}

		// Code to execute when the application is closing (eg, user hit Back)
		// This code will not execute when the application is deactivated
		private void Application_Closing(object sender, ClosingEventArgs e)
        {
            Instrumentation.GetInstance().sendInstrumentation(
                "Heartbeat", "Exit", "closing", null, null);
		}

		// Code to execute if a navigation fails
		private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			if (Debugger.IsAttached)
			{
				// A navigation has failed; break into the debugger
				Debugger.Break();
			}
		}

		// Code to execute on Unhandled Exceptions
		private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
		{
			if (Debugger.IsAttached)
			{
				// An unhandled exception has occurred; break into the debugger
				Debugger.Break();
			}
		}

		#region Phone application initialization

		// Avoid double-initialization
		private bool phoneApplicationInitialized = false;

		// Do not add any additional code to this method
		private void InitializePhoneApplication()
		{
			if (phoneApplicationInitialized)
				return;

			// Create the frame but don't set it as RootVisual yet; this allows the splash
			// screen to remain active until the application is ready to render.
			RootFrame = new PhoneApplicationFrame();
			RootFrame.Navigated += CompleteInitializePhoneApplication;

			// Handle navigation failures
			RootFrame.NavigationFailed += RootFrame_NavigationFailed;

			// Handle reset requests for clearing the backstack
			RootFrame.Navigated += CheckForResetNavigation;

			// Ensure we don't initialize again
			phoneApplicationInitialized = true;
		}

		// Do not add any additional code to this method
		private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
		{
			// Set the root visual to allow the application to render
			if (RootVisual != RootFrame)
				RootVisual = RootFrame;

			// Remove this handler since it is no longer needed
			RootFrame.Navigated -= CompleteInitializePhoneApplication;
		}

		private void CheckForResetNavigation(object sender, NavigationEventArgs e)
		{
			// If the app has received a 'reset' navigation, then we need to check
			// on the next navigation to see if the page stack should be reset
			if (e.NavigationMode == NavigationMode.Reset)
				RootFrame.Navigated += ClearBackStackAfterReset;
		}

		private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
		{
			// Unregister the event so it doesn't get called again
			RootFrame.Navigated -= ClearBackStackAfterReset;

			// Only clear the stack for 'new' (forward) and 'refresh' navigations
			if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
				return;

			// For UI consistency, clear the entire page stack
			while (RootFrame.RemoveBackEntry() != null)
			{
				; // do nothing
			}
		}

		#endregion

		// Initialize the app's font and flow direction as defined in its localized resource strings.
		//
		// To ensure that the font of your application is aligned with its supported languages and that the
		// FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
		// and ResourceFlowDirection should be initialized in each resx file to match these values with that
		// file's culture. For example:
		//
		// AppResources.es-ES.resx
		//    ResourceLanguage's value should be "es-ES"
		//    ResourceFlowDirection's value should be "LeftToRight"
		//
		// AppResources.ar-SA.resx
		//     ResourceLanguage's value should be "ar-SA"
		//     ResourceFlowDirection's value should be "RightToLeft"
		//
		// For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
		//
		private void InitializeLanguage()
		{
			try
			{
				// Set the font to match the display language defined by the
				// ResourceLanguage resource string for each supported language.
				//
				// Fall back to the font of the neutral language if the Display
				// language of the phone is not supported.
				//
				// If a compiler error is hit then ResourceLanguage is missing from
				// the resource file.
				RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

				// Set the FlowDirection of all elements under the root frame based
				// on the ResourceFlowDirection resource string for each
				// supported language.
				//
				// If a compiler error is hit then ResourceFlowDirection is missing from
				// the resource file.
				FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
				RootFrame.FlowDirection = flow;
			}
			catch
			{
				// If an exception is caught here it is most likely due to either
				// ResourceLangauge not being correctly set to a supported language
				// code or ResourceFlowDirection is set to a value other than LeftToRight
				// or RightToLeft.

				if (Debugger.IsAttached)
				{
					Debugger.Break();
				}

				throw;
			}
		}
	}
}