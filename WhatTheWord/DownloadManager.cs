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

		public event EventHandler<FileDownloadedEventArgs> FileDownloaded;
		public bool InProgress { get; set; }
		private Queue<KeyValuePair<string, string>> FilesToDownload = new Queue<KeyValuePair<string, string>>();

		#region Constructors
		private DownloadManager() { }
		public static DownloadManager GetInstance()
		{
			if (downloadManagerInstance == null)
			{
				downloadManagerInstance = new DownloadManager() { InProgress = false };
			}
			return downloadManagerInstance;
		}
		#endregion

		#region Unused
		public async Task<string> ListMissingFilesAndQueue(Dictionary<string, string> filesToDownload, string applicationPackagePath)
		{
			FilesToDownload.Clear();

			List<string> filesInLocalFolder = await FileAccess.ListFilesInLocalFolder();
			foreach (KeyValuePair<string, string> fileKVP in filesToDownload)
			{
				string fileName = fileKVP.Key;
				//if (!await FileAccess.Exists(fileName, applicationPackagePath))
				//{
				//	FilesToDownload.Enqueue(fileKVP);
				//}
				if (-1 == filesInLocalFolder.IndexOf(fileName))
				{
					FilesToDownload.Enqueue(fileKVP);
				}
			}

			return GetFilesToDownload();
		}

		public async Task<string> ListMissingFiles(List<string> fileNames, string applicationPackagePath)
		{
			string missingFiles = string.Empty;
			List<string> filesInLocalFolder = await FileAccess.ListFilesInLocalFolder();

			foreach (string fileName in fileNames)
			{
				//if (!await FileAccess.Exists(fileName, applicationPackagePath))
				//{
				//	missingFiles += fileName + Environment.NewLine;
				//}
				if (-1 == filesInLocalFolder.IndexOf(fileName))
				{
					missingFiles += fileName + Environment.NewLine;
				}
			}
			return missingFiles;
		}

		public async Task DownloadMissingFiles(Dictionary<string, string> filesToDownload, string applicationPackagePath)
		{
			FilesToDownload.Clear();

			List<string> filesInLocalFolder = await FileAccess.ListFilesInLocalFolder();
			foreach (KeyValuePair<string, string> fileKVP in filesToDownload)
			{
				string fileName = fileKVP.Key;
				//if (!await FileAccess.Exists(fileName, applicationPackagePath))
				//{
				//	FilesToDownload.Enqueue(fileKVP);
				//}
				if (-1 == filesInLocalFolder.IndexOf(fileName))
				{
					FilesToDownload.Enqueue(fileKVP);
				}
			}

			DownloadMissingFiles();
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

		private string GetFilesToDownload()
		{
			string filesToDownloadString = string.Empty;
			foreach (KeyValuePair<string, string> fileKVP in FilesToDownload)
			{
				filesToDownloadString += fileKVP.Key + Environment.NewLine;
			}

			return filesToDownloadString;
		}
		#endregion

		public void DownloadAndUnzipJpgFiles(Dictionary<string, string> filesToDownload)
		{
			FilesToDownload.Clear();
			foreach (KeyValuePair<string, string> fileKVP in filesToDownload)
			{
				FilesToDownload.Enqueue(fileKVP);
			}

			if (FilesToDownload.Count > 0) { InProgress = true; }
			DownloadAndUnzipJpgFile();
		}

		public void DownloadAndUnzipJpgFile()
		{
			if (FilesToDownload.Count == 0)
			{
				InProgress = false;
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
				if (FilesToDownload.Count == 0)
				{
					return;
				}

				Stream zipPackageStream = e.Result;
				string fileName = FilesToDownload.Peek().Key.Replace("zip", "jpg");
				StreamResourceInfo zipSri = new StreamResourceInfo(zipPackageStream, null);
				Uri uri = new Uri(fileName, UriKind.Relative);
				StreamResourceInfo zippedFileSri = App.GetResourceStream(zipSri, uri);
				if (zippedFileSri == null)
				{
					// Can't find zipped jpg file with the same name as the next in the queue
					// This is either a bad resource or the callback from a cancelled download
					return;
				}
				FilesToDownload.Dequeue();
				await FileAccess.WriteStreamToFileAsync(zippedFileSri.Stream, fileName);

				FileDownloadedEventArgs args = new FileDownloadedEventArgs();	
				args.FilesLeftCount = FilesToDownload.Count();
				OnFileDownloadedEvent(args);
			}

			DownloadAndUnzipJpgFile();
		}

		protected virtual void OnFileDownloadedEvent(FileDownloadedEventArgs e)
		{
			EventHandler<FileDownloadedEventArgs> handler = FileDownloaded;
			if (handler != null)
			{
				handler(this, e);
			}
		}
	}

	public class FileDownloadedEventArgs : EventArgs
	{
		public int FilesLeftCount { get; set; }
	}
}
