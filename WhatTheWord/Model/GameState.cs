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
		#region Constants
		public const int GUESSPANEL_LETTER_REVEALED = 100;
		public const int GUESSPANEL_LETTER_NOT_GUESSED = 101;
		public const int CHARACTERPANEL_LETTER_REMOVED = 200;
		public const int CHARACTERPANEL_LETTER_GUESSED = 201;
		#endregion

		#region Config variables
		public int initialCoins { get; set; }
		public double boostRemoveLettersCost { get; set; }
		public int boostRemoveLettersNumberOfLetters { get; set; }
		public double boostRevealLetterCost { get; set; }
		public double boostShuffleCost { get; set; }
		public int rewardCoinsPerQuestion { get; set; }
		public int rateMeReward { get; set; }
		public int rateMeShowInitial { get; set; }
		public int rateMeShowReminderInterval { get; set; }
		public int boostBounceTimeInterval { get; set; }
		public int picsFailedDownloadWait { get; set; }
		public int picsSuccessDownloadWait { get; set; }
		public string rateMeURL { get; set; }
		public string picturesFilenamePath { get; set; }
		public Dictionary<string,InAppPurchase> Purchases { get; set; }
		public List<Puzzle> Puzzles { get; set; }
		#endregion

		#region Gameplay variables
		public int CurrentLevel { get; set; }
		public int Coins { get; set; }
		public String PuzzleWord { get; set; }
		public String PuzzleCharacters { get; set; }
		public int[] GuessPanelState { get; set; }
		public int[] CharacterPanelState { get; set; }
		#endregion

		public GameState()
		{
			this.CurrentLevel = 0;
			this.Coins = 0;
			this.PuzzleWord = "";
			this.PuzzleCharacters = "";
			this.GuessPanelState = null;
			this.CharacterPanelState = null;
            this.Purchases = new Dictionary<string, InAppPurchase>();
		}

		public async void LoadGameStateFromFile()
		{
			//String gameData = await FileAccess.LoadDataFromFileAsync("gamestate.txt");
			String gameData = Resources.AppResources.GameStateDefault;
			DeserializeGameData(gameData);
		}

		public void DeserializeJsonFile(string gameData)
		{
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
				this.GuessPanelState = gs.GuessPanelState;
				this.CharacterPanelState = gs.CharacterPanelState;
			}
		}

		public Puzzle getCurrentPuzzle()
		{
			return Puzzles.Find(i => i.Order == CurrentLevel);
		}

		#region Parse Functions
		public void DeserializeGameData(string gameData)
		{
			string[] lines = gameData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			if (lines.Length < 3) { throw new ApplicationException("Gamedata File is empty"); }

			string key = string.Empty;
			string value = string.Empty;

			// Check status and file integrity
			string[] statusLine = lines[0].Split(new string[] { ";;;" }, StringSplitOptions.None);
			GetKeyValuePairFromString(statusLine[0], out key, out value);
			if (key != "status" || value != "ok") { throw new ApplicationException("Corrupt Gamedata file"); }

			// Check flight hash - should be the same for the second and last lines
			string[] flightLine1 = lines[1].Split(new string[] { ";;;" }, StringSplitOptions.None);
			GetKeyValuePairFromString(flightLine1[0], out key, out value);
			if (key != "flight") { throw new ApplicationException("Corrupt Gamedata file"); }
			string flightHash = value;

			string[] flightLine2 = lines[lines.Length - 1].Split(new string[] { ";;;" }, StringSplitOptions.None);
			GetKeyValuePairFromString(flightLine2[0], out key, out value);
			if (key != "flight") { throw new ApplicationException("Corrupt Gamedata file"); }

			if (flightHash != value) { throw new ApplicationException("Corrupt Gamedata file"); }

			for (int i = 2; i < lines.Length - 1; i++)
			{
				string dataType = string.Empty;
				string dataValue = string.Empty;
				GetDataTypeAndDataValue(lines[i], out dataType, out dataValue);

				switch (dataType)
				{
					case "config":
						parseConfigString(dataValue);
						break;
					case "puzzles":
						parsePuzzleString(dataValue);
						break;
					case "iap":
						parseInAppPurchase(dataValue);
						break;
					default:
						throw new ApplicationException("Invalid line in Gamedata file:" + lines[i]);
				}
			}
		}

		private void parseConfigString(string configInfo)
		{
			string[] kvps = configInfo.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
			if (kvps.Length < 2) { throw new ApplicationException("Missing data in Gamedata config info" ); }

			string key = string.Empty;
			string keyName = string.Empty;
			string value = string.Empty;

			// Get key values
			GetKeyValuePairFromString(kvps[0], out key, out keyName);
			if (key != "key") { throw new ApplicationException("Corrupt Gamedata config info"); }

			GetKeyValuePairFromString(kvps[1], out key, out value);
			if (key != "value") { throw new ApplicationException("Corrupt Gamedata config info"); }

			bool success = false;
			int initialCoins = 0;
			double boostRemoveLettersCost = 0;
			int boostRemoveLettersNumberOfLetters = 0;
			double boostRevealLetterCost = 0;
			double boostShuffleCost = 0;
			int rewardCoinsPerQuestion = 0;
			int rateMeReward = 0;
			int rateMeShowInitial = 0;
			int rateMeShowReminderInterval = 0;
			int boostBounceTimeInterval = 0;
			int picsFailedDownloadWait = 0;
			int picsSuccessDownloadWait = 0;
			string rateMeURL = string.Empty;
			string picturesFilenamePath = string.Empty;

			switch (keyName)
			{
				case "initialCoins":
					success = int.TryParse(value, out initialCoins);
					break;
				case "boostRemoveLettersCost":
					success = double.TryParse(value, out boostRemoveLettersCost);
					break;
				case "boostRemoveLettersNumberOfLetters":
					success = int.TryParse(value, out boostRemoveLettersNumberOfLetters);
					break;
				case "boostRevealLetterCost":
					success = double.TryParse(value, out boostRevealLetterCost);
					break;
				case "boostShuffleCost":
					success = double.TryParse(value, out boostShuffleCost);
					break;
				case "rewardCoinsPerQuestion":
					success = int.TryParse(value, out rewardCoinsPerQuestion);
					break;
				case "rateMeReward":
					success = int.TryParse(value, out rateMeReward);
					break;
				case "rateMeShowInitial":
					success = int.TryParse(value, out rateMeShowInitial);
					break;
				case "rateMeShowReminderInterval":
					success = int.TryParse(value, out rateMeShowReminderInterval);
					break;
				case "boostBounceTimeInterval":
					success = int.TryParse(value, out boostBounceTimeInterval);
					break;
				case "picsFailedDownloadWait":
					success = int.TryParse(value, out picsFailedDownloadWait);
					break;
				case "picsSuccessDownloadWait":
					success = int.TryParse(value, out picsSuccessDownloadWait);
					break;
				case "rateMeURL":
					rateMeURL = value;
					success = true;
					break;
				case "picturesFilenamePath":
					picturesFilenamePath = value;
					success = true;
					break;
				default:
					throw new ApplicationException("Unknown Gatedata config key: " + keyName);
			}

			if (!success)
			{
				throw new ApplicationException("Invalid Gatedata config info. Trouble parsing " + keyName + ": " + value);
			}

			// Only doing it this way because TryParse can't write to instance variables :(
			this.initialCoins = initialCoins;
			this.boostRemoveLettersCost = boostRemoveLettersCost;
			this.boostRemoveLettersNumberOfLetters = boostRemoveLettersNumberOfLetters;
			this.boostRevealLetterCost = boostRevealLetterCost;
			this.boostShuffleCost = boostShuffleCost;
			this.rewardCoinsPerQuestion = rewardCoinsPerQuestion;
			this.rateMeReward = rateMeReward;
			this.rateMeShowInitial = rateMeShowInitial;
			this.rateMeShowReminderInterval = rateMeShowReminderInterval;
			this.boostBounceTimeInterval = boostBounceTimeInterval;
			this.picsFailedDownloadWait = picsFailedDownloadWait;
			this.picsSuccessDownloadWait = picsSuccessDownloadWait;
			this.rateMeURL = rateMeURL;
			this.picturesFilenamePath = picturesFilenamePath;
		}

		private void parsePuzzleString(string puzzleInfo)
		{
			List<Puzzle> puzzles = new List<Puzzle>();
			string[] kvps = puzzleInfo.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < kvps.Length; i++)
			{
				string key = string.Empty;
				string value = string.Empty;
				GetKeyValuePairFromString(kvps[i], out key, out value);

				int id = 0;
				string answer = string.Empty;
				bool enable = false;
				int order = 0;
				string filename1 = string.Empty;
				string credits1 = string.Empty;
				string filename2 = string.Empty;
				string credits2 = string.Empty;
				string filename3 = string.Empty;
				string credits3 = string.Empty;
				string filename4 = string.Empty;
				string credits4 = string.Empty;

				bool success = false;
				switch (key)
				{
					case "id":
						success = (int.TryParse(value, out id));
						break;
					case "answer":
						answer = value;
						success = true;
						break;
					case "enable":
						if (value == "1") { enable = true; success = true; }
						break;
					case "order":
						success = (int.TryParse(value, out order));
						break;
					case "filename1":
						filename1 = value;
						success = true;
						break;
					case "credits1":
						credits1 = value;
						success = true;
						break;
					case "filename2":
						filename2 = value;
						success = true;
						break;
					case "credits2":
						credits2 = value;
						success = true;
						break;
					case "filename3":
						filename3 = value;
						success = true;
						break;
					case "credits3":
						credits3 = value;
						success = true;
						break;
					case "filename4":
						filename4 = value;
						success = true;
						break;
					case "credits4":
						credits4 = value;
						success = true;
						break;
					default:
						throw new ApplicationException("Unknown Gatedata puzzle property: " + key);
				}

				if (!success)
				{
					throw new ApplicationException("Invalid Gamedata puzzle info. Trouble parsing " + key + ": " + value);
				}

				puzzles.Add(new Puzzle() {
					Word = answer.ToUpper(), // TODO: Fix constructor to do the ToUpper() instead of here
					Picture1 = new Picture { URI = filename1, Credits = credits1 },
					Picture2 = new Picture { URI = filename2, Credits = credits2 },
					Picture3 = new Picture { URI = filename3, Credits = credits3 },
					Picture4 = new Picture { URI = filename4, Credits = credits4 },
					Enabled = enable,
					Id = id,
					Order = order,
				});
			}

			this.Puzzles = puzzles;
		}

		private void parseInAppPurchase(string iapInfo)
		{
			string[] kvps = iapInfo.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            InAppPurchase p = null;
            string bundleId = string.Empty;
            decimal price = 0;
            int coins = 0;
            int order = 0;
            string name = string.Empty;
            string discount = string.Empty;

			for (int i = 0; i < kvps.Length; i++)
			{
				string key = string.Empty;
				string value = string.Empty;
				GetKeyValuePairFromString(kvps[i], out key, out value);

				bool success = false;
				switch (key)
				{
					case "bundleId":
						bundleId = value;
						success = true;
                        p = new InAppPurchase() { BundleId = bundleId };
				        this.Purchases.Add(bundleId, p);
						break;
					case "price":
						success = decimal.TryParse(value, out price);
                        p.Price = price;
						break;
					case "coins":
						success = int.TryParse(value, out coins);
                        p.Coins = coins;
						break;
					case "order":
						success = (int.TryParse(value, out order));
                        p.Order = order;
						break;
					case "name":
						name = value;
						success = true;
                        p.Name = name;
						break;
					case "discount":
						discount = value;
						success = true;
                        p.Discount = discount;
						break;
					default:
						throw new ApplicationException("Unknown Gatedata InAppPurchase property: " + key);
				}

				if (!success)
				{
					throw new ApplicationException("Invalid InAppPurchase info. Trouble parsing " + key + ": " + value);
				}
			}
		}

		public void GetDataTypeAndDataValue(string dataLine, out string dataType, out string dataValue)
		{
			String[] linePiece = dataLine.Split(new string[] { ";;;" }, StringSplitOptions.None);
			if (linePiece.Length < 2) { throw new ApplicationException("Invalid Gamedata file. Data missing:" + dataLine); }
			string dataTypePiece = linePiece[0];

			// get dataType
			string dataTypeKey = string.Empty;
			string dataTypeValue = string.Empty;
			GetKeyValuePairFromString(dataTypePiece, out dataTypeKey, out dataTypeValue);
			if (dataTypeKey != "dataType") { throw new ApplicationException("Invalid line in Gamedata at: " + dataLine); }
			dataType = dataTypeValue;

			// get dataValue
			string dataValuePiece = linePiece[1];
			int indexOfFirstEquals = dataValuePiece.IndexOf('=');
			if (-1 == indexOfFirstEquals) { throw new ApplicationException("Invalid line in Gamedata at: " + dataLine); }

			string dataValueKey = dataValuePiece.Substring(0, indexOfFirstEquals);
			string dataValueValue = dataValuePiece.Substring(indexOfFirstEquals + 1);
			if (dataValueKey != "dataValue") { throw new ApplicationException("Invalid line in Gamedata at:" + dataLine); }
			dataValue = dataValueValue;
		}

		public void GetKeyValuePairFromString(string kvpString, out string key, out string value)
		{
			String[] kvp = kvpString.Split(new string[] { "=" }, StringSplitOptions.None);
			key = kvp[0];
			value = kvp[1];
		}
		#endregion

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
				if (PuzzleCharacters[GuessPanelState[i]] != PuzzleWord[i] && PuzzleCharacters[GuessPanelState[i]] != GameState.GUESSPANEL_LETTER_REVEALED) return false;
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

		#endregion

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
	}
}
