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
using System.Windows.Media.Imaging;
using Facebook;

namespace WhatTheWord.Popups
{
    public partial class SettingsUserControl : UserControl
    {
        private Popup _popup;
        private MainPage _mainPage;

		private BitmapImage _settingsToggleButtonOn = new BitmapImage(new Uri("/Assets/toggleButtonOn@1280_768.png", UriKind.Relative));
		private BitmapImage _settingsToggleButtonOff = new BitmapImage(new Uri("/Assets/toggleButtonOff@1280_768.png", UriKind.Relative));

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
			Overlay.Tap += (sender, e) =>
			{
				this.hide();
			};

            this.PopupWidth = this.HostWindowWidth * 0.9;

            HeaderPanel.Width = this.PopupWidth;
            //HeaderPanel.Height = 102;

            ContentPanel.Width = this.PopupWidth;
            //ContentPanel.MaxHeight = this.PopupHeight - HeaderPanel.Height;

            SoundToggleButton.Source = App.Current.StateData.SoundEnabled ? _settingsToggleButtonOn : _settingsToggleButtonOff;
        }

        #region Show and Hide
        public void show()
        {
			int leftMargin = (int)((HostPanel.Width - this.PopupWidth) / 2.0);
			int topMargin = 60;

			if (string.IsNullOrEmpty(App.Current.StateData.FacebookToken))
			{
				LogOutBorder.Visibility = Visibility.Collapsed;
				ResetEntireGameBackground.Style = this.Resources["SettingBackgrondImage_Alt"] as Style;
				topMargin = 120;
			}
			else
			{
				LogOutBorder.Visibility = Visibility.Visible;
				ResetEntireGameBackground.Style = this.Resources["SettingBackgroundImage"] as Style;
				topMargin = 60;
			}


			HeaderPanel.Margin = new Thickness(leftMargin, topMargin, 0, 0);
			ContentPanel.Margin = new Thickness(leftMargin, 0, 0, 0);

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
            App.Current.StateData.SoundEnabled = !App.Current.StateData.SoundEnabled;
			App.Current.StateData.Save();
            SoundToggleButton.Source = App.Current.StateData.SoundEnabled ? _settingsToggleButtonOn : _settingsToggleButtonOff;

            WhatTheWord.Controls.SoundEffects.PlayClick();
        }

        private void About_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WhatTheWord.Controls.SoundEffects.PlayClick();
            _mainPage.aboutUserControl.isOpenedFromSettings = true;
            _mainPage.aboutUserControl.show();
            this.hide();
        }

        private void Rate_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WhatTheWord.Controls.SoundEffects.PlayClick();
            _mainPage.userReviewUserControl.isOpenedFromSettings = true;
            _mainPage.userReviewUserControl.show();
            this.hide();
        }

        private void Feedback_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WhatTheWord.Controls.SoundEffects.PlayClick();

            byte[] deviceID = (byte[])Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceUniqueId");
			string deviceIDAsString = BitConverter.ToString(deviceID);

            var nameHelper = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
            var version = nameHelper.Version;
            var full = nameHelper.FullName;
            var name = nameHelper.Name;

            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = "Guess This Word Support";
            emailComposeTask.Body = "Device ID: " + deviceIDAsString + Environment.NewLine + 
                "Version: " + version + Environment.NewLine + 
                Environment.NewLine + 
                "(Please enter your issue here)" +
                Environment.NewLine;
            emailComposeTask.To = "support@kooapps.com";

            emailComposeTask.Show();
        }

		private void ResetEntireGame_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			WhatTheWord.Controls.SoundEffects.PlayClick();
			_mainPage.resetGameConfirmationUserControl.isOpenedFromSettings = true;
			_mainPage.resetGameConfirmationUserControl.show();
			this.hide();
		}

		private async void Logout_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			App.Current.StateData.FacebookToken = string.Empty;
			await _mainPage.facebookUserControl.Browser.ClearCookiesAsync();
			App.Current.StateData.Save();
			this.hide();
		}
    }

}
