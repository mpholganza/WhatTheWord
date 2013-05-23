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
using WhatTheWord.Controls;
using Windows.Storage;
using System.Windows.Media.Animation;
using System.IO;
using System.Threading.Tasks;

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
		public ZoomedPictureUserControl zoomedPictureUserControl;
        public ResetGameConfirmationUserControl resetGameConfirmationUserControl;
        public UserReviewUserControl userReviewUserControl;

        private DispatcherTimer boostBounceTimer;

		// Constructor
		public MainPage()
		{
			InitializeComponent();

            SoundEffects.Initialize();
            InitializeBoostBounceTimer();

			InitializeFacebookPopup();
			InitializeCoinsPopup();
			InitializeBoostsPopup();
			InitializeAboutPopup();
			InitializeSettingsPopup();
			InitializeNewPuzzlesPopup();
			InitializeOutOfPuzzlesPopup();
			InitializeZoomedPicturePopup();
			InitializeResetGameConfirmationPopup();
            InitializeUserReviewPopup();

			ClearButton.Tap += ClearButton_Tap;
			ShuffleButton.Tap += ShuffleButton_Tap;
			CoinsButton.Tap += CoinsButton_Tap;
			CoinsStackPanel.Tap += CoinsButton_Tap;
			CoinsDollarSign.Tap += CoinsButton_Tap;
			CoinsText.Tap += CoinsButton_Tap;
			SettingsButton.Tap += SettingsButton_Tap;
			BoostButton.Tap += BoostsButton_Tap;
			FacebookButton.Tap += FacebookButton_Tap;

			if (App.Current.StateData.CurrentLevel > App.Current.ConfigData.Puzzles.Count)
			{
				if (App.Current.Downloader.InProgress)
                {
                    Instrumentation.GetInstance().sendInstrumentation(
                        "Puzzle", "AllComplete", "download-more-puzzles", null, null);

					newPuzzlesUserControl.show();
				}
				else
                {
                    Instrumentation.GetInstance().sendInstrumentation(
                        "Puzzle", "AllComplete", "no-more-puzzles", null, null);

					outOfPuzzlesUserControl.show();
				}
				return;
			}

			CurrentPuzzle = App.Current.ConfigData.Puzzles[App.Current.StateData.CurrentLevel];
			if (!CurrentPuzzle.TryLoad())
			{
				if (App.Current.Downloader.InProgress)
				{
					newPuzzlesUserControl.show();
				}
				return;
			}

			Picture1.Source = CurrentPuzzle.Picture1.ImageSource;
			Picture2.Source = CurrentPuzzle.Picture2.ImageSource;
			Picture3.Source = CurrentPuzzle.Picture3.ImageSource;
			Picture4.Source = CurrentPuzzle.Picture4.ImageSource;

			Picture1.Tap += (sender, e) =>
			{
				WhatTheWord.Controls.SoundEffects.PlayClick();
				zoomedPictureUserControl.show((Image)sender);
			};

			Picture2.Tap += (sender, e) =>
			{
				WhatTheWord.Controls.SoundEffects.PlayClick();
				zoomedPictureUserControl.show((Image)sender);
			};

			Picture3.Tap += (sender, e) =>
			{
				WhatTheWord.Controls.SoundEffects.PlayClick();
				zoomedPictureUserControl.show((Image)sender);
			};

			Picture4.Tap += (sender, e) =>
			{
				WhatTheWord.Controls.SoundEffects.PlayClick();
				zoomedPictureUserControl.show((Image)sender);
			};

			if (App.Current.StateData.CurrentLevel == 1)
			{
				InitializeTutorialPuzzle();
			}

			App.Current.StateData.InitializePuzzle(CurrentPuzzle);
			DisplayGame();
		}

		private void InitializeTutorialPuzzle()
		{
			HeaderPanel.Visibility = Visibility.Collapsed;
            TutorialOverlay.Visibility = Visibility.Visible;
            TutorialInstruction.Visibility = Visibility.Visible;
			GuessPanelTutorial1.Visibility = Visibility.Visible;
			GuessPanelTutorial2.Visibility = Visibility.Visible;
			ShuffleButton.Visibility = Visibility.Collapsed;
			ClearButton.Visibility = Visibility.Collapsed;
			FacebookButton.Visibility = Visibility.Collapsed;
			BoostButton.Visibility = Visibility.Collapsed;

			LetterPickerPanel1.HorizontalAlignment = HorizontalAlignment.Center;
			LetterPickerPanel2.HorizontalAlignment = HorizontalAlignment.Center;

			App.Current.StateData.PuzzleWord = CurrentPuzzle.Word;
			App.Current.StateData.GuessPanelState = new int[App.Current.StateData.PuzzleWord.Length];
			for (int i = 0; i < App.Current.StateData.PuzzleWord.Length; i++)
			{
				App.Current.StateData.GuessPanelState[i] = GameState.GUESSPANEL_LETTER_NOT_GUESSED;
			}

			App.Current.StateData.PuzzleCharacters = "PYLPALYAPYLP";
			App.Current.StateData.CharacterPanelState = new int[App.Current.StateData.PuzzleCharacters.Length];
			for (int i = 0; i < App.Current.StateData.PuzzleCharacters.Length; i++)
			{
				App.Current.StateData.CharacterPanelState[i] = i;
			}
			App.Current.StateData.PuzzleInitialized = true;
		}

		private void InitializeFacebookPopup()
		{
			facebookUserControl = new FacebookUserControl(new Popup(), this,
				Application.Current.Host.Content.ActualWidth, Application.Current.Host.Content.ActualHeight);

            setupLetterImageAnimation(FacebookButton);
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

            setupLetterImageAnimation(BoostButton);
		}

        private void InitializeAboutPopup()
        {
            aboutUserControl = new AboutUserControl(new Popup(), this,
                Application.Current.Host.Content.ActualWidth, Application.Current.Host.Content.ActualHeight);
        }

        private void InitializeUserReviewPopup()
        {
            userReviewUserControl = new UserReviewUserControl(new Popup(), this,
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

		private void InitializeZoomedPicturePopup()
		{
			zoomedPictureUserControl = new ZoomedPictureUserControl(new Popup(), this,
				Application.Current.Host.Content.ActualWidth, Application.Current.Host.Content.ActualHeight);
		}

		private void InitializeResetGameConfirmationPopup()
		{
			resetGameConfirmationUserControl = new ResetGameConfirmationUserControl(new Popup(), this,
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
            var currentPage = ((PhoneApplicationFrame)Application.Current.RootVisual).Content;

            if (currentPage == this && !AreTherePopupsOpened())
            {
                BoostBounceStoryboard.Begin();
                SoundEffects.PlayBounce();
            }

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
					SoundEffects.PlayTapLetter();
					GuessPanelLetterPressed(x);
				};

                setupLetterImageAnimation(letterImage);

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
                    SoundEffects.PlayTapLetter();
					CharacterPanelLetterPressed(x);
				};

                setupLetterImageAnimation(letterImage);

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

        private void setupLetterImageAnimation(Image letterImage)
        {
            letterImage.RenderTransformOrigin = new Point(0.5, 0.5);

            CompositeTransform compositeTransform = new CompositeTransform();
            compositeTransform.ScaleX = 1;
            compositeTransform.ScaleY = 1;
            letterImage.RenderTransform = compositeTransform;

            Storyboard mouseEnterStoryboard = createLetterImageMouseEnterStoryboard();
            foreach (Timeline timeline in mouseEnterStoryboard.Children)
            {
                Storyboard.SetTarget(timeline, letterImage);
            }
            letterImage.MouseEnter += (sender, e) =>
            {
                mouseEnterStoryboard.Begin();
            };

            Storyboard mouseLeaveStoryboard = createLetterImageMouseLeaveStoryboard();
            foreach (Timeline timeline in mouseLeaveStoryboard.Children)
            {
                Storyboard.SetTarget(timeline, letterImage);
            }
            letterImage.MouseLeave += (sender, e) =>
            {
                mouseLeaveStoryboard.Begin();
            };

        }

        private Storyboard createLetterImageMouseEnterStoryboard()
        {
            DoubleAnimation scaleXAnimation = new DoubleAnimation();
            scaleXAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(50));
            //scaleXAnimation.From = 1;
            scaleXAnimation.To = 1.25;
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleX)"));

            DoubleAnimation scaleYAnimation = new DoubleAnimation();
            scaleYAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(50));
            //scaleYAnimation.From = 0;
            scaleYAnimation.To = 1.25;
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleY)"));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(scaleXAnimation);
            storyboard.Children.Add(scaleYAnimation);

            return storyboard;
        }

        private Storyboard createLetterImageMouseLeaveStoryboard()
        {
            DoubleAnimation scaleXAnimation = new DoubleAnimation();
            scaleXAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(20));
            //scaleXAnimation.From = 0;
            scaleXAnimation.To = 1;
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleX)"));

            DoubleAnimation scaleYAnimation = new DoubleAnimation();
            scaleYAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(20));
            //scaleYAnimation.From = 0;
            scaleYAnimation.To = 1;
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleY)"));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(scaleXAnimation);
            storyboard.Children.Add(scaleYAnimation);

            return storyboard;
        }

		private void ClearButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			App.Current.StateData.ClearGuessPanel();

            DisplayGame();
            SoundEffects.PlayClick();
		}

		private void ShuffleButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			App.Current.StateData.JumblePuzzleCharacters();

            DisplayGame();
            SoundEffects.PlayClick();
		}

		private void BoostsButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
            boostsUserControl.show();
            SoundEffects.PlayClick();
		}

		private void CoinsButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
            coinsUserControl.show();
            SoundEffects.PlayClick();
		}

		private void FacebookButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
            facebookUserControl.show();
            SoundEffects.PlayClick();
		}

        private void SettingsButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            settingsUserControl.show();
            SoundEffects.PlayClick();
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
		public void PuzzleComplete()
        {
            Instrumentation.GetInstance().sendInstrumentation(
                "Puzzle", "StageComplete", null, null, App.Current.ConfigData.rewardCoinsPerQuestion.ToString());

            SoundEffects.PlayWin();
            TutorialInstruction.Visibility = System.Windows.Visibility.Collapsed;
            PuzzleAttemptStatusBackground.Source = new BitmapImage(new Uri("/Assets/correctSlider@1280_768.png", UriKind.Relative));
			PuzzleAttemptStatusBackground.Visibility = Visibility.Visible;
			Overlay.Visibility = Visibility.Visible; // Disallow user input
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
			App.Current.StateData.CompleteLevel();
			NavigationService.Navigate(new Uri("/WinPage.xaml", UriKind.Relative));
			NavigationService.RemoveBackEntry();
            puzzleStatusTimer.Stop();
            TutorialOverlay.Visibility = Visibility.Collapsed;
            PuzzleAttemptStatus.Visibility = Visibility.Collapsed;
            PuzzleAttemptStatusBackground.Visibility = Visibility.Collapsed;
            Overlay.Visibility = Visibility.Collapsed; // Re-allow user input
		}

		private void PuzzleIncorrect()
        {
            SoundEffects.PlayWrong();
			PuzzleAttemptStatusBackground.Source = new BitmapImage(new Uri("/Assets/tryAgainSlider@1280_768.png", UriKind.Relative));
			PuzzleAttemptStatusBackground.Visibility = Visibility.Visible;
			Overlay.Visibility = Visibility.Visible;
            //TutorialOverlay.Visibility = Visibility.Collapsed;
            TutorialInstruction.Visibility = Visibility.Collapsed;
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
			Overlay.Visibility = Visibility.Collapsed;

			if (App.Current.StateData.CurrentLevel == 1)
			{
                TutorialOverlay.Visibility = Visibility.Visible;
                TutorialInstruction.Visibility = Visibility.Visible;
			}

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
			else if (zoomedPictureUserControl.isOpen())
			{
				zoomedPictureUserControl.hide();
            }
            else if (userReviewUserControl.isOpen())
            {
                userReviewUserControl.hide();
            }
            else if (resetGameConfirmationUserControl.isOpen())
            {
                resetGameConfirmationUserControl.hide();
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

        private bool AreTherePopupsOpened()
        {
            return facebookUserControl.isOpen()
                || boostsUserControl.isOpen()
                || coinsUserControl.isOpen()
                || settingsUserControl.isOpen()
                || aboutUserControl.isOpen()
                || newPuzzlesUserControl.isOpen()
                || outOfPuzzlesUserControl.isOpen()
                || resetGameConfirmationUserControl.isOpen()
                || zoomedPictureUserControl.isOpen()
                || userReviewUserControl.isOpen()
                || resetGameConfirmationUserControl.isOpen();
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