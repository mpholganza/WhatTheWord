using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;
using Windows.Storage;
using Windows.Storage.Search;

namespace WhatTheWord
{
	public class FileAccess
	{
		public async static Task<string> LoadDataFromFileAsync(string fileName)
		{
			string data = string.Empty;
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
			catch (FileNotFoundException) { }

			return data;
		}

		public async static void WriteDataToFileAsync(string data, string fileName)
		{
			StorageFile file = null;
			try
			{
				file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
				using (Stream stream = await file.OpenStreamForWriteAsync())
				{
					byte[] content = Encoding.UTF8.GetBytes(data);
					await stream.WriteAsync(content, 0, content.Length);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Error writing to file" + e.Message);
			}
		}

		public async static Task WriteStreamToFileAsync(Stream inStream, string fileName)
		{
			StorageFile file = null;
			try
			{
				file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
				using (Stream outStream = await file.OpenStreamForWriteAsync())
				{
					byte[] data = new byte[16 * 1024];
					int bytesRead;
					while ((bytesRead = await inStream.ReadAsync(data, 0, data.Length)) > 0)
					{
						await outStream.WriteAsync(data, 0, bytesRead);
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Error writing to file" + e.Message);
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

		public async static Task<List<String>> ListFilesInLocalFolder()
		{
			List<string> fileNames = new List<string>();
			IReadOnlyList<StorageFile> storageItems = await ApplicationData.Current.LocalFolder.GetFilesAsync();
			foreach (StorageFile storageItem in storageItems)
			{
				fileNames.Add(storageItem.Name);
			}

			return fileNames;
		}

		public async static Task<bool> Delete(string fileName)
		{
			StorageFile file = null;
			try
			{
				file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
				await file.DeleteAsync();
			}
			catch
			{
				return false;
			}
			return true;
		}
	}
}
