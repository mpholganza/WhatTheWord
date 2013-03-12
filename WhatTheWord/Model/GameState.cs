using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace WhatTheWord.Model
{
	public class GameState
	{
		public const int GUESSPANEL_LETTER_REVEALED = 100;
		public const int GUESSPANEL_LETTER_NOT_GUESSED = 101;
		public const int CHARACTERPANEL_LETTER_REMOVED = 200;
		public const int CHARACTERPANEL_LETTER_GUESSED = 201;

		[DataMember]
		public int CurrentLevel { get; set; }

		[DataMember]
		public int Coins { get; set; }

		[DataMember]
		public String FBMessage { get; set; }

		[DataMember]
		public String PuzzleWord { get; set; }

		[DataMember]
		public String PuzzleCharacters { get; set; }

		[DataMember]
		public int[] GuessPanelState { get; set; }

		[DataMember]
		public int[] CharacterPanelState { get; set; }

		public GameState()
		{
			this.CurrentLevel = 0;
			this.Coins = 0;
			this.PuzzleWord = "";
			this.PuzzleCharacters = "";
			this.FBMessage = "";
			this.GuessPanelState = null;
			this.CharacterPanelState = null;
		}

		public async void LoadGameStateFromFile()
		{
			String gameData = await FileAccess.LoadDataFromFileAsync("ms-appdata:///local/gamestate.txt");
			GameState gs = new GameState();
			MemoryStream stream = new MemoryStream();
			DataContractJsonSerializer ser = new DataContractJsonSerializer(this.GetType());
			gs = ser.ReadObject(stream) as GameState;
			stream.Close();

			// TODO: This is the wrong way to do this
			if (null != gs)
			{
				this.CurrentLevel = gs.CurrentLevel;
				this.Coins = gs.Coins;
				this.PuzzleWord = gs.PuzzleWord;
				this.PuzzleCharacters = gs.PuzzleCharacters;
				this.FBMessage = gs.FBMessage;
				this.GuessPanelState = gs.GuessPanelState;
				this.CharacterPanelState = gs.CharacterPanelState;
			}
		}

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

		public void WriteGameDataToFile()
		{
			FileAccess.WriteDataToFileAsync(this.ToString(), "ms-appdata:///local/gamestate.txt");
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

		internal void Initialize(Puzzle CurrentPuzzle)
		{
			this.GuessPanelState = new int[CurrentPuzzle.Word.Length];
			for (int i = 0; i < CurrentPuzzle.Word.Length; i++)
			{
				GuessPanelState[i] = GameState.GUESSPANEL_LETTER_NOT_GUESSED;
			}

			this.PuzzleCharacters = CurrentPuzzle.GeneratePuzzleCharacters();
			this.CharacterPanelState = new int[this.PuzzleCharacters.Length];
			for (int i = 0; i < this.PuzzleCharacters.Length; i++)
			{
				CharacterPanelState[i] = i;
			}

		}

		internal void CheckAnswer()
		{
			throw new NotImplementedException();
		}
	}
}
