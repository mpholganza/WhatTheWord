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
using System.Windows.Media.Animation;

namespace WhatTheWord.Popups
{
    public partial class NewPuzzlesUserControl : UserControl
    {
        private Popup _popup;
        private MainPage _mainPage;

        public bool isOpenedFromSettings = false;

        public double HostWindowWidth { get; set; }
        public double HostWindowHeight { get; set; }

        public double PopupWidth { get; set; }
        public double PopupHeight { get; set; }

        public NewPuzzlesUserControl(Popup popup, MainPage mainPage, double hostWindowWidth, double hostWindowHeight)
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
        }

		void Downloader_FileDownloaded(object sender, FileDownloadedEventArgs e)
		{
			if (e.FilesLeftCount == 0)
			{
				App.Current.Downloader.FileDownloaded -= Downloader_FileDownloaded;
				this.hide();
				// Reload the game
				_mainPage.NavigationService.Navigate(new Uri("/LoadingPage.xaml", UriKind.Relative));
				_mainPage.NavigationService.RemoveBackEntry();
			}
		}

        #region Show and Hide
        public void show()
        {
			App.Current.Downloader.FileDownloaded += Downloader_FileDownloaded;
			AnimateContent();
            if (!_popup.IsOpen)
            {
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

		private void AnimateContent()
		{
			LoadingAnimation.RepeatBehavior = RepeatBehavior.Forever;
			LoadingAnimation.Begin();
		}

        #endregion
    }

}
