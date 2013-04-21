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
		public const string GAMESTATEFILE = "gamestate.txt";
		public const string GAMESTATEDEFAULTFILE = "gamestatedefault.txt";
		#endregion

        #region Gameplay variables
        public string FacebookToken { get; set; }
		public int CurrentLevel { get; set; }
		public bool PuzzleInitialized { get; set; }
		public int Coins { get; set; }
		public String PuzzleWord { get; set; }
		public String PuzzleCharacters { get; set; }
		public int[] GuessPanelState { get; set; }
		public int[] CharacterPanelState { get; set; }
		#endregion

		public GameState()
		{
			this.CurrentLevel = 1;
			this.Coins = 0;
			this.PuzzleWord = "";
			this.PuzzleCharacters = "";
			this.GuessPanelState = null;
			this.CharacterPanelState = null;
			this.FacebookToken = "";
		}

		public async void Load()
		{
			if (await LoadGameStateFromFile()) { return; }

			if (!LoadGameStateFromDefaultFile())
			{
				throw new ApplicationException("Unable to load game state information.");
			}
		}

		private bool LoadGameStateFromDefaultFile()
		{
			StreamResourceInfo sri = App.GetResourceStream(new Uri(GameState.GAMESTATEDEFAULTFILE, UriKind.Relative));
			StreamReader streamReader = new StreamReader(sri.Stream);
			string gameData = streamReader.ReadToEnd();
			try
			{
				Deserialize(gameData);
			}
			catch (ApplicationException)
			{
				// deserialized incorrectly. fail quietly
				// TODO: report to server of failed deserialization
				Console.WriteLine("Failed deserialization:\n" + gameData);
				return false;
			}

			return true;
		}

		private async Task<bool> LoadGameStateFromFile()
		{
			string gameData = await FileAccess.LoadDataFromFileAsync(GameState.GAMESTATEFILE);
			try
			{
				Deserialize(gameData);
			}
			catch (ApplicationException)
			{
				// deserialized incorrectly. fail quietly
				// TODO: report to server of failed deserialization
				Console.WriteLine("Failed deserialization:\n" + gameData);
				return false;
			}

			return true;
		}

		private void Deserialize(string gameData)
		{
			int currentlevel = 0;
			int coins = 0;
			int[] guesspanelstate = null;
			int[] characterpanelstate = null;
			string puzzleword = string.Empty;
			string puzzlecharacters = string.Empty;
			bool puzzleinitialized = false;
			string facebooktoken = string.Empty;

			string[] lines = gameData.Split(new string[] { "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			if (lines.Length < 1) { throw new ApplicationException("GameState data is empty."); }

			for (int i = 0; i < lines.Length; i++)
			{
				string key = string.Empty;
				string value = string.Empty;
				GameConfig.GetKeyValuePairFromString(lines[i], out key, out value);
				bool success = false;

				switch (key)
				{
					case "currentlevel":
						success = Int32.TryParse(value, out currentlevel);
						break;
					case "coins":
						success = Int32.TryParse(value, out coins);
						break;
					case "guesspanelstate":
						try
						{
							guesspanelstate = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
							success = true;
						}
						catch (Exception)
						{
							success = false;
						}
						break;
					case "characterpanelstate":
						try
						{
							characterpanelstate = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
							success = true;
						}
						catch (Exception)
						{
							success = false;
						}
						break;
					case "puzzleword":
						puzzleword = value;
						success = true;
						break;
					case "puzzlecharacters":
						puzzlecharacters = value;
						success = true;
						break;
					case "puzzleinitialized":
						success = bool.TryParse(value, out puzzleinitialized);
						break;
					case "facebooktoken":
						facebooktoken = value;
						success = true;
						break;
					default:
						throw new ApplicationException("Unknown GameState property: " + key);
				}

				if (!success)
				{
					throw new ApplicationException("Invalid GameState data. Trouble parsing " + key + ": " + value);
				}
			}

			this.CurrentLevel = currentlevel;
			this.Coins = coins;
			this.GuessPanelState = guesspanelstate;
			this.CharacterPanelState = characterpanelstate;
			this.PuzzleWord = puzzleword;
			this.PuzzleCharacters = puzzlecharacters;
			this.PuzzleInitialized = puzzleinitialized;
			this.FacebookToken = facebooktoken;
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
			// TODO: IMPORTANT BUG - CAN'T SHIP WITH THIS
			// if jumbled after removing letters removed letters can characters show up
			// replacing non-removed characters. this can make the puzzle unsolvable
			// should only jumble characters in the characterpanel
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
			PuzzleInitialized = false;
			WriteGameStateToFile();
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

			this.WriteGameStateToFile();
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

			this.WriteGameStateToFile();
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

		internal void InitializePuzzle(Puzzle CurrentPuzzle)
		{
			if (!this.PuzzleInitialized)
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

				this.PuzzleInitialized = true;
			}
		}
		#endregion

		public void WriteGameStateToFile()
		{
			FileAccess.WriteDataToFileAsync(this.ToString(), GameState.GAMESTATEFILE);
		}

		public override string ToString()
		{
			string gamePlayData = "currentlevel=" + this.CurrentLevel + Environment.NewLine +
				"coins=" + this.Coins + Environment.NewLine +
				"guesspanelstate=" + Utility.IntArrayToString(this.GuessPanelState, ",") + Environment.NewLine +
				"characterpanelstate=" + Utility.IntArrayToString(this.CharacterPanelState, ",") + Environment.NewLine +
				"puzzleword=" + this.PuzzleWord + Environment.NewLine +
				"puzzlecharacters=" + this.PuzzleCharacters + Environment.NewLine +
				"puzzleinitialized=" + this.PuzzleInitialized.ToString() + Environment.NewLine +
				"facebooktoken=" + this.FacebookToken;

			return gamePlayData;
		}
	}
}
