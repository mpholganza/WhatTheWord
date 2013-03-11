using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace WhatTheWord
{
	class FileAccess
	{
		public async static Task<String> LoadGameStateFromLocalFolderAsync()
		{
			String data = String.Empty;
			StorageFile file = null;
			bool exists = false;
			try
			{
				file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(
					new Uri("ms-appdata:///local/gamestate.txt"));
				exists = true;
			}
			catch (FileNotFoundException) {}

			if (exists)
			{
				Stream stream = await file.OpenStreamForReadAsync();
				using (StreamReader reader = new StreamReader(stream))
				{
					data = await reader.ReadToEndAsync();
				}
			}

			return data;
		}

		public async static void WriteGameStateToLocalFolderAsync(String data)
		{
			StorageFile file = null;
			bool exists = false;
			try
			{
				file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(
					new Uri("ms-appdata:///local/gamestate.txt"));
				exists = true;
			}
			catch (FileNotFoundException)
			{
				exists = false;
			}

			if (!exists)
			{
				file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
					"ms-appdata:///local/gamestate.txt", CreationCollisionOption.OpenIfExists);
			}

			Stream stream = await file.OpenStreamForWriteAsync();
			using (StreamWriter writer = new StreamWriter(stream))
			{
				await writer.WriteAsync(data);
			}
		}
	}
}
