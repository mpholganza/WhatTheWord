﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Windows.Controls.Primitives;

namespace WhatTheWord.Popups
{
    public partial class ResetGameConfirmationUserControl : UserControl
    {
        private Popup _popup;
        private MainPage _mainPage;

        public bool isOpenedFromSettings = false;
        public bool isOpenedFromOutOfPuzzles = false;

        private bool userPressedYes = false;

        public double HostWindowWidth { get; set; }
        public double HostWindowHeight { get; set; }

        public double PopupWidth { get; set; }
        public double PopupHeight { get; set; }

		public ResetGameConfirmationUserControl(Popup popup, MainPage mainPage, double hostWindowWidth, double hostWindowHeight)
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
            if (isOpenedFromOutOfPuzzles)
            {
                if (!userPressedYes)
                {
                    _mainPage.outOfPuzzlesUserControl.show();
                }
                this.isOpenedFromOutOfPuzzles = false;
            }
            else if (isOpenedFromSettings)
            {
                if (!userPressedYes)
                {
                    _mainPage.settingsUserControl.show();
                }
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
            NoResetGame_Tap(sender, e);
        }

		private void NoResetGame_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			WhatTheWord.Controls.SoundEffects.PlayClick();
            userPressedYes = false;
			this.hide();
		}

		private void YesResetGame_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			WhatTheWord.Controls.SoundEffects.PlayClick();
			App.Current.StateData.ResetGame();
            userPressedYes = true;
			this.hide();

			_mainPage.NavigationService.Navigate(new Uri("/LoadingPage.xaml", UriKind.Relative));
			_mainPage.NavigationService.RemoveBackEntry();
		}
    }

}
