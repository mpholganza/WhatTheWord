using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WhatTheWord.Resources;
using WhatTheWord.Model;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using WhatTheWord.Popups;
using System.Windows.Threading;
using System.Windows.Media;

namespace WhatTheWord
{
	public partial class MainPage : PhoneApplicationPage
	{
		public Puzzle CurrentPuzzle { get; set; }
		FacebookUserControl facebookUserControl;
		public CoinsUserControl coinsUserControl;
        public BoostsUserControl boostsUserControl;
        public AboutUserControl aboutUserControl;
        public SettingsUserControl settingsUserControl;
		public NewPuzzlesUserControl newPuzzlesUserControl;
		public OutOfPuzzlesUserControl outOfPuzzlesUserControl;

        private DispatcherTimer boostBounceTimer;

		// Constructor
		public MainPage()
		{
			InitializeComponent();

			InitializeFacebookPopup();
			InitializeCoinsPopup();
			InitializeBoostsPopup();
			InitializeAboutPopup();
			InitializeSettingsPopup();
			InitializeBoostBounceTimer();
			InitializeNewPuzzlesPopup();
			InitializeOutOfPuzzlesPopup();

			ClearButton.Tap += ClearButton_Tap;
			ShuffleButton.Tap += ShuffleButton_Tap;
			CoinsButton.Tap += CoinsButton_Tap;
			CoinsStackPanel.Tap += CoinsButton_Tap;
			SettingsButton.Tap += SettingsButton_Tap;
			BoostButton.Tap += BoostsButton_Tap;
			FacebookButton.Tap += FacebookButton_Tap;

			if (!TryLoadCurrentPuzzle(App.Current.StateData.CurrentLevel))
			{
				outOfPuzzlesUserControl.show();
				return;
			}

			App.Current.StateData.InitializePuzzle(CurrentPuzzle);
			DisplayGame();
		}

		private void InitializeFacebookPopup()
		{
			facebookUserControl = new FacebookUserControl(new Popup(), this,
				Application.Current.Host.Content.ActualWidth, Application.Current.Host.Content.ActualHeight);
		}

		private void InitializeCoinsPopup()
		{
			coinsUserControl = new CoinsUserControl(new Popup(), this,
                Application.Current.Host.Content.ActualWidth, Application.Current.Host.Content.ActualHeight);
		}

		private void InitializeBoostsPopup()
		{
			boostsUserControl = new BoostsUserControl(new Popup(), this,
                Application.Current.Host.Content.ActualWidth, Application.Current.Host.Content.ActualHeight);
		}

        private void InitializeAboutPopup()
        {
            aboutUserControl = new AboutUserControl(new Popup(), this,
                Application.Current.Host.Content.ActualWidth, Application.Current.Host.Content.ActualHeight);
        }

        private void InitializeSettingsPopup()
        {
            settingsUserControl = new SettingsUserControl(new Popup(), this,
                Application.Current.Host.Content.ActualWidth, Application.Current.Host.Content.ActualHeight);
        }

		private void InitializeOutOfPuzzlesPopup()
		{
			outOfPuzzlesUserControl = new OutOfPuzzlesUserControl(new Popup(), this,
				Application.Current.Host.Content.ActualWidth, Application.Current.Host.Content.ActualHeight);
		}

		private void InitializeNewPuzzlesPopup()
		{
			newPuzzlesUserControl = new NewPuzzlesUserControl(new Popup(), this,
				Application.Current.Host.Content.ActualWidth, Application.Current.Host.Content.ActualHeight);
		}

        private void InitializeBoostBounceTimer()
        {
            boostBounceTimer = new System.Windows.Threading.DispatcherTimer();
            boostBounceTimer.Interval = new TimeSpan(0, 0, 0, 10, 0); // 10 seconds
            boostBounceTimer.Tick += new EventHandler(bounceBoostButton);
            boostBounceTimer.Start();
        }

        private void bounceBoostButton(object o, EventArgs sender)
        {
            BoostBounceStoryboard.Begin();
        }

		private bool TryLoadCurrentPuzzle(int currentLevel)
		{
			Puzzle puzzle;
			try
			{
				puzzle = App.Current.ConfigData.Puzzles[currentLevel];
				// TODO: Look for file in LocalFolder first

				// if not in LocalFolder look in pre-packaged Assets folder
				puzzle.Picture1.URI = "/Assets/PuzzlePictures/" + puzzle.Picture1.URI.Replace("zip", "jpg");
				puzzle.Picture2.URI = "/Assets/PuzzlePictures/" + puzzle.Picture2.URI.Replace("zip", "jpg");
				puzzle.Picture3.URI = "/Assets/PuzzlePictures/" + puzzle.Picture3.URI.Replace("zip", "jpg");
				puzzle.Picture4.URI = "/Assets/PuzzlePictures/" + puzzle.Picture4.URI.Replace("zip", "jpg");
			}
			catch
			{
				return false;
			}

			CurrentPuzzle = puzzle;
			return true;
			//CurrentPuzzle = new Puzzle()
			//{
			//	Word = "random".ToUpper(),
			//	Picture1 = new Picture { URI = "/Assets/PuzzlePictures/mangoes.png", Credits = "mph" },
			//	Picture2 = new Picture { URI = "/Assets/PuzzlePictures/icecream.png", Credits = "mph" },
			//	Picture3 = new Picture { URI = "/Assets/PuzzlePictures/water_houses.png", Credits = "mph" },
			//	Picture4 = new Picture { URI = "/Assets/PuzzlePictures/magnets.png", Credits = "mph" },
			//};
		}

		/// <summary>
		/// Display game
		/// </summary>
		public void DisplayGame()
		{
			LayoutRoot.DataContext = CurrentPuzzle;
			HeaderPanel.DataContext = App.Current.StateData;
			CoinsText.Text = App.Current.StateData.Coins.ToString();

			GuessPanel.Children.Clear();
			for (int i = 0; i < App.Current.StateData.GuessPanelState.Length; i++)
			{
				Image letterImage = new Image();
				letterImage.Width = 37.5;
				letterImage.Height = 37.5;
				switch (App.Current.StateData.GuessPanelState[i])
				{
					case GameState.GUESSPANEL_LETTER_NOT_GUESSED:
						letterImage.Source = new BitmapImage(new Uri("/Assets/GuessLetters/gb_clear@1280_768.png", UriKind.Relative));
						break;
					case GameState.GUESSPANEL_LETTER_REVEALED:
						// TODO: Style this differently
						string correctLetter = App.Current.StateData.PuzzleWord[i].ToString();
						letterImage.Source = new BitmapImage(new Uri("/Assets/GuessLetters/gb_" + correctLetter + "@1280_768.png", UriKind.Relative));
						break;
					default:
						string guessLetter = App.Current.StateData.PuzzleCharacters[App.Current.StateData.GuessPanelState[i]].ToString();
						letterImage.Source = new BitmapImage(new Uri("/Assets/GuessLetters/gb_" + guessLetter + "@1280_768.png", UriKind.Relative));
						break;
				}

				var x = i;
				letterImage.Tap += (sender, e) =>
				{
					GuessPanelLetterPressed(x);
				};

				GuessPanel.Children.Add(letterImage);
			}

			LetterPickerPanel1.Children.Clear(); LetterPickerPanel2.Children.Clear();
			for (int i = 0; i < App.Current.StateData.CharacterPanelState.Length; i++)
			{
				Image letterImage = new Image();
				// TODO: This style info should be in a resource file
				letterImage.Height = 56.25;
				letterImage.Width = 56.25;

				switch (App.Current.StateData.CharacterPanelState[i])
				{
					case GameState.CHARACTERPANEL_LETTER_REMOVED:
						letterImage.Source = new BitmapImage(new Uri("/Assets/sandBoxBG@1280_768.png", UriKind.Relative));
						break;
					case GameState.CHARACTERPANEL_LETTER_GUESSED:
						letterImage.Source = new BitmapImage(new Uri("/Assets/sandBoxBG@1280_768.png", UriKind.Relative));
						break;
					default:
						string letter = App.Current.StateData.PuzzleCharacters[i].ToString();
						letterImage.Source = new BitmapImage(new Uri("/Assets/SandboxLetters/sb_" + letter + "@1280_768.png", UriKind.Relative));
						break;
				}

				var x = i;
				letterImage.Tap += (sender, e) =>
				{
					CharacterPanelLetterPressed(x);
				};

				if (0 == i % 2)
				{
					LetterPickerPanel1.Children.Add(letterImage);
				}
				else
				{
					LetterPickerPanel2.Children.Add(letterImage);
				}
			}
		}

		private void ClearButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			App.Current.StateData.ClearGuessPanel();

			DisplayGame();
		}

		private void ShuffleButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			App.Current.StateData.JumblePuzzleCharacters();

			DisplayGame();
		}

		private void BoostsButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
            boostsUserControl.show();
		}

		private void CoinsButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			coinsUserControl.show();
		}

		private void FacebookButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			facebookUserControl.show();
		}

        private void SettingsButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            settingsUserControl.show();
        }

		private void GuessPanelLetterPressed(int guessPanelIndex)
		{
			int currentGuessPanelIndexValue = App.Current.StateData.GuessPanelState[guessPanelIndex];
			if (currentGuessPanelIndexValue != GameState.GUESSPANEL_LETTER_REVEALED && currentGuessPanelIndexValue != GameState.GUESSPANEL_LETTER_NOT_GUESSED)
			{
				// Return a letter back to the character panel
				App.Current.StateData.CharacterPanelState[currentGuessPanelIndexValue] = currentGuessPanelIndexValue;
				App.Current.StateData.GuessPanelState[guessPanelIndex] = GameState.GUESSPANEL_LETTER_NOT_GUESSED;
			}

			App.Current.StateData.Save();
			DisplayGame();
		}

		private void CharacterPanelLetterPressed(int characterPanelIndex)
		{
			int currentCharacterPanelIndexValue = App.Current.StateData.CharacterPanelState[characterPanelIndex];
			if (currentCharacterPanelIndexValue != GameState.CHARACTERPANEL_LETTER_GUESSED && currentCharacterPanelIndexValue != GameState.CHARACTERPANEL_LETTER_REMOVED)
			{
				// Place the chosen letter as a guess of the letter in the next available slot in the guess panel
				int nextFreeGuessPanelIndex = App.Current.StateData.GetNextFreeGuessPanelIndex();
				if (nextFreeGuessPanelIndex == -1) return;
				App.Current.StateData.GuessPanelState[nextFreeGuessPanelIndex] = currentCharacterPanelIndexValue;
				App.Current.StateData.CharacterPanelState[characterPanelIndex] = GameState.CHARACTERPANEL_LETTER_GUESSED;
			}

			if (-1 == App.Current.StateData.GetNextFreeGuessPanelIndex())
			{
				if (App.Current.StateData.CheckAnswer())
				{
					PuzzleComplete();
				}
				else
				{
					PuzzleIncorrect();
				}
			}

			App.Current.StateData.Save();
			DisplayGame();
		}

		private DispatcherTimer puzzleStatusTimer;
		private void PuzzleComplete()
		{
			PuzzleAttemptStatusBackground.Source = new BitmapImage(new Uri("/Assets/correctSlider@1280_768.png", UriKind.Relative));
			PuzzleAttemptStatusBackground.Visibility = Visibility.Visible;
			PuzzleAttemptStatus.Text = "CORRECT!";
			PuzzleAttemptStatus.Visibility = Visibility.Visible;
			GuessPanelGrid.Background.SetValue(ImageBrush.ImageSourceProperty, new BitmapImage(new Uri("/Assets/guessBGCorrect@1280_768.png", UriKind.Relative)));
			puzzleStatusTimer = new DispatcherTimer();
			puzzleStatusTimer.Interval = new TimeSpan(0, 0, 0, 1, 200);
			puzzleStatusTimer.Tick += puzzleStatusTimer_Correct;
			puzzleStatusTimer.Start();
		}

		private void puzzleStatusTimer_Correct(object sender, EventArgs e)
		{
			PuzzleAttemptStatus.Visibility = Visibility.Collapsed;
			PuzzleAttemptStatusBackground.Visibility = Visibility.Collapsed;
			GuessPanelGrid.Background.SetValue(ImageBrush.ImageSourceProperty, new BitmapImage(new Uri("/Assets/guessBG@1280_768.png", UriKind.Relative)));
			App.Current.StateData.CompleteLevel();
			NavigationService.Navigate(new Uri("/WinPage.xaml", UriKind.Relative));
			NavigationService.RemoveBackEntry();
			puzzleStatusTimer.Stop();
		}

		private void PuzzleIncorrect()
		{
			PuzzleAttemptStatusBackground.Source = new BitmapImage(new Uri("/Assets/tryAgainSlider@1280_768.png", UriKind.Relative));
			PuzzleAttemptStatusBackground.Visibility = Visibility.Visible;
			PuzzleAttemptStatus.Text = "TRY AGAIN!";
			PuzzleAttemptStatus.Visibility = Visibility.Visible;
			GuessPanelGrid.Background.SetValue(ImageBrush.ImageSourceProperty, new BitmapImage(new Uri("/Assets/guessBGTryAgain@1280_768.png", UriKind.Relative)));
			puzzleStatusTimer = new DispatcherTimer();
			puzzleStatusTimer.Interval = new TimeSpan(0, 0, 0, 1, 200);
			puzzleStatusTimer.Tick += puzzleStatusTimer_Incorrect;
			puzzleStatusTimer.Start();
		}

		private void puzzleStatusTimer_Incorrect(object sender, EventArgs e)
		{
			PuzzleAttemptStatusBackground.Visibility = Visibility.Collapsed;
			PuzzleAttemptStatus.Visibility = Visibility.Collapsed;
			GuessPanelGrid.Background.SetValue(ImageBrush.ImageSourceProperty, new BitmapImage(new Uri("/Assets/guessBG@1280_768.png", UriKind.Relative)));
			puzzleStatusTimer.Stop();
		}

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            bool cancelBackbutton = true;

            if (facebookUserControl.isOpen())
            {
                facebookUserControl.hide();
            }
            else if (boostsUserControl.isOpen())
            {
                boostsUserControl.hide();
            }
            else if (coinsUserControl.isOpen())
            {
                coinsUserControl.hide();
            }
            else if (aboutUserControl.isOpen())
            {
                aboutUserControl.hide();
            }
            else if (settingsUserControl.isOpen())
            {
                settingsUserControl.hide();
            }
            else
            {
                cancelBackbutton = false;
            }

            if (cancelBackbutton)
            {
                e.Cancel = true;  //Cancels the default behavior.
            }
        }

		// Sample code for building a localized ApplicationBar
		//private void BuildLocalizedApplicationBar()
		//{
		//    // Set the page's ApplicationBar to a new instance of ApplicationBar.
		//    ApplicationBar = new ApplicationBar();

		//    // Create a new button and set the text value to the localized string from AppResources.
		//    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
		//    appBarButton.Text = AppResources.AppBarButtonText;
		//    ApplicationBar.Buttons.Add(appBarButton);

		//    // Create a new menu item with the localized string from AppResources.
		//    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
		//    ApplicationBar.MenuItems.Add(appBarMenuItem);
		//}
	}
}