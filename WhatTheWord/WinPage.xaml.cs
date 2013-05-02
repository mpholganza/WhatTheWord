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
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Threading;

namespace WhatTheWord
{
	public partial class WinPage : PhoneApplicationPage
	{
        GameState _gameState;
        GameConfig _gameConfig;

        List<Image> winLetters;
        int winLettersIndex;

        System.Windows.Threading.DispatcherTimer myDispatcherTimer;

		public WinPage()
		{
			InitializeComponent();
			NextPuzzle.Click += NextPuzzle_Click;

            _gameState = App.Current.StateData;
            _gameConfig = App.Current.ConfigData;
            winLetters = new List<Image>();
            winLettersIndex = 0;
            myDispatcherTimer = new System.Windows.Threading.DispatcherTimer();

            hideControlsBeforeAnimating();
            updateContent();
            animateContent();
		}

		void NextPuzzle_Click(object sender, RoutedEventArgs e)
		{
            WhatTheWord.Controls.SoundEffects.PlayClick();
			NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
			NavigationService.RemoveBackEntry();
		}

        private void hideControlsBeforeAnimating()
        {
            NiceJob.Opacity = 0;
            TheWordWas.Opacity = 0;
            CoinsEarned.Opacity = 0;
            CoinsEarned.Opacity = 0;
            CoinsIcon.Opacity = 0;
            WinCoins.Opacity = 0;
            NextPuzzle.Opacity = 0;
        }

        private void updateContent()
        {
            int index = Math.Max(_gameState.CurrentLevel - 1, 1);

            Puzzle currentPuzzle = _gameConfig.Puzzles[index];

            // set image
            WinImage.Source = new BitmapImage(new Uri(currentPuzzle.Picture1.URI, UriKind.Relative));

            // set word
            WinWord.Children.Clear();
            winLetters.Clear();
            foreach (char letter in currentPuzzle.Word)
            {
                Image letterImage = new Image();
                letterImage.Name = "WinLetter_" + letter + "_" + winLetters.Count.ToString();
                letterImage.Source = new BitmapImage(new Uri("/Assets/WinLetters/gb_" + letter + "_win@1280_768.png", UriKind.Relative));
                letterImage.Margin = new Thickness(4,0,4,0);
                letterImage.Height = 40;
                letterImage.Opacity = 0;
                WinWord.Children.Add(letterImage);
                winLetters.Add(letterImage);
            }
            
            // set coins
            WinCoins.Text = _gameConfig.rewardCoinsPerQuestion.ToString();
        }

        private void animateContent()
        {
            animateNiceJob(this, null);
        }

        private void animateNiceJob(object sender, EventArgs e)
        {
            NiceJobStoryboard.Completed += animateTheWordWas;
            NiceJobStoryboard.Begin();
            NiceJob.Opacity = 1;
        }

        private void animateTheWordWas(object sender, EventArgs e)
        {
            Thread.Sleep(350);

            winLettersIndex = 0;
            TheWordWasStoryboard.Completed += animateWinLetter;
            TheWordWasStoryboard.Begin();
            TheWordWas.Opacity = 1;
        }

        private void animateWinLetter(object sender, EventArgs e)
        {
            if (winLettersIndex == 0)
            {
                Thread.Sleep(30);
            }

            if (winLettersIndex < winLetters.Count)
            {
                Image letterImage = winLetters[winLettersIndex];
                setRenderTransform(letterImage);

                Storyboard storyboard = createWinLetterStoryboard();
                foreach (Timeline timeline in storyboard.Children)
                {
                    Storyboard.SetTarget(timeline, letterImage);
                }

                winLettersIndex++;
                storyboard.Completed += animateWinLetter;
                storyboard.Begin();
                letterImage.Opacity = 1;
            }
            else
            {
                animateCoinsEarned(sender, e);
            }
        }

        private void setRenderTransform(UIElement ui)
        {
            ui.RenderTransformOrigin = new Point(0.5, 0.5);

            CompositeTransform compositeTransform = new CompositeTransform();
            compositeTransform.ScaleX = 1;
            compositeTransform.ScaleY = 1;
            ui.RenderTransform = compositeTransform;
        }

        private Storyboard createWinLetterStoryboard()
        {
            DoubleAnimation scaleXAnimation = new DoubleAnimation();
            scaleXAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            scaleXAnimation.From = 0;
            scaleXAnimation.To = 1;
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleX)"));

            DoubleAnimation scaleYAnimation = new DoubleAnimation();
            scaleYAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            scaleYAnimation.From = 0;
            scaleYAnimation.To = 1;
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleY)"));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(scaleXAnimation);
            storyboard.Children.Add(scaleYAnimation);

            return storyboard;
        }

        private void animateCoinsEarned(object sender, EventArgs e)
        {
            Thread.Sleep(400);

            CoinsEarnedStoryboard.Completed += animateWinCoins;
            CoinsEarnedStoryboard.Begin();
            CoinsEarned.Opacity = 1;
        }



        private void animateWinCoins(object sender, EventArgs e)
        {
            Thread.Sleep(350);

            //CoinsIconStoryboard.Completed += animateNextPuzzle;
            CoinsIconStoryboard.Begin();
            //CoinsIcon.Visibility = System.Windows.Visibility.Visible;
            CoinsIcon.Opacity = 1;

            //WinCoinsAnimation.To = _gameConfig.rewardCoinsPerQuestion;
            //WinCoinsStoryboard.Begin();

            WinCoins.Text = "0";
            myDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 30); // 100 Milliseconds 
            myDispatcherTimer.Tick += new EventHandler(incrementWinCoins);
            myDispatcherTimer.Start();
            WinCoins.Opacity = 1;
        }

        private void incrementWinCoins(object sender, EventArgs e)
        {
            int currentCoins = int.Parse(WinCoins.Text) + 1;

            if (currentCoins > _gameConfig.rewardCoinsPerQuestion)
            {
                myDispatcherTimer.Stop();
                animateNextPuzzle(sender, e);
            }
            else
            {
                WinCoins.Text = currentCoins.ToString();
            }
        }

        private void animateNextPuzzle(object sender, EventArgs e)
        {
            Thread.Sleep(350);
            NextPuzzleStoryboard.Begin();
            NextPuzzle.Opacity = 1;
        }
	}
}