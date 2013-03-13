using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WhatTheWord
{
	public partial class WinPage : PhoneApplicationPage
	{
		public WinPage()
		{
			InitializeComponent();
			NextPuzzle.Click += NextPuzzle_Click;
		}

		void NextPuzzle_Click(object sender, RoutedEventArgs e)
		{
			NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
		}
	}
}