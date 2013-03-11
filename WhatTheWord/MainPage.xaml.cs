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
			CurrentPuzzle = new Puzzle ("Puzzle1")
			{
				Picture1 = new Picture { URI = "/Assets/PuzzlePictures/mangoes.jpg", Credits = "mph" },
				Picture2 = new Picture { URI = "/Assets/PuzzlePictures/icecream.jpg", Credits = "mph" },
				Picture3 = new Picture { URI = "/Assets/PuzzlePictures/water_houses.jpg", Credits = "mph" },
				Picture4 = new Picture { URI = "/Assets/PuzzlePictures/magnets.jpg", Credits = "mph" },
			};

			LoadInitialGameState();
		}

		private void LoadInitialGameState()
		{
			LayoutRoot.DataContext = CurrentPuzzle;

			for (int i = 0; i < CurrentPuzzle.Word.Length; i++)
			{
				TextBlock text = new TextBlock();
				text.Width = 48;
				text.Height = 48;
				text.Text = CurrentPuzzle.Word[i].ToString();
				GuessPanel.Children.Add(text);
			}

			for (int i = 0; i < CurrentPuzzle.Characters.Length; i++)
			{
				TextBlock text = new TextBlock();
				text.Height = 48;
				text.Width = 48;
				text.Style = (Style)Application.Current.Resources["Character"];
				text.Text = CurrentPuzzle.Characters[i].ToString();
				text.Tap += text_Tap;
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

		void text_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			switch (sender.GetType().ToString())
			{
				case "TextBox":
					TextBlock text = (TextBlock)sender;
					break;
				default:
					throw new ApplicationException("Unexpected object.");
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