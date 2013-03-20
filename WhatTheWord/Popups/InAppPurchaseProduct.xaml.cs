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
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using WhatTheWord.Model;
#else
using Windows.ApplicationModel.Store;
#endif

namespace WhatTheWord.Popups
{
    public partial class InAppPurchaseProduct : UserControl
    {
        private MainPage _mainPage;

        public InAppPurchaseProduct(ProductListing listing, MainPage mainPage)
        {
            InitializeComponent();

            _mainPage = mainPage;

            ProductName.Text = listing.Name;
            Price.Text = listing.FormattedPrice;
            Button.Tag = listing.ProductId;

            Button.Tap += PurchaseButton_Click;
        }

        private void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            PurchaseProduct((String)b.Tag);
        }

        async void PurchaseProduct(string productId)
        {
            await CurrentApp.RequestProductPurchaseAsync(productId, false);
            DoFulfillment();
        }

        public void DoFulfillment()
        {
            var productLicenses = CurrentApp.LicenseInformation.ProductLicenses;
            string coinProductIdPrefix = "WhatTheWord.Consumables.Coins.";
            int coinsToAdd = 0;

            foreach (ProductLicense license in productLicenses.Values)
            {
                if (license.IsConsumable && license.IsActive)
                {
                    if (license.ProductId.StartsWith(coinProductIdPrefix))
                    {
                        coinsToAdd += int.Parse(license.ProductId.Substring(coinProductIdPrefix.Length));
                        CurrentApp.ReportProductFulfillment(license.ProductId);
                    }
                }
            }

            _mainPage.CurrentGameState.Coins += coinsToAdd;
            _mainPage.DisplayGame();
        }
    }
}
