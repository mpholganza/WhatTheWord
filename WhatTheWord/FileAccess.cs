using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;
using Windows.Storage;

namespace WhatTheWord
{
	class FileAccess
	{
		public async static Task<String> LoadDataFromFileAsync(string fileName)
		{
			String data = String.Empty;
			try
			{
				StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
				using (Stream stream = await file.OpenStreamForReadAsync())
				{
					byte[] content = new byte[stream.Length];
					await stream.ReadAsync(content, 0, (int)stream.Length);
					data = Encoding.UTF8.GetString(content, 0, content.Length);
				}
			}
			catch (FileNotFoundException) {}

			return data;
		}

		public async static void WriteDataToFileAsync(string data, string fileName)
		{
			StorageFile file = null;
			try
			{
				file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
			}
			catch (Exception e)
			{
				Console.WriteLine("Error writing to file" + e.Message);
			}

			using (Stream stream = await file.OpenStreamForWriteAsync())
			{
				byte[] content = Encoding.UTF8.GetBytes(data);
				await stream.WriteAsync(content, 0 , content.Length);
			}
		}

		public async static Task<bool> Exists(string fileName, string appPackagePath)
		{
			if (await ExistsInLocalFolder(fileName))
			{
				return true;
			}

			if (ExistsInApplicationPackage(appPackagePath + "/" + fileName))
			{
				return true;
			}

			return false;
		}

		public static bool ExistsInApplicationPackage(string filePath)
		{
			// Check if file exists in the application package directory
			Uri uri = new Uri(filePath, UriKind.Relative);
			return (null != App.GetResourceStream(uri));
		}

		public async static Task<bool> ExistsInLocalFolder(string fileName)
		{
			StorageFile file = null;
			// Check if file exists as in LocalFolder
			try
			{
				file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
				return true;
			}
			catch { }
			return false;
		}
	}
}
