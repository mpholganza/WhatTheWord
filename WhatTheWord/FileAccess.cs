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

			using (Stream stream = await file.OpenStreamForWriteAsync())
			{
				byte[] content = Encoding.UTF8.GetBytes(data);
				await stream.WriteAsync(content, 0 , content.Length);
			}
		}
	}
}
