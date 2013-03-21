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
        private Popup _popup;
        private MainPage _mainPage;

        public double HostWindowWidth { get; set; }
        public double HostWindowHeight { get; set; }

        public double PopupWidth { get; set; }
        public double PopupHeight { get; set; }

        public CoinsUserControl(Popup popup, MainPage mainPage, double hostWindowWidth, double hostWindowHeight)
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

            setupPurchases();
        }

        private async void setupPurchases()
        {
            try
            {
                ListingInformation listingInformation = await CurrentApp.LoadListingInformationAsync();
                Dictionary<int, InAppPurchase> unorderedPurchases = new Dictionary<int, InAppPurchase>();
                ContentStackPanel.Children.Clear();

                // for each registered product on the Store
                foreach (ProductListing listing in listingInformation.ProductListings.Values)
                {
                    // if there is a match between a registered product on the Store and our own server data
                    if (_mainPage.CurrentGameState.Purchases.ContainsKey(listing.ProductId))
                    {
                        // add it to the list of products to be displayed
                        unorderedPurchases.Add(
                            _mainPage.CurrentGameState.Purchases[listing.ProductId].Order,
                            _mainPage.CurrentGameState.Purchases[listing.ProductId]);
                    }
                }

                // sort the list of products based on Order
                var list = unorderedPurchases.Keys.ToList();
                list.Sort();

                // add the list of products in ascending Order
                foreach (var key in list)
                {
                    ContentStackPanel.Children.Add(new InAppPurchaseProduct(unorderedPurchases[key], _mainPage));
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
