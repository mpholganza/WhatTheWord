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
		private static DownloadManager downloadManagerInstance;

		private DownloadManager() { }

		public static DownloadManager GetInstance()
		{
			if (downloadManagerInstance == null)
			{
				downloadManagerInstance = new DownloadManager();
			}
			return downloadManagerInstance;
		}

		private Queue<KeyValuePair<string, string>> FilesToDownload = new Queue<KeyValuePair<string, string>>();

		public async Task<string> ListMissingFilesAndQueue(Dictionary<string, string> filesToDownload, string applicationPackagePath)
		{
			FilesToDownload.Clear();
			foreach (KeyValuePair<string, string> fileKVP in filesToDownload)
			{
				string fileName = fileKVP.Key;
				if (!await FileAccess.Exists(fileName, applicationPackagePath))
				{
					FilesToDownload.Enqueue(fileKVP);
				}
			}

			return GetFilesToDownload();
		}

		public async Task<string> ListMissingFiles(List<string> fileNames, string applicationPackagePath)
		{
			string missingFiles = string.Empty;
			foreach (string fileName in fileNames)
			{
				if (!await FileAccess.Exists(fileName, applicationPackagePath))
				{
					missingFiles += fileName + Environment.NewLine;
				}
			}
			return missingFiles;
		}

		public async Task DownloadMissingFiles(Dictionary<string, string> filesToDownload, string applicationPackagePath)
		{
			FilesToDownload.Clear();
			foreach (KeyValuePair<string, string> fileKVP in filesToDownload)
			{
				string fileName = fileKVP.Key;
				if (!await FileAccess.Exists(fileName, applicationPackagePath))
				{
					FilesToDownload.Enqueue(fileKVP);
				}
			}

			DownloadMissingFiles();
		}

		public async Task DownloadAndUnzipJpgFile(Dictionary<string, string> filesToDownload, string applicationPackagePath)
		{
			FilesToDownload.Clear();
			foreach (KeyValuePair<string, string> fileKVP in filesToDownload)
			{
				string fileName = fileKVP.Key;
				if (!await FileAccess.Exists(fileName, applicationPackagePath))
				{
					FilesToDownload.Enqueue(fileKVP);
				}
			}

			DownloadAndUnzipJpgFile();
		}

		public string GetFilesToDownload()
		{
			string filesToDownloadString = string.Empty;
			foreach (KeyValuePair<string, string> fileKVP in FilesToDownload)
			{
				filesToDownloadString += fileKVP.Key + Environment.NewLine;
			}

			return filesToDownloadString;
		}

		public void DownloadMissingFiles()
		{
			if (FilesToDownload.Count == 0)
			{
				return;
			}

			KeyValuePair<string, string> fileKVP = FilesToDownload.Peek();

			string fileName = fileKVP.Key;
			string fileUrl = fileKVP.Value;
			if (fileName == null || fileUrl == null)
			{
				FilesToDownload.Dequeue();
				DownloadMissingFiles();
			}
			else
			{
				Uri uri = new Uri(fileUrl);
				WebClient client = new WebClient();
				client.OpenReadCompleted += client_OpenReadCompleted_SaveStreamToFile;
				client.OpenReadAsync(uri);
			}
		}

		private async void client_OpenReadCompleted_SaveStreamToFile(object sender, OpenReadCompletedEventArgs e)
		{
			if (e.Error == null)
			{
				Stream inStream = e.Result;
				string fileName = FilesToDownload.Dequeue().Key;
				await FileAccess.WriteStreamToFileAsync(inStream, fileName);
			}

			DownloadMissingFiles();
		}

		public void DownloadAndUnzipJpgFile()
		{
			if (FilesToDownload.Count == 0)
			{
				return;
			}

			KeyValuePair<string, string> fileKVP = FilesToDownload.Peek();
			string fileName = fileKVP.Key;
			string fileUrl = fileKVP.Value;
			if (fileName == null || fileUrl == null)
			{
				FilesToDownload.Dequeue();
				DownloadAndUnzipJpgFile();
			}
			else
			{
				Uri uri = new Uri(fileUrl);
				WebClient client = new WebClient();
				client.OpenReadCompleted += client_OpenReadCompleted_UnzipStreamAndSaveJpg;
				client.OpenReadAsync(uri);
			}
		}

		private async void client_OpenReadCompleted_UnzipStreamAndSaveJpg(object sender, OpenReadCompletedEventArgs e)
		{
			if (e.Error == null)
			{
				Stream zipPackageStream = e.Result;
				string fileName = FilesToDownload.Dequeue().Key.Replace("zip", "jpg");
				StreamResourceInfo zipSri = new StreamResourceInfo(zipPackageStream, null);
				Uri uri = new Uri(fileName, UriKind.Relative);
				StreamResourceInfo zippedFileSri = App.GetResourceStream(zipSri, uri);
				await FileAccess.WriteStreamToFileAsync(zippedFileSri.Stream, fileName);
			}

			DownloadAndUnzipJpgFile();
		}
	}
}
