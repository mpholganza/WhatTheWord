using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
				// TODO: Look for file in LocalFolder first
				// if not in LocalFolder look in pre-packaged Assets folder
				Picture1.URI = "/Assets/PuzzlePictures/" + Picture1.URI.Replace("zip", "jpg");
				Picture2.URI = "/Assets/PuzzlePictures/" + Picture2.URI.Replace("zip", "jpg");
				Picture3.URI = "/Assets/PuzzlePictures/" + Picture3.URI.Replace("zip", "jpg");
				Picture4.URI = "/Assets/PuzzlePictures/" + Picture4.URI.Replace("zip", "jpg");
			}
			catch
			{
				return false;
			}

			return true;
			//CurrentPuzzle = new Puzzle()
			//{
			//	Word = "random".ToUpper(),
			//	Picture1 = new Picture { URI = "/Assets/PuzzlePictures/mangoes.png", Credits = "mph" },
			//	Picture2 = new Picture { URI = "/Assets/PuzzlePictures/icecream.png", Credits = "mph" },
			//	Picture3 = new Picture { URI = "/Assets/PuzzlePictures/water_houses.png", Credits = "mph" },
			//	Picture4 = new Picture { URI = "/Assets/PuzzlePictures/magnets.png", Credits = "mph" },
			//};
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
