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
		public async static Task<String> LoadDataFromFileAsync(String fileName)
		{
			String data = String.Empty;
			try
			{
				Stream stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(fileName);
				using (StreamReader reader = new StreamReader(stream))
				{
					data = await reader.ReadToEndAsync();
				}
			}
			catch (FileNotFoundException) {}

			return data;
		}

		public async static void WriteDataToFileAsync(String data, String fileName)
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

			Stream stream = await file.OpenStreamForWriteAsync();
			using (StreamWriter writer = new StreamWriter(stream))
			{
				await writer.WriteAsync(data);
			}
		}
	}
}
