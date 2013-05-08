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

        private void setupBoosts()
        {
            RevealALetterButton_Text.Text = App.Current.ConfigData.boostRevealLetterCost.ToString();
            RemoveALetterButton_Text.Text = App.Current.ConfigData.boostRemoveLettersCost.ToString();
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
                this.RevealALetterButton_Icon.Source = canRevealLetter ? _boostButtonEnabledIcon : _boostButtonDisabledIcon;
            }

            if (this.RemoveALetterButton.IsEnabled != canRemoveLetter)
            {
                this.RemoveALetterButton.IsEnabled = canRemoveLetter;
                this.RemoveALetterButton_Icon.Source = canRemoveLetter ? _boostButtonEnabledIcon : _boostButtonDisabledIcon;
            }
        }

		private void RevealALetter()
		{
			int cost = int.Parse(RevealALetterButton_Text.Text);

			if (cost > App.Current.StateData.Coins)
			{
				openCoinsPopup();
			}
			else
			{
				App.Current.StateData.Coins -= cost;
				App.Current.StateData.RevealLetter();
				_mainPage.DisplayGame();
				WhatTheWord.Controls.SoundEffects.PlayBuy();
			}

			this.hide();
		}

        private void RevealALetterButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
			RevealALetter();
        }

		private void RemoveALetter()
		{
			int cost = int.Parse(RemoveALetterButton_Text.Text);

			if (cost > App.Current.StateData.Coins)
			{
				openCoinsPopup();
			}
			else
			{
				App.Current.StateData.Coins -= cost;
				App.Current.StateData.RemoveLetter();
				_mainPage.DisplayGame();
				WhatTheWord.Controls.SoundEffects.PlayBuy();
			}

			this.hide();
		}

        private void RemoveALetterButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
			RemoveALetter();
        }

		//private void ShuffleButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		//{
		//	int cost = int.Parse(ShuffleButton_Text.Text);

		//	if (cost > App.Current.StateData.Coins)
		//	{
		//		openCoinsPopup();
		//	}
		//	else
		//	{
		//		App.Current.StateData.Coins -= cost;
		//		App.Current.StateData.JumblePuzzleCharacters();
		//		_mainPage.DisplayGame();
		//		WhatTheWord.Controls.SoundEffects.PlayBuy();
		//	}

		//	this.hide();
		//}

		private void RevealALetterRow_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			RevealALetter();
		}

		private void RemoveALetterRow_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			RemoveALetter();
		}
    }

}
