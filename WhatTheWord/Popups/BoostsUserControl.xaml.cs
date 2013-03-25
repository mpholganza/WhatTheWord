﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Facebook;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;

#if DEBUG
using MockIAPLib;
using System.Threading.Tasks;
using WhatTheWord.Model;
#else
using Windows.ApplicationModel.Store;
#endif

namespace WhatTheWord.Popups
{
    public partial class BoostsUserControl : UserControl
    {
        private Popup _popup;
        private MainPage _mainPage;

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
            RevealALetterButton_Text.Text = _mainPage.CurrentGameState.boostRevealLetterCost.ToString();
            RemoveALetterButton_Text.Text = _mainPage.CurrentGameState.boostRemoveLettersCost.ToString();
            ShuffleButton_Text.Text = _mainPage.CurrentGameState.boostShuffleCost.ToString();
        }

        #region Show and Hide
        public void show()
        {
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
            this.hide();
        }

        private void openCoinsPopup()
        {
            _mainPage.coinsUserControl.isOpenedFromBoosts = true;
            _mainPage.coinsUserControl.show();
        }

        private void RevealALetterButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            int cost = int.Parse(RevealALetterButton_Text.Text);

            if (cost > _mainPage.CurrentGameState.Coins)
            {
                openCoinsPopup();
            }
            else
            {
                _mainPage.CurrentGameState.Coins -= cost;
                _mainPage.CurrentGameState.RevealLetter();
                _mainPage.DisplayGame();
            }
            
            this.hide();
        }

        private void RemoveALetterButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            int cost = int.Parse(RemoveALetterButton_Text.Text);

            if (cost > _mainPage.CurrentGameState.Coins)
            {
                openCoinsPopup();
            }
            else
            {
                _mainPage.CurrentGameState.Coins -= cost;
                _mainPage.CurrentGameState.RemoveLetter();
                _mainPage.DisplayGame();
            }

            this.hide();
        }

        private void ShuffleButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            int cost = int.Parse(ShuffleButton_Text.Text);

            if (cost > _mainPage.CurrentGameState.Coins)
            {
                openCoinsPopup();
            }
            else
            {
                _mainPage.CurrentGameState.Coins -= cost;
                _mainPage.CurrentGameState.JumblePuzzleCharacters();
                _mainPage.DisplayGame();
            }

            this.hide();
        }
    }

}
