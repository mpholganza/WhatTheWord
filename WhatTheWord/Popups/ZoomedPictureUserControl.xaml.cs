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
using System.Windows.Media.Imaging;

namespace WhatTheWord.Popups
{
    public partial class ZoomedPictureUserControl : UserControl
    {
        private Popup _popup;
        private MainPage _mainPage;

        public bool isOpenedFromSettings = false;

        public double HostWindowWidth { get; set; }
        public double HostWindowHeight { get; set; }

        public double PopupWidth { get; set; }
        public double PopupHeight { get; set; }

		public ZoomedPictureUserControl(Popup popup, MainPage mainPage, double hostWindowWidth, double hostWindowHeight)
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

            this.PopupWidth = this.HostWindowWidth - (29 * 2);

			ThePicture.Width = this.PopupWidth;
			ThePicture.Height = this.PopupWidth;
			ThePicture.Tap += (sender, e) =>
			{
				this.hide();
			};

            ContentPanel.Width = this.PopupWidth;
            //ContentPanel.MaxHeight = this.PopupHeight - HeaderPanel.Height;

            int leftMargin = (int) ((HostPanel.Width - this.PopupWidth) / 2.0);
            int topMargin = (int) (97);

            ContentPanel.Margin = new Thickness(leftMargin, topMargin, 0, 0);

        }

        #region Show and Hide
		public void show(Image image)
		{
			if (!_popup.IsOpen)
			{
				_popup.Child = this;
				_popup.IsOpen = true;
				ThePicture.Source = image.Source;
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

            ContentPanel.Visibility = System.Windows.Visibility.Collapsed;

            if (!_popup.IsOpen)
            {
                _popup.Child = this;
                _popup.IsOpen = true;
            }
        }
        #endregion
    }

}
