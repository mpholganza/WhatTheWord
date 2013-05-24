using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

#if DEBUG
using MockIAPLib;
#else
using Windows.ApplicationModel.Store;
#endif

using Facebook;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
using System.Threading.Tasks;
using WhatTheWord.Model;


namespace WhatTheWord.Popups
{
    public partial class BoostsUserControl : UserControl
    {
        private Popup _popup;
        private MainPage _mainPage;

        private BitmapImage _boostButtonEnabledIcon = new BitmapImage(new Uri("/Assets/coinIcon@1280_768.png", UriKind.Relative));
        private BitmapImage _boostButtonDisabledIcon = new BitmapImage(new Uri("/Assets/coinIcon_disabled@1280_768.png", UriKind.Relative));

        public double HostWindowWidth { get; set; }
        public double HostWindowHeight { get; set; }

        public double PopupWidth { get; set; }
        public double PopupHeight { get; set; }

        public BoostsUserControl(Popup popup, MainPage mainPage, double hostWindowWidth, double hostWindowHeight)
        {
            InitializeComponent();

            _popup = popup;
            _mainPage = mainPage;

            this.HostWindowWidth = hostWindowWidth;
            this.HostWindowHeight = hostWindowHeight;

            HostPanel.Width = this.HostWindowWidth;
            HostPanel.Height = this.HostWindowHeight;

            Overlay.Width = this.HostWindowWidth;
            Overlay.Height = this.HostWindowHeight;
			Overlay.Tap += (sender, e) =>
			{
				this.hide();
			};

            this.PopupWidth = this.HostWindowWidth * 0.9;

            HeaderPanel.Width = this.PopupWidth;
            //HeaderPanel.Height = 102;

            ContentPanel.Width = this.PopupWidth;
            //ContentPanel.MaxHeight = this.PopupHeight - HeaderPanel.Height;

            int leftMargin = (int) ((HostPanel.Width - this.PopupWidth) / 2.0);
            int topMargin = (int) (120);

            HeaderPanel.Margin = new Thickness(leftMargin, topMargin, 0 , 0);
            ContentPanel.Margin = new Thickness(leftMargin, 0, 0, 0);

            setupBoosts();
        }

        private int calculateAnswerPuzzleCost()
        {
            int currentLevel = App.Current.StateData.CurrentLevel;
            int numLetters = currentLevel > 0 ? App.Current.ConfigData.Puzzles[currentLevel].Word.Length : 0;

            return numLetters <= 3 ? 120 : (numLetters * 40) - 40;
        }

        private void setupBoosts()
        {
            RevealALetterButton_Text.Text = App.Current.ConfigData.boostRevealLetterCost.ToString();
            RemoveALetterButton_Text.Text = App.Current.ConfigData.boostRemoveLettersCost.ToString();
            AnswerPuzzleButton_Text.Text = calculateAnswerPuzzleCost().ToString();
			//ShuffleButton_Text.Text = App.Current.ConfigData.boostShuffleCost.ToString();
        }

        #region Show and Hide
        public void show()
        {
            disableButtonsIfCannotBoost();
            if (!_popup.IsOpen)
            {
                _popup.Child = this;
                _popup.IsOpen = true;
            }
        }

        public void hide()
        {
            _popup.IsOpen = false;
        }

        public bool isOpen()
        {
            return _popup.IsOpen;
        }

        private void showLoading()
        {
            Overlay.Visibility = System.Windows.Visibility.Visible;

            HeaderPanel.Visibility = System.Windows.Visibility.Collapsed;
            ContentPanel.Visibility = System.Windows.Visibility.Collapsed;

            if (!_popup.IsOpen)
            {
                _popup.Child = this;
                _popup.IsOpen = true;
            }
        }
        #endregion

        private void BackButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WhatTheWord.Controls.SoundEffects.PlayClick();
            this.hide();
        }

        private void openCoinsPopup()
        {
            _mainPage.coinsUserControl.isOpenedFromBoosts = true;
            _mainPage.coinsUserControl.show();
        }

        private void disableButtonsIfCannotBoost()
        {
            bool canRevealLetter = App.Current.StateData.CanRevealLetter();
            bool canRemoveLetter = App.Current.StateData.CanRemoveLetter();

            if (this.RevealALetterButton.IsEnabled != canRevealLetter)
            {
                this.RevealALetterButton.IsEnabled = canRevealLetter;
                this.RevealALetterButton_Text.Text = canRevealLetter ? App.Current.ConfigData.boostRevealLetterCost.ToString() : "MAX";
                this.RevealALetterButton_Icon.Source = canRevealLetter ? _boostButtonEnabledIcon : null;
            }

            if (this.RemoveALetterButton.IsEnabled != canRemoveLetter)
            {
                this.RemoveALetterButton.IsEnabled = canRemoveLetter;
                this.RemoveALetterButton_Text.Text = canRemoveLetter ? App.Current.ConfigData.boostRemoveLettersCost.ToString() : "MAX";
                this.RemoveALetterButton_Icon.Source = canRemoveLetter ? _boostButtonEnabledIcon : null;
            }
        }

		private void RevealALetter()
		{
			int cost = int.Parse(RevealALetterButton_Text.Text);

			if (cost > App.Current.StateData.Coins)
            {
                Instrumentation.GetInstance().sendInstrumentation(
                    "Monetization", "InsufficientCurrency", "revealaletter", null, cost.ToString());

                WhatTheWord.Controls.SoundEffects.PlayClick();
				openCoinsPopup();
			}
			else
            {
                Instrumentation.GetInstance().sendInstrumentation(
                    "Monetization", "Purchase", "revealaletter", null, cost.ToString());

                WhatTheWord.Controls.SoundEffects.PlayBuy();
				App.Current.StateData.Coins -= cost;
				App.Current.StateData.RevealLetter();
				App.Current.StateData.Save();
				_mainPage.DisplayGame();
			}

			this.hide();
		}

        private void RevealALetter_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
			RevealALetter();
        }

		private void RemoveALetter()
		{
			int cost = int.Parse(RemoveALetterButton_Text.Text);

			if (cost > App.Current.StateData.Coins)
            {
                Instrumentation.GetInstance().sendInstrumentation(
                    "Monetization", "InsufficientCurrency", "removealetter", null, cost.ToString());

                WhatTheWord.Controls.SoundEffects.PlayClick();
				openCoinsPopup();
			}
			else
            {
                Instrumentation.GetInstance().sendInstrumentation(
                    "Monetization", "Purchase", "removealetter", null, cost.ToString());

                WhatTheWord.Controls.SoundEffects.PlayBuy();
				App.Current.StateData.Coins -= cost;
				App.Current.StateData.RemoveLetter();
				App.Current.StateData.Save();
				_mainPage.DisplayGame();
			}

			this.hide();
		}

        private void RemoveALetter_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
			RemoveALetter();
        }

        private void AnswerPuzzle()
        {
            int cost = int.Parse(AnswerPuzzleButton_Text.Text);

            if (cost > App.Current.StateData.Coins)
            {
                Instrumentation.GetInstance().sendInstrumentation(
                    "Monetization", "InsufficientCurrency", "answerpuzzle", null, cost.ToString());

                WhatTheWord.Controls.SoundEffects.PlayClick();
                openCoinsPopup();
            }
            else
            {
                Instrumentation.GetInstance().sendInstrumentation(
                    "Monetization", "Purchase", "answerpuzzle", null, cost.ToString());

                WhatTheWord.Controls.SoundEffects.PlayBuy();
                App.Current.StateData.Coins -= cost;
                App.Current.StateData.AnswerPuzzle();
				App.Current.StateData.Save();
                _mainPage.DisplayGame();
                _mainPage.PuzzleComplete();
            }

            this.hide();
        }

        private void AnswerPuzzle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            AnswerPuzzle();
        }
    }

}
