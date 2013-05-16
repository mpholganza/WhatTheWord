using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;
using Windows.Storage;

namespace WhatTheWord.Model
{
	public class Puzzle
	{
		public const int MAX_WORD_LENGTH = 8;
		public const int GUESS_ARRAY_LENGTH = 12;

		public String Word { get; set; }
		public Picture Picture1 { get; set; }
		public Picture Picture2 { get; set; }
		public Picture Picture3 { get; set; }
		public Picture Picture4 { get; set; }

		public int Id { get; set; }
		public bool Enabled { get; set; }
		public int Order { get; set; }

		public bool TryLoad()
		{
			try
			{
				// if not in LocalFolder look in pre-packaged Assets folder
				Picture1.URI = GetPictureUri(Picture1.URI);
				Picture2.URI = GetPictureUri(Picture2.URI);
				Picture3.URI = GetPictureUri(Picture3.URI);
				Picture4.URI = GetPictureUri(Picture4.URI);
			}
			catch
			{
				return false;
			}

			return true;
		}

		private string GetPictureUri(string originalURI)
		{
			string pictureUri = originalURI.Replace("zip", "jpg");
			if (App.Current.LocalFolderFiles.Contains(pictureUri))
			{
				pictureUri = "ms-appdata:///local/" + pictureUri;
			}
			else
			{
				pictureUri = App.PuzzlePicturesPath + pictureUri;
				StreamResourceInfo sri = App.GetResourceStream(new Uri(pictureUri, UriKind.Relative));
				if (sri == null)
				{
					throw new Exception(pictureUri + " missing.");
				}
			}
			return pictureUri;
		}

		public static String GeneratePuzzleCharacters(String word)
		{
			if (word.Length > MAX_WORD_LENGTH) throw new ApplicationException("Word is too long");
			StringBuilder builder = new StringBuilder();
			Random random = new Random((int)DateTime.Now.Ticks);

			for (int i = word.Length; i < Puzzle.GUESS_ARRAY_LENGTH; i++)
			{
				builder.Append(Convert.ToChar(Convert.ToInt32(Math.Floor(random.NextDouble() * 26 + 65))));
			}

			String orderedCharacters = word + builder.ToString();

			return Jumble(orderedCharacters);
		}

		public static String Jumble(String orderedCharacters)
		{
			Random random = new Random((int)DateTime.Now.Ticks);
			
			String jumbledCharacters = String.Empty;
			for (int i = 0; i < orderedCharacters.Length; i++)
			{
				int insertIndex = Convert.ToInt32(Math.Floor(random.NextDouble() * i));
				jumbledCharacters = jumbledCharacters.Insert(insertIndex, orderedCharacters[i].ToString());
			}

			return jumbledCharacters;
		}
	}
}
