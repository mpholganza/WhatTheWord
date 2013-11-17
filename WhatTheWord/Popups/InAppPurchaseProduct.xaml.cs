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

using System.Threading.Tasks;
using System.Text.RegularExpressions;
using WhatTheWord.Model;

namespace WhatTheWord.Popups
{
    public partial class InAppPurchaseProduct : UserControl
    {
        private InAppPurchase _product;
        private MainPage _mainPage;
        private CoinsUserControl _coinsUserControl;

        public InAppPurchaseProduct(InAppPurchase product, MainPage mainPage, CoinsUserControl coinsUserControl)
        {
            InitializeComponent();

            _mainPage = mainPage;
            _product = product;
            _coinsUserControl = coinsUserControl;

            Price.Text = String.Format("{0:C}", _product.Price);
            ProductName.Text = _product.Name;

            if ( String.IsNullOrWhiteSpace(_product.Discount) )
            {
                ProductDiscount.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                ProductDiscount.Text = _product.Discount;
            }

            Button.Tap += PurchaseButton_Click;
        }

        private void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            WhatTheWord.Controls.SoundEffects.PlayClick();
            PurchaseProduct();
        }

        async void PurchaseProduct()
        {
            Instrumentation.GetInstance().sendInstrumentation(
                "Monetization", "AttemptIap", this._product.BundleId, null, _product.Price.ToString());

            string receipt = "";
            try
            {
                receipt = await CurrentApp.RequestProductPurchaseAsync(_product.BundleId, true);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                MessageBox.Show("We can't connect to the Store right now. Please try again later.");
            }
            DoFulfillment(receipt);
        }

        public void DoFulfillment(string receipt)
        {
            var productLicenses = CurrentApp.LicenseInformation.ProductLicenses;
            int coinsToAdd = 0;

            foreach (ProductLicense license in productLicenses.Values)
            {
                if (license.IsConsumable && license.IsActive && App.Current.ConfigData.Purchases.ContainsKey(license.ProductId))
                {
                    coinsToAdd += App.Current.ConfigData.Purchases[license.ProductId].Coins;
                    CurrentApp.ReportProductFulfillment(license.ProductId);

                    Instrumentation.GetInstance().sendInstrumentation(
                        "Monetization", "SuccessIap", license.ProductId, receipt, _product.Price.ToString());
                }
            }

            if (coinsToAdd != 0)
            {
                App.Current.StateData.Coins += coinsToAdd;
				App.Current.StateData.Save();
                _mainPage.DisplayGame();
                WhatTheWord.Controls.SoundEffects.PlayBuy();
            }

            _coinsUserControl.hide();
        }
    }
}
