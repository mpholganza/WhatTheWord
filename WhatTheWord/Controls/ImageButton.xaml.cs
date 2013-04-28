using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;

namespace WhatTheWord.Controls
{
    public partial class ImageButton
    {
        #region Fields

        public new static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(ImageButton), null);
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(string), typeof(ImageButton), null);
        public static readonly DependencyProperty ImagePressedSourceProperty = DependencyProperty.Register("ImagePressedSource", typeof(string), typeof(ImageButton), null);
        public static readonly DependencyProperty ImageDisabledSourceProperty = DependencyProperty.Register("ImageDisabledSource", typeof(string), typeof(ImageButton), null);

        private BitmapImage _image;
        private BitmapImage _imagePressed;
        private BitmapImage _imageDisabled;
        private bool _isPressed;

        #endregion

        #region Constructor

        public ImageButton()
        {
            InitializeComponent();
            IsEnabled = true;
        }

        #endregion

        #region Properties

        public new bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }

            set
            {
                SetValue(IsEnabledProperty, value);

                SetImageFromState();
            }
        }

        public string ImageSource
        {
            get { return (string)GetValue(ImageSourceProperty); }

            set
            {
                SetValue(ImageSourceProperty, value);

                _image = SetImage(value);
                SetImageFromState();
            }
        }

        public string ImagePressedSource
        {
            get { return (string)GetValue(ImagePressedSourceProperty); }

            set
            {
                SetValue(ImagePressedSourceProperty, value);

                _imagePressed = SetImage(value);
                SetImageFromState();
            }
        }

        public string ImageDisabledSource
        {
            get { return (string)GetValue(ImageDisabledSourceProperty); }

            set
            {
                SetValue(ImageDisabledSourceProperty, value);

                _imageDisabled = SetImage(value);
                SetImageFromState();
            }
        }

        #endregion

        #region Event Handlers

        private void ButtonIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetImageFromState();
        }

        private void ButtonMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isPressed = true;

            SetImageFromState();
        }

        private void ButtonMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isPressed = false;

            SetImageFromState();
        }

        #endregion

        #region Private Methods

        private static BitmapImage SetImage(string imageSource)
        {
            BitmapImage bitmapImage = null;

            if (!string.IsNullOrEmpty(imageSource))
            {
                bitmapImage = new BitmapImage(new Uri(imageSource, UriKind.RelativeOrAbsolute));
            }

            return bitmapImage;
        }

        private void SetImageFromState()
        {
            if (!IsEnabled)
            {
                image.Source = _imageDisabled;
            }
            else if (_isPressed)
            {
                image.Source = _imagePressed;
            }
            else
            {
                image.Source = _image;
            }
        }

        #endregion
    }
}
