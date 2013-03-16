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

        UIElement _screenshotUI;
        WriteableBitmap _bitmap;

        public double HostWindowWidth { get; set; }
        public double HostWindowHeight { get; set; }

        public double PopupWidth { get; set; }
        public double PopupHeight { get; set; }

        public FacebookUserControl(UIElement screenshotUI, double hostWindowWidth, double hostWindowHeight)
        {
            InitializeComponent();

            _screenshotUI = screenshotUI;

            this.HostWindowWidth = hostWindowWidth;
            this.HostWindowHeight = hostWindowHeight;

            this.PopupWidth = this.HostWindowWidth - 24;
            this.PopupHeight = Math.Min(this.HostWindowHeight - 120, 800);

            this.Visibility = System.Windows.Visibility.Collapsed;
            webBrowser1.Visibility = System.Windows.Visibility.Collapsed;
            SharePanel.Visibility = System.Windows.Visibility.Collapsed;
        }

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

        private void webBrowser1_Loaded(object sender, RoutedEventArgs e)
        {
            var loginUrl = GetFacebookLoginUrl(AppId, ExtendedPermissions);
            webBrowser1.Navigate(loginUrl);
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

        private void webBrowser1_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            FacebookOAuthResult oauthResult;
            if (!_fb.TryParseOAuthCallbackUrl(e.Uri, out oauthResult))
            {
                fadeIn(this);
                SharePanel.Visibility = System.Windows.Visibility.Collapsed;
                this.Visibility = System.Windows.Visibility.Visible;
                webBrowser1.Visibility = System.Windows.Visibility.Visible;
                return;
            }

            if (oauthResult.IsSuccess)
            {
                _accessToken = oauthResult.AccessToken;
                webBrowser1.Visibility = System.Windows.Visibility.Collapsed;
                this.Visibility = System.Windows.Visibility.Visible;
                SharePanel.Visibility = System.Windows.Visibility.Visible;

                TakeScreenshot();
                //LoginSucceded(accessToken);
            }
            else
            {
                // user cancelled
                MessageBox.Show(oauthResult.ErrorDescription);
            }
        }

        private void TakeScreenshot()
        {
            var fileName = String.Format("atomic_{0:}.jpg", DateTime.Now.Ticks);
            _bitmap = new WriteableBitmap((int)Application.Current.Host.Content.ActualWidth, (int)Application.Current.Host.Content.ActualHeight);

            _bitmap.Render(_screenshotUI, new System.Windows.Media.MatrixTransform());
            _bitmap.Invalidate();

            Screenshot.Source = _bitmap;

            //SaveToMediaLibrary(bmpCurrentScreenImage, fileName, 100);
            //MessageBox.Show("Captured image " + fileName + " Saved Sucessfully", "WmDev Capture Screen", MessageBoxButton.OK);
        }

        public void SaveToMediaLibrary(WriteableBitmap bitmap, string name, int quality)
        {
            using (var stream = new MemoryStream())
            {
                // Save the picture to the Windows Phone media library.
                bitmap.SaveJpeg(stream, bitmap.PixelWidth, bitmap.PixelHeight, 0, quality);
                stream.Seek(0, SeekOrigin.Begin);
                new Microsoft.Xna.Framework.Media.MediaLibrary().SavePicture(name, stream);
            }
        }

        private void PostButton_Click_1(object sender, RoutedEventArgs routedEventArgs)
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

                //var url = string.Format("/Pages/FacebookInfoPage.xaml?access_token={0}&id={1}", accessToken, id);

                //Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri(url, UriKind.Relative)));
            };

            // Encode to JPEG format
            byte[] screenshot;
            var parameters = new Dictionary<string, object>();
            using (var stream = new MemoryStream())
            {

                // Save the picture to the Windows Phone media library.
                _bitmap.SaveJpeg(stream, _bitmap.PixelWidth, _bitmap.PixelHeight, 0, 100);
                stream.Seek(0, SeekOrigin.Begin);
                screenshot = ConvertToByteArray(stream);
            }

            parameters["message"] = Message.Text;
            parameters["file"] = new FacebookMediaObject
            {
                ContentType = "image/jpeg",
                FileName = "image.jpeg"
            }.SetValue(screenshot);

            //fb.PostAsync("me/feed", parameters);
            fb.PostAsync("me/photos", parameters);
        }

        /// Converts to byte array.
        /// 
        /// "fileStream">The file stream.
        /// 
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


        ////private void LoginSucceded(string accessToken)
        ////{
        ////    var fb = new FacebookClient(accessToken);

        ////    fb.GetCompleted += (o, e) =>
        ////    {
        ////        if (e.Error != null)
        ////        {
        ////            Dispatcher.BeginInvoke(() => MessageBox.Show(e.Error.Message));
        ////            return;
        ////        }

        ////        var result = (IDictionary<string, object>)e.GetResultData();
        ////        var id = (string)result["id"];

        ////        var url = string.Format("/Pages/FacebookInfoPage.xaml?access_token={0}&id={1}", accessToken, id);

        ////        Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri(url, UriKind.Relative)));
        ////    };

        ////    fb.GetAsync("me?fields=id");
    }
}
