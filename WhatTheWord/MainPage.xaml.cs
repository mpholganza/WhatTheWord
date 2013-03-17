﻿using System;
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

namespace WhatTheWord
{
	public partial class MainPage : PhoneApplicationPage
	{
		public Puzzle CurrentPuzzle { get; set; }
		public GameState CurrentGameState { get; set; }

        Popup _CoinsPopup = new Popup();
        Popup _FacebookPopup = new Popup();
        Popup _BoostPopup = new Popup();

        FacebookUserControl facebookUserControl;
        CoinsUserControl coinsUserControl;

		// Constructor
		public MainPage()
		{
            InitializeComponent();

			// Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();

            Loaded += MainPage_Loaded;

		}

        private void InitializeFacebookPopup()
        {
            double hostWidth = Application.Current.Host.Content.ActualWidth;
            double hostHeight = Application.Current.Host.Content.ActualHeight;

            facebookUserControl = new FacebookUserControl(_FacebookPopup, this.LayoutRoot, hostWidth, hostHeight);
        }

        private void InitializeCoinsPopup()
        {
            double hostWidth = Application.Current.Host.Content.ActualWidth;
            double hostHeight = Application.Current.Host.Content.ActualHeight;

            coinsUserControl = new CoinsUserControl(_CoinsPopup, CurrentGameState, hostWidth, hostHeight);
        }

		void MainPage_Loaded(object sender, RoutedEventArgs e)
		{
			LoadGameState();

            DisplayGame();

            InitializeFacebookPopup();
            InitializeCoinsPopup();

			ClearButton.Tap += ClearButton_Tap;
            ShuffleButton.Tap += ShuffleButton_Tap;
            CoinsButton.Tap += CoinsButton_Tap;
            FacebookButton.Tap += FacebookButton_Tap;
		}

		/// <summary>
		/// Load game state from file
		/// </summary>
		private void LoadGameState()
		{
			CurrentGameState = new GameState();
			CurrentGameState.LoadGameStateFromFile();

			CurrentPuzzle = LoadCurrentPuzzle(CurrentGameState.CurrentLevel);

			CurrentGameState.Initialize(CurrentPuzzle);
		}

		private Puzzle LoadCurrentPuzzle(int currentLevel)
		{
			return new Puzzle()
			{
				Word = "randomus".ToUpper(),
				Picture1 = new Picture { URI = "/Assets/PuzzlePictures/mangoes.png", Credits = "mph" },
				Picture2 = new Picture { URI = "/Assets/PuzzlePictures/icecream.png", Credits = "mph" },
				Picture3 = new Picture { URI = "/Assets/PuzzlePictures/water_houses.png", Credits = "mph" },
				Picture4 = new Picture { URI = "/Assets/PuzzlePictures/magnets.png", Credits = "mph" },
			};
		}

		/// <summary>
		/// Display game
		/// </summary>
		private void DisplayGame()
		{
			LayoutRoot.DataContext = CurrentPuzzle;
			HeaderPanel.DataContext = CurrentGameState;

			GuessPanel.Children.Clear();
			for (int i = 0; i < CurrentGameState.GuessPanelState.Length; i++)
			{
				Image letterImage = new Image();
				letterImage.Width = 36;
				letterImage.Height = 36;
				switch (CurrentGameState.GuessPanelState[i]) {
					case GameState.GUESSPANEL_LETTER_NOT_GUESSED:
						letterImage.Source = new BitmapImage(new Uri("/Assets/GuessLetters/gb_clear@1280_768.png", UriKind.Relative));
						break;
					case GameState.GUESSPANEL_LETTER_REVEALED:
						// TODO: Style this differently
						string correctLetter = CurrentGameState.PuzzleWord[i].ToString();
						letterImage.Source = new BitmapImage(new Uri("/Assets/GuessLetters/gb_" + correctLetter + "@1280_768.png", UriKind.Relative));
						break;
					default:
						string guessLetter = CurrentGameState.PuzzleCharacters[CurrentGameState.GuessPanelState[i]].ToString();
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
			for (int i = 0; i < CurrentGameState.CharacterPanelState.Length; i++)
			{
				Image letterImage = new Image();
				// TODO: This style info should be in a resource file
				letterImage.Height = 48;
				letterImage.Width = 48;

				switch (CurrentGameState.CharacterPanelState[i])
				{
					case GameState.CHARACTERPANEL_LETTER_REMOVED:
						letterImage.Source = new BitmapImage(new Uri("/Assets/sandBoxBG@1280_768.png", UriKind.Relative));
						break;
					case GameState.CHARACTERPANEL_LETTER_GUESSED:
						letterImage.Source = new BitmapImage(new Uri("/Assets/sandBoxBG@1280_768.png", UriKind.Relative));
						break;
					default:
						string letter = CurrentGameState.PuzzleCharacters[i].ToString();
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
			CurrentGameState.ClearGuessPanel();

			DisplayGame();
		}

		private void ShuffleButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			CurrentGameState.JumblePuzzleCharacters();

			DisplayGame();
		}

        private void CoinsButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            coinsUserControl.show();
        }

        private void FacebookButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            facebookUserControl.show();
        }

		private void GuessPanelLetterPressed(int guessPanelIndex)
		{
			int currentGuessPanelIndexValue = CurrentGameState.GuessPanelState[guessPanelIndex];
			if (currentGuessPanelIndexValue != GameState.GUESSPANEL_LETTER_REVEALED && currentGuessPanelIndexValue != GameState.GUESSPANEL_LETTER_NOT_GUESSED)
			{
				// Return a letter back to the character panel
				CurrentGameState.CharacterPanelState[currentGuessPanelIndexValue] = currentGuessPanelIndexValue;
				CurrentGameState.GuessPanelState[guessPanelIndex] = GameState.GUESSPANEL_LETTER_NOT_GUESSED;
			}

			DisplayGame();
		}

		private void CharacterPanelLetterPressed(int characterPanelIndex)
		{
			int currentCharacterPanelIndexValue = CurrentGameState.CharacterPanelState[characterPanelIndex];
			if (currentCharacterPanelIndexValue != GameState.CHARACTERPANEL_LETTER_GUESSED && currentCharacterPanelIndexValue != GameState.CHARACTERPANEL_LETTER_REMOVED)
			{
				// Place the chosen letter as a guess of the letter in the next available slot in the guess panel
				int nextFreeGuessPanelIndex = CurrentGameState.GetNextFreeGuessPanelIndex();
				if (nextFreeGuessPanelIndex == -1) return;
				CurrentGameState.GuessPanelState[nextFreeGuessPanelIndex] = currentCharacterPanelIndexValue;
				CurrentGameState.CharacterPanelState[characterPanelIndex] = GameState.CHARACTERPANEL_LETTER_GUESSED;
			}

			if (-1 == CurrentGameState.GetNextFreeGuessPanelIndex())
			{
				if (CurrentGameState.CheckAnswer())
				{
					PuzzleComplete();
					return;
				}
				else
				{
					PuzzleIncorrect();
				}
			}

			DisplayGame();
		}

		private void PuzzleComplete()
		{
			CurrentGameState.CompleteLevel();
			NavigationService.Navigate(new Uri("/WinPage.xaml", UriKind.Relative));
		}

		private void PuzzleIncorrect()
		{
			// display incorrect guess
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