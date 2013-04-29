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
using System.Reflection;
using WhatTheWord.Controls;

namespace WhatTheWord.Popups
{
    public partial class SettingsUserControl : UserControl
    {
        private Popup _popup;
        private MainPage _mainPage;

        public double HostWindowWidth { get; set; }
        public double HostWindowHeight { get; set; }

        public double PopupWidth { get; set; }
        public double PopupHeight { get; set; }

        public SettingsUserControl(Popup popup, MainPage mainPage, double hostWindowWidth, double hostWindowHeight)
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
            WhatTheWord.Controls.SoundEffects.PlayClick();
            this.hide();
        }

        private void SoundToggleButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SoundEffects.isSoundEnabled = !SoundEffects.isSoundEnabled;
        }

        private void About_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WhatTheWord.Controls.SoundEffects.PlayClick();
            _mainPage.aboutUserControl.isOpenedFromSettings = true;
            _mainPage.aboutUserControl.show();
            this.hide();
        }

        private void Feedback_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WhatTheWord.Controls.SoundEffects.PlayClick();

            byte[] deviceID = (byte[])Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceUniqueId");
            string deviceIDAsString = Convert.ToBase64String(deviceID);

            var nameHelper = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
            var version = nameHelper.Version;
            var full = nameHelper.FullName;
            var name = nameHelper.Name;

            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = "Guess the Word Support";
            emailComposeTask.Body = "Device ID: " + deviceIDAsString + Environment.NewLine + 
                "Version: " + version + Environment.NewLine + 
                Environment.NewLine + 
                "(Please enter your issue here)" +
                Environment.NewLine;
            emailComposeTask.To = "support@kooapps.com";

            emailComposeTask.Show();
        }
    }

}
