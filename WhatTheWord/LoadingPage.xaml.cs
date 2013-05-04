﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using WhatTheWord.Model;
using System.Threading;

namespace WhatTheWord
{
	public partial class LoadingPage : PhoneApplicationPage
	{
		public LoadingPage()
		{
			InitializeComponent();
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			await LoadGame();
			Thread.Sleep(800);
			NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
			NavigationService.RemoveBackEntry();
		}

		private async Task LoadGame()
		{
			App.Current.ConfigData = new GameConfig();
			await App.Current.ConfigData.Load();
			App.Current.StateData = new GameState();
			await App.Current.StateData.Load();
		}
	}
}