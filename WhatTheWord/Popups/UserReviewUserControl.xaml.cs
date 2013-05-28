using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Windows.Controls.Primitives;
using Microsoft.Phone.Tasks;

namespace WhatTheWord.Popups
{
    public partial class UserReviewUserControl : UserControl
    {
        private Popup _popup;
        private MainPage _mainPage;

        public bool isOpenedFromSettings = false;

        public double HostWindowWidth { get; set; }
        public double HostWindowHeight { get; set; }

        public double PopupWidth { get; set; }
        public double PopupHeight { get; set; }

        private readonly string UserReviewText_WithReward =
			"Tell us what you think about Guess This Word and get " + App.Current.ConfigData.rateMeReward + " coins for free!";
        private readonly string UserReviewText_ThankYou =
            "Thank you for rating Guess This Word. We have added " + App.Current.ConfigData.rateMeReward + " coins to your game!";
        private readonly string UserReviewText_NoReward =
            "Tell us what you think about Guess This Word!";

        public UserReviewUserControl(Popup popup, MainPage mainPage, double hostWindowWidth, double hostWindowHeight)
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

            if (!App.Current.StateData.RewardGivenForUserReview)
            {
                UserReviewText.Text = UserReviewText_WithReward;
            }
            else
            {
                UserReviewText.Text = UserReviewText_NoReward;
            }
        }

        #region Show and Hide
        public void show()
        {
            if (!_popup.IsOpen)
            {
                if (!App.Current.StateData.RewardGivenForUserReview)
                {
                    UserReviewText.Text = UserReviewText_WithReward;
                    UserReviewButton.IsEnabled = true;
                }
                else
                {
                    UserReviewText.Text = UserReviewText_NoReward;
                    UserReviewButton.IsEnabled = true;
                }

                _popup.Child = this;
                _popup.IsOpen = true;
            }
        }

        public void hide()
        {
            if (isOpenedFromSettings)
            {
                _mainPage.settingsUserControl.show();
                this.isOpenedFromSettings = false;
            }
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

        private void UserReviewButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WhatTheWord.Controls.SoundEffects.PlayClick();
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();

            if (!App.Current.StateData.RewardGivenForUserReview)
            {
                System.Threading.Thread.Sleep(500);
                App.Current.StateData.Coins += App.Current.ConfigData.rateMeReward;
                App.Current.StateData.RewardGivenForUserReview = true;
                UserReviewText.Text = UserReviewText_ThankYou;
                UserReviewButton.IsEnabled = false;
                _mainPage.DisplayGame();
            }
            else
            {
                this.hide();
            }
        }
    }

}
