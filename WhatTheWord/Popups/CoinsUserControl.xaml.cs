using System;
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
    public partial class CoinsUserControl : UserControl
    {
        Popup _popup;
        GameState _gameState;

        public double HostWindowWidth { get; set; }
        public double HostWindowHeight { get; set; }

        public double PopupWidth { get; set; }
        public double PopupHeight { get; set; }

        public CoinsUserControl(Popup popup, GameState gameState, double hostWindowWidth, double hostWindowHeight)
        {
            InitializeComponent();

            _popup = popup;
            _gameState = gameState;

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

            setupPurchases();
        }

        private async void setupPurchases()
        {
            ListingInformation listingInformation;

            try
            {
                listingInformation = await CurrentApp.LoadListingInformationAsync();

                ContentStackPanel.Children.Clear();

                foreach (ProductListing listing in listingInformation.ProductListings.Values)
                {
                    ContentStackPanel.Children.Add(new InAppPurchaseProduct(listing, _gameState));
                }
            }
            catch (Exception e)
            {
                if (e.Source != null)
                {
                    // error handling
                }
            }
        }

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


        private void BackButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.hide();
        }
    }

}
