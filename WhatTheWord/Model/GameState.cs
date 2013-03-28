using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows.Resources;
using System.Net;
using Microsoft.Phone.Info;
using System.Threading;
using System.ComponentModel;

namespace WhatTheWord.Model
{
	public class GameState
	{

		#region Constants
		public const int GUESSPANEL_LETTER_REVEALED = 100;
		public const int GUESSPANEL_LETTER_NOT_GUESSED = 101;
		public const int CHARACTERPANEL_LETTER_REMOVED = 200;
		public const int CHARACTERPANEL_LETTER_GUESSED = 201;
		public const string GAMECONFIGFILE = "gameconfig.txt";
		public const string GAMESTATEFILE = "gamestate.txt";
		#endregion

        #region Gameplay variables
        public string FacebookToken { get; set; }
		public int CurrentLevel { get; set; }
		public int Coins { get; set; }
		public String PuzzleWord { get; set; }
		public String PuzzleCharacters { get; set; }
		public int[] GuessPanelState { get; set; }
		public int[] CharacterPanelState { get; set; }
		#endregion

		public GameState()
		{
            this.FacebookToken = "";
			this.CurrentLevel = 0;
			this.Coins = 0;
			this.PuzzleWord = "";
			this.PuzzleCharacters = "";
			this.GuessPanelState = null;
			this.CharacterPanelState = null;
		}

		public string GamePlayDataToString()
		{
			string gamePlayData = "currentLevel=" + this.CurrentLevel + Environment.NewLine +
				"coins=" + this.Coins + Environment.NewLine +
				"guesspanelstate=" + this.GuessPanelState.ToString() + Environment.NewLine +
				"characterpanelstate=" + this.CharacterPanelState.ToString() + Environment.NewLine +
				"puzzleword=" + this.PuzzleWord + Environment.NewLine +
				"puzzlecharacters=" + this.PuzzleCharacters + Environment.NewLine +
				"facebooktoken=" + this.FacebookToken;

			return gamePlayData;
		//public string FacebookToken { get; set; }
		//public int CurrentLevel { get; set; }
		//public int Coins { get; set; }
		//public String PuzzleWord { get; set; }
		//public String PuzzleCharacters { get; set; }
		//public int[] GuessPanelState { get; set; }
		//public int[] CharacterPanelState { get; set; }
		//public bool Loaded { get; set; }

		}

		#region Gameplay logic
		public int GetNextFreeGuessPanelIndex()
		{
			if (GuessPanelState != null)
			{
				for (int i = 0; i < GuessPanelState.Length; i++)
				{
					if (GuessPanelState[i] == GameState.GUESSPANEL_LETTER_NOT_GUESSED) return i;
				}
			}

			return -1;
		}

		internal void ClearGuessPanel()
		{
			// Clear guess panel
			for (int i = 0; i < GuessPanelState.Length; i++)
			{
				if (GuessPanelState[i] != GameState.GUESSPANEL_LETTER_REVEALED)
				{
					GuessPanelState[i] = GameState.GUESSPANEL_LETTER_NOT_GUESSED;
				}
			}

			// Clear character panel
			for (int i = 0; i < CharacterPanelState.Length; i++)
			{
				if (CharacterPanelState[i] != GameState.CHARACTERPANEL_LETTER_REMOVED)
				{
					CharacterPanelState[i] = i;
				}
			}
		}

		public void JumblePuzzleCharacters()
		{
			this.ClearGuessPanel();
			PuzzleCharacters = Puzzle.Jumble(PuzzleCharacters);
		}

		internal bool CheckAnswer()
		{
			for (int i = 0; i < GuessPanelState.Length; i++)
			{
				if (GuessPanelState[i] != GameState.GUESSPANEL_LETTER_REVEALED && PuzzleCharacters[GuessPanelState[i]] != PuzzleWord[i]) return false;
			}

			return true;
		}

		internal void CompleteLevel()
		{
			CurrentLevel++;
			Coins = Coins + 2;
			PuzzleWord = "";
			PuzzleCharacters = "";
			GuessPanelState = null;
			CharacterPanelState = null;
		}

		public void RevealLetter()
		{
			if (!this.CanRevealLetter())
			{
				// TODO: Report to server. This should never happen.
				Console.WriteLine("Called reveal letter when user shouldn't be able to.");
				return;
			}

			// Clear the guess panel
			this.ClearGuessPanel();

			// Pick a letter that hasn't been revealed yet
			int revealedLetters = 0;
			for (int i = 0; i < GuessPanelState.Length; i++)
			{
				if (GuessPanelState[i] == GUESSPANEL_LETTER_REVEALED) revealedLetters++;
			}

			Random random = new Random((int)DateTime.Now.Ticks);
			int revealIndex = Convert.ToInt32(Math.Floor(random.NextDouble() * (GuessPanelState.Length - revealedLetters)));
			int revealIndexOffset = 0;

			// Set it to revealed
			for (int i = 0; i <= revealIndex; i++)
			{
				while (GameState.GUESSPANEL_LETTER_REVEALED == GuessPanelState[i + revealIndexOffset]) { revealIndexOffset++; }
			}

			GuessPanelState[revealIndex + revealIndexOffset] = GameState.GUESSPANEL_LETTER_REVEALED;
		}

		public bool CanRevealLetter()
		{
			int revealedLetters = 0;
			for (int i = 0; i < GuessPanelState.Length; i++)
			{
				if (GuessPanelState[i] == GUESSPANEL_LETTER_REVEALED) revealedLetters++;
			}

			return (revealedLetters < this.GuessPanelState.Length - 1);
		}

		public void RemoveLetter()
		{
			if (!this.CanRemoveLetter())
			{
				// TODO: Report to server. This should never happen
				return;
			}

			// clear guess panel
			this.ClearGuessPanel();

			// Pick a letter that is not a part of the actual word
			List<char> removeCandidates = new List<char>();
			for (int i = 0; i < CharacterPanelState.Length; i++)
			{
				int candidate = CharacterPanelState[i];
				if (candidate != GameState.CHARACTERPANEL_LETTER_REMOVED)
				{
					removeCandidates.Add(PuzzleCharacters[candidate]);
				}
			}

			List<char> unremovableCharacters = this.PuzzleWord.ToList<char>();
			removeCandidates.Sort();
			unremovableCharacters.Sort();

			unremovableCharacters.ForEach(c => removeCandidates.Remove(c));

			Random random = new Random((int)DateTime.Now.Ticks);
			int removeIndex = Convert.ToInt32(Math.Floor(random.NextDouble() * removeCandidates.Count));
			char removeChar = removeCandidates[removeIndex];

			for (int i = 0; i < CharacterPanelState.Length; i++)
			{
				if (CharacterPanelState[i] != GameState.CHARACTERPANEL_LETTER_REMOVED && PuzzleCharacters[CharacterPanelState[i]] == removeChar)
				{
					// Set it to removed
					CharacterPanelState[i] = GameState.CHARACTERPANEL_LETTER_REMOVED;
					return;
				}
			}
		}

		public bool CanRemoveLetter()
		{
			// check that number of unremoved letters is more than the length of the word
			int removedLetters = 0;
			for (int i = 0; i < CharacterPanelState.Length; i++)
			{
				if (CharacterPanelState[i] == CHARACTERPANEL_LETTER_REMOVED) removedLetters++;
			}

			return (CharacterPanelState.Length - removedLetters > this.PuzzleWord.Length);
		}

		internal void Initialize(Puzzle CurrentPuzzle)
		{
			this.PuzzleWord = CurrentPuzzle.Word;
			this.GuessPanelState = new int[PuzzleWord.Length];
			for (int i = 0; i < PuzzleWord.Length; i++)
			{
				GuessPanelState[i] = GameState.GUESSPANEL_LETTER_NOT_GUESSED;
			}

			this.PuzzleCharacters = Puzzle.GeneratePuzzleCharacters(CurrentPuzzle.Word);
			this.CharacterPanelState = new int[this.PuzzleCharacters.Length];
			for (int i = 0; i < this.PuzzleCharacters.Length; i++)
			{
				CharacterPanelState[i] = i;
			}
		}
		#endregion

		public static void WriteGameConfigToFile(string gameData)
		{
			FileAccess.WriteDataToFileAsync(gameData, GameState.GAMECONFIGFILE);
		}

		public static void WriteGamePlayStateToFile(string gamePlayState)
		{
			FileAccess.WriteDataToFileAsync(gamePlayState, GameState.GAMECONFIGFILE);
		}

		public override String ToString()
		{
			MemoryStream stream = new MemoryStream();
			DataContractJsonSerializer ser = new DataContractJsonSerializer(this.GetType());
			ser.WriteObject(stream, this);
			byte[] json = stream.ToArray();
			stream.Close();
			return Encoding.UTF8.GetString(json, 0, json.Length);
		}
	}
}
