using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace WhatTheWord
{
	public class DownloadManager
	{

		public async static void DownloadMissingFiles(List<string> filesToDownload, string sourceUri)
		{
			foreach (string fileName in filesToDownload)
			{
				if (!await FileAccess.Exists(fileName, "Assets/PuzzlePictures"))
				{
					Uri uri = new Uri(sourceUri + fileName); // TODO: Check to ensure sourceUri has a trailiing "/" for now we assume it's there
					WebClient client = new WebClient();
					client.OpenReadCompleted += client_OpenReadCompleted;
					client.OpenReadAsync(uri);
				}
			}
		}

		private async static void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
		{
			Stream zipPackageStream = e.Result;
			Image image = await LoadImageFromZipPackage("test.jpg", zipPackageStream);
			
		}

		public async static Task<Image> LoadImageFromZipPackage(string relativeUri, Stream zipPackageStream)
		{
			StreamResourceInfo sri = new StreamResourceInfo(zipPackageStream, null);
			Uri uri = new Uri(relativeUri, UriKind.Relative);
			StreamResourceInfo imageSri = App.GetResourceStream(sri, uri);

			Stream output;
			//await imageSri.Stream.CopyToAsync(output);

			//// Convert the stream to an Image.
			//BitmapImage bi = new BitmapImage();
			//bi.SetSource(imageSri.Stream);
			//Image img = new Image();
			//img.Source = bi;

			return new Image();
		}
	}
}
