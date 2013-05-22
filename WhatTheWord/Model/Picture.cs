using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using Windows.Storage;

namespace WhatTheWord
{
	public class Picture
	{
		public string Path { get; set; }
		public Uri URI { get; set; }
		public string Credits { get; set; }
		public BitmapImage ImageSource { get; set; }
		public bool Loaded { get; set; }
		public bool LoadFromAppPackage { get; set; }
		public bool Load()
		{
			LoadFromAppPackage = true;
			try
			{
				if (string.IsNullOrEmpty(Path)) throw new Exception("Invalid empty path.");
				string pictureUri = Path.Replace("zip", "jpg");
				if (App.Current.LocalFolderFiles.Contains(pictureUri))
				{
					StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
					string localPath = local.Path;
					URI = new Uri(localPath + "\\" + pictureUri);
					LoadFromAppPackage = false;
				}
				else
				{
					URI = new Uri(App.PuzzlePicturesPath + pictureUri, UriKind.Relative);
					StreamResourceInfo sri = App.GetResourceStream(URI);
					if (sri == null)
					{
						throw new Exception(pictureUri + " missing.");
					}
				}
				ImageSource = new BitmapImage(URI);
				Loaded = true;
			}
			catch
			{
				Loaded = false;
			}

			return Loaded;
		}
	}
}
