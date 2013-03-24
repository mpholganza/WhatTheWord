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

namespace WhatTheWord.Popups
{
    public partial class FacebookUserControl : UserControl
    {
        private const string AppId = "561883217157240";

        /// <summary>
        /// Extended permissions is a comma separated list of permissions to ask the user.
        /// </summary>
        /// <remarks>
        /// For extensive list of available extended permissions refer to 
        /// https://developers.facebook.com/docs/reference/api/permissions/
        /// </remarks>
        //private const string ExtendedPermissions = "user_about_me,read_stream,publish_stream";
        private const string ExtendedPermissions = "publish_actions";

        private readonly FacebookClient _fb = new FacebookClient();
        private string _accessToken = "";

        private bool isBrowserLoaded = false;

        private Popup _popup;
        private MainPage _mainPage;
        private UIElement _screenshotUI;
        private WriteableBitmap _bitmap;

        public double HostWindowWidth { get; set; }
        public double HostWindowHeight { get; set; }

        public double PopupWidth { get; set; }
        public double PopupHeight { get; set; }

        public FacebookUserControl(Popup popup, MainPage mainPage, double hostWindowWidth, double hostWindowHeight)
        {
            InitializeComponent();

            _popup = popup;
            _mainPage = mainPage;
            _screenshotUI = _mainPage.LayoutRoot;

            this.HostWindowWidth = hostWindowWidth;
            this.HostWindowHeight = hostWindowHeight;

            HostPanel.Width = this.HostWindowWidth;
            HostPanel.Height = this.HostWindowHeight;

            Overlay.Width = this.HostWindowWidth;
            Overlay.Height = this.HostWindowHeight;

            this.PopupWidth = this.HostWindowWidth * 0.9;
            this.PopupHeight = this.HostWindowHeight * 0.8;

            HeaderPanel.Width = this.PopupWidth;
            //HeaderPanel.Height = 102;

            ContentPanel.Width = this.PopupWidth;
            //ContentPanel.MaxHeight = this.PopupHeight - HeaderPanel.Height;

            Browser.Width = this.PopupWidth;
            Browser.Height = this.PopupHeight - HeaderPanel.Height;

            int leftMargin = (int) ((HostPanel.Width - this.PopupWidth) / 2.0);
            int topMargin = (int) ((HostPanel.Height - this.PopupHeight) / 2.0);

            HeaderPanel.Margin = new Thickness(leftMargin, topMargin, 0 , 0);
            ContentPanel.Margin = new Thickness(leftMargin, 0, 0, 0);
            Browser.Margin = new Thickness(leftMargin, 0, 0, 0);
        }

        #region Show and Hide

        public void show()
        {
            showLoading();

            var loginUrl = GetFacebookLoginUrl(AppId, ExtendedPermissions);
            Browser.Navigate(loginUrl);

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
            Browser.Visibility = System.Windows.Visibility.Collapsed;
            ContentPanel.Visibility = System.Windows.Visibility.Collapsed;

            if (!_popup.IsOpen)
            {
                _popup.Child = this;
                _popup.IsOpen = true;
            }
        }

        private void showLogin()
        {
            HeaderTitle.Text = "Login";

            Overlay.Visibility = System.Windows.Visibility.Visible;
            HeaderPanel.Visibility = System.Windows.Visibility.Visible;
            Browser.Visibility = System.Windows.Visibility.Visible;

            ContentPanel.Visibility = System.Windows.Visibility.Collapsed;

            if (!_popup.IsOpen)
            {
                _popup.Child = this;
                _popup.IsOpen = true;
            }
        }

        private void showPost()
        {
            HeaderTitle.Text = "Share";

            Overlay.Visibility = System.Windows.Visibility.Visible;
            HeaderPanel.Visibility = System.Windows.Visibility.Visible;
            ContentPanel.Visibility = System.Windows.Visibility.Visible;

            Browser.Visibility = System.Windows.Visibility.Collapsed;

            if (!_popup.IsOpen)
            {
                _popup.Child = this;
                _popup.IsOpen = true;
            }
        }
        #endregion

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
            this.isBrowserLoaded = true;
            bool b = this.isBrowserLoaded;
        }

        private Uri GetFacebookLoginUrl(string appId, string extendedPermissions)
        {
            var parameters = new Dictionary<string, object>();
            parameters["client_id"] = appId;
            parameters["redirect_uri"] = "https://www.facebook.com/connect/login_success.html";
            parameters["response_type"] = "token";
            parameters["display"] = "touch";

            // add the 'scope' only if we have extendedPermissions.
            if (!string.IsNullOrEmpty(extendedPermissions))
            {
                // A comma-delimited list of permissions
                parameters["scope"] = extendedPermissions;
            }

            return _fb.GetLoginUrl(parameters);
        }

        private void Browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            FacebookOAuthResult oauthResult;
            if (!_fb.TryParseOAuthCallbackUrl(e.Uri, out oauthResult))
            {
                showLogin();
                return;
            }

            if (oauthResult.IsSuccess)
            {
                _accessToken = oauthResult.AccessToken;

                showPost();
                takeScreenshot();
            }
            else
            {
                // user cancelled
                MessageBox.Show(oauthResult.ErrorDescription);
            }
        }


        private void PostToFacebook()
        {
            FacebookClient fb = new FacebookClient(_accessToken);

            fb.PostCompleted += (o, e) =>
            {
                if (e.Cancelled || e.Error != null)
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show(e.Error.Message));
                    return;
                }

                var result = (IDictionary<string, object>)e.GetResultData();
                var id = (string)result["id"];
            };

            // Encode to JPEG format
            byte[] screenshot;
            using (var stream = new MemoryStream())
            {
                _bitmap.SaveJpeg(stream, _bitmap.PixelWidth, _bitmap.PixelHeight, 0, 95);
                stream.Seek(0, SeekOrigin.Begin);
                screenshot = ConvertToByteArray(stream);
            }

            var parameters = new Dictionary<string, object>();
            parameters["message"] = Message.Text;
            parameters["file"] = new FacebookMediaObject
            {
                ContentType = "image/jpeg",
                FileName = "image.jpeg"
            }.SetValue(screenshot);

            //fb.PostAsync("me/feed", parameters);
            fb.PostAsync("me/photos", parameters);

            this.hide();
        }

        private void PostButton_Tap(object sender, System.Windows.Input.GestureEventArgs evt)
        {
            PostToFacebook();
            this.hide();
        }

        private void BackButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.hide();
        }

        #region Image Helper Functions
        private void takeScreenshot()
        {
            _bitmap = new WriteableBitmap((int)Application.Current.Host.Content.ActualWidth, (int)Application.Current.Host.Content.ActualHeight);
            _bitmap.Render(_screenshotUI, new System.Windows.Media.MatrixTransform());
            _bitmap.Invalidate();

            Screenshot.Source = _bitmap;
        }

        public byte[] ConvertToByteArray(Stream fileStream)
        {
            if (fileStream != null)
            {
                int fileStreamLength = (int)fileStream.Length;
                byte[] bytes = new byte[fileStreamLength];
                fileStream.Read(bytes, 0, fileStreamLength);

                return bytes;
            }

            return null;
        }
        #endregion

        #region Animation
        private static bool fadedIn = false;

        private void fadeIn(FrameworkElement ui)
        {
            if (fadedIn)
            {
                return;
            }
            fadedIn = true;
            // Create a duration of 2 seconds.
            Duration duration = new Duration(TimeSpan.FromMilliseconds(200));

            // Create two DoubleAnimations and set their properties.
            DoubleAnimation myDoubleAnimation1 = new DoubleAnimation();
            DoubleAnimation myDoubleAnimation2 = new DoubleAnimation();

            myDoubleAnimation1.Duration = duration;
            myDoubleAnimation2.Duration = duration;

            Storyboard sb = new Storyboard();
            sb.Duration = duration;

            sb.Children.Add(myDoubleAnimation1);
            sb.Children.Add(myDoubleAnimation2);

            Storyboard.SetTarget(myDoubleAnimation1, ui);
            Storyboard.SetTarget(myDoubleAnimation2, ui);

            // Set the attached properties of Canvas.Left and Canvas.Top
            // to be the target properties of the two respective DoubleAnimations.
            Storyboard.SetTargetProperty(myDoubleAnimation1, new PropertyPath("Width"));
            Storyboard.SetTargetProperty(myDoubleAnimation2, new PropertyPath("Height"));

            myDoubleAnimation1.From = 0;
            myDoubleAnimation1.To = this.PopupWidth;

            myDoubleAnimation2.From = 0;
            myDoubleAnimation2.To = this.PopupHeight;

            sb.Begin();
            //new Animation.SizeAnimation(ui.ActualWidth, ui.ActualHeight).Apply(ui);
        }
        #endregion Animation

    }

}
