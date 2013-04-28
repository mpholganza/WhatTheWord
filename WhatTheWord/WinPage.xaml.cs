using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WhatTheWord.Model;
using System.Windows.Media.Imaging;

namespace WhatTheWord
{
	public partial class WinPage : PhoneApplicationPage
	{
        GameState _gameState;
        GameConfig _gameConfig;

		public WinPage()
		{
			InitializeComponent();
			NextPuzzle.Click += NextPuzzle_Click;

            _gameState = App.Current.StateData;
            _gameConfig = App.Current.ConfigData;

            updateContent();
		}

		void NextPuzzle_Click(object sender, RoutedEventArgs e)
		{
			NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
			NavigationService.RemoveBackEntry();
		}

        private void updateContent()
        {
            int index = Math.Max(_gameState.CurrentLevel - 1, 0);

            Puzzle currentPuzzle = _gameConfig.Puzzles[index];

            // set image
            WinImage.Source = new BitmapImage(new Uri(currentPuzzle.Picture1.URI, UriKind.Relative));

            // set word
            WinWord.Children.Clear();
            foreach (char letter in currentPuzzle.Word)
            {
                Image letterImage = new Image();
                letterImage.Source = new BitmapImage(new Uri("/Assets/WinLetters/gb_" + letter + "_win@1280_768.png", UriKind.Relative));
                letterImage.Margin = new Thickness(4,0,4,0);
                letterImage.Height = 40;
                WinWord.Children.Add(letterImage);
            }
            
            // set coins
            WinCoins.Text = _gameConfig.rewardCoinsPerQuestion.ToString();
        }
	}
}