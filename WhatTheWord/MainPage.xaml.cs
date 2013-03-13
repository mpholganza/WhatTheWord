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

namespace WhatTheWord
{
	public partial class MainPage : PhoneApplicationPage
	{
		public Puzzle CurrentPuzzle { get; set; }
		public GameState CurrentGameState { get; set; }
		// Constructor
		public MainPage()
		{
			InitializeComponent();

			// Sample code to localize the ApplicationBar
			//BuildLocalizedApplicationBar();

			Loaded += MainPage_Loaded;
		}

		void MainPage_Loaded(object sender, RoutedEventArgs e)
		{
			LoadGameState();

			DisplayGame();
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
			return new Puzzle("randomus")
			{
				Picture1 = new Picture { URI = "/Assets/PuzzlePictures/mangoes.jpg", Credits = "mph" },
				Picture2 = new Picture { URI = "/Assets/PuzzlePictures/icecream.jpg", Credits = "mph" },
				Picture3 = new Picture { URI = "/Assets/PuzzlePictures/water_houses.jpg", Credits = "mph" },
				Picture4 = new Picture { URI = "/Assets/PuzzlePictures/magnets.jpg", Credits = "mph" },
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
				TextBlock guessText = new TextBlock();
				guessText.Width = 48;
				guessText.Height = 48;
				switch (CurrentGameState.GuessPanelState[i]) {
					case GameState.GUESSPANEL_LETTER_NOT_GUESSED:
						guessText.Text = "_";
						break;
					case GameState.GUESSPANEL_LETTER_REVEALED:
						// TODO: Style this differently
						guessText.Text = CurrentGameState.PuzzleWord[i].ToString();
						break;
					default:
						guessText.Text = CurrentGameState.PuzzleCharacters[CurrentGameState.GuessPanelState[i]].ToString();
						break;
				}

				var x = i;
				guessText.Tap += (sender, e) =>
				{
					GuessPanelLetterPressed(x);
				};

				GuessPanel.Children.Add(guessText);
			}

			LetterPickerPanel1.Children.Clear(); LetterPickerPanel2.Children.Clear();
			for (int i = 0; i < CurrentGameState.CharacterPanelState.Length; i++)
			{
				TextBlock text = new TextBlock();
				// TODO: This style info should be in a resource file
				text.Height = 48;
				text.Width = 48;
				text.Style = (Style)Application.Current.Resources["Character"];

				switch (CurrentGameState.CharacterPanelState[i])
				{
					case GameState.CHARACTERPANEL_LETTER_REMOVED:
						text.Text = "*";
						break;
					case GameState.CHARACTERPANEL_LETTER_GUESSED:
						text.Text = "_";
						break;
					default:
						text.Text = CurrentGameState.PuzzleCharacters[i].ToString();
						break;
				}

				var x = i;
				text.Tap += (sender, e) =>
				{
					CharacterPanelLetterPressed(x);
				};

				Border border = new Border();
				border.Child = text;
				border.Style = (Style)Application.Current.Resources["CharacterBorder"];

				if (0 == i % 2)
				{
					LetterPickerPanel1.Children.Add(border);
				}
				else
				{
					LetterPickerPanel2.Children.Add(border);
				}
			}
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
					return;
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