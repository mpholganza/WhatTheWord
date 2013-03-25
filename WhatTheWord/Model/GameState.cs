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
		#endregion

		#region Config variables
		public int initialCoins { get; set; }
		public int boostRemoveLettersCost { get; set; }
		public int boostRemoveLettersNumberOfLetters { get; set; }
		public int boostRevealLetterCost { get; set; }
		public int boostShuffleCost { get; set; }
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
        public string FacebookToken { get; set; }
		public int CurrentLevel { get; set; }
		public int Coins { get; set; }
		public String PuzzleWord { get; set; }
		public String PuzzleCharacters { get; set; }
		public int[] GuessPanelState { get; set; }
		public int[] CharacterPanelState { get; set; }
		public bool Loaded { get; set; }
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
			this.Loaded = false;
            this.Purchases = new Dictionary<string, InAppPurchase>();
		}

		/// <summary>
		/// Load game config data. This can come from two potential places. In order of precedence
		/// 1. Previously downloaded config info from the web.
		/// 2. Built-in config file.
		/// </summary>
		public void LoadGameState()
		{
			// Load config info from previously saved file
			LoadGameConfigFromFile();
			if (this.Loaded) return;

			// Built-in config file
			LoadGameConfigFromDefaultFile();

			if (!this.Loaded)
			{
				throw new ApplicationException("Unable to load game config information.");
			}
		}

		private async void LoadGameConfigFromFile()
		{
			Task<String> loadDataFromFileTask =  FileAccess.LoadDataFromFileAsync("gamestate.txt");
			string gameData = await loadDataFromFileTask;
			if (gameData != string.Empty)
			{
				try
				{
					DeserializeGameData(gameData);
				}
				catch (ApplicationException)
				{
					// deserialized incorrectly. fail quietly
					// TODO: report to server of failed deserialization
					Console.WriteLine("Failed deserialization:\n" + gameData);
				}
			}
		}

		private void LoadGameConfigFromDefaultFile()
		{
			StreamResourceInfo sri = App.GetResourceStream(new Uri("Assets/gamestate.txt", UriKind.Relative));
			StreamReader streamReader = new StreamReader(sri.Stream);
			string gameData = streamReader.ReadToEnd();
			DeserializeGameData(gameData);
		}

		private string MD5(string original)
		{
			// TODO: get md5 implementation
			return original;
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
                this.FacebookToken = gs.FacebookToken;
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
			string[] lines = gameData.Split(new string[] { "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
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

			if (flightHash != value) { throw new ApplicationException("Corrupt Gamedata file. Hash does not match"); }

			this.Purchases = new Dictionary<string, InAppPurchase>();

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
						InAppPurchase purchase = parseInAppPurchase(dataValue);
						this.Purchases.Add(purchase.BundleId, purchase);
						break;
					default:
						throw new ApplicationException("Invalid line in Gamedata file:" + lines[i]);
				}
			}

			this.Loaded = true;
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
			int boostRemoveLettersCost = 0;
			int boostRemoveLettersNumberOfLetters = 0;
			int boostRevealLetterCost = 0;
			int boostShuffleCost = 0;
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
                    this.initialCoins = initialCoins;
					break;
				case "boostRemoveLettersCost":
					success = int.TryParse(value, out boostRemoveLettersCost);
			        this.boostRemoveLettersCost = boostRemoveLettersCost;
					break;
				case "boostRemoveLettersNumberOfLetters":
					success = int.TryParse(value, out boostRemoveLettersNumberOfLetters);
                    this.boostRemoveLettersNumberOfLetters = boostRemoveLettersNumberOfLetters;
					break;
				case "boostRevealLetterCost":
					success = int.TryParse(value, out boostRevealLetterCost);
                    this.boostRevealLetterCost = boostRevealLetterCost;
					break;
				case "boostShuffleCost":
					success = int.TryParse(value, out boostShuffleCost);
                    this.boostShuffleCost = boostShuffleCost;
					break;
				case "rewardCoinsPerQuestion":
					success = int.TryParse(value, out rewardCoinsPerQuestion);
                    this.rewardCoinsPerQuestion = rewardCoinsPerQuestion;
					break;
				case "rateMeReward":
					success = int.TryParse(value, out rateMeReward);
                    this.rateMeReward = rateMeReward;
					break;
				case "rateMeShowInitial":
					success = int.TryParse(value, out rateMeShowInitial);
                    this.rateMeShowInitial = rateMeShowInitial;
					break;
				case "rateMeShowReminderInterval":
					success = int.TryParse(value, out rateMeShowReminderInterval);
                    this.rateMeShowReminderInterval = rateMeShowReminderInterval;
					break;
				case "boostBounceTimeInterval":
					success = int.TryParse(value, out boostBounceTimeInterval);
                    this.boostBounceTimeInterval = boostBounceTimeInterval;
					break;
				case "picsFailedDownloadWait":
					success = int.TryParse(value, out picsFailedDownloadWait);
                    this.picsFailedDownloadWait = picsFailedDownloadWait;
					break;
				case "picsSuccessDownloadWait":
					success = int.TryParse(value, out picsSuccessDownloadWait);
                    this.picsSuccessDownloadWait = picsSuccessDownloadWait;
					break;
				case "rateMeURL":
					rateMeURL = value;
                    this.rateMeURL = rateMeURL;
					success = true;
					break;
				case "picturesFilenamePath":
					picturesFilenamePath = value;
                    this.picturesFilenamePath = picturesFilenamePath;
					success = true;
					break;
				default:
					throw new ApplicationException("Unknown Gatedata config key: " + keyName);
			}

			if (!success)
			{
				throw new ApplicationException("Invalid Gatedata config info. Trouble parsing " + keyName + ": " + value);
			}
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

		private InAppPurchase parseInAppPurchase(string iapInfo)
		{
			string[] kvps = iapInfo.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

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
						break;
					case "price":
						success = decimal.TryParse(value, out price);
						break;
					case "coins":
						success = int.TryParse(value, out coins);
						break;
					case "order":
						success = (int.TryParse(value, out order));
						break;
					case "name":
						name = value;
						success = true;
						break;
					case "discount":
						discount = value;
						success = true;
						break;
					default:
						throw new ApplicationException("Unknown Gatedata InAppPurchase property: " + key);
				}

				if (!success)
				{
					throw new ApplicationException("Invalid InAppPurchase info. Trouble parsing " + key + ": " + value);
				}
			}

			return new InAppPurchase()
			{
				BundleId = bundleId,
				Price = price,
				Coins = coins,
                Discount = discount,
				Order = order,
				Name = name,
			};
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

        public void RevealLetter()
        {
            // Clear the guess panel
            this.ClearGuessPanel();

            // Pick a letter that hasn't been revealed yet
            Random random = new Random((int)DateTime.Now.Ticks);
            int revealIndex = Convert.ToInt32(Math.Floor(random.NextDouble() * GuessPanelState.Length));

            // Set it to revealed
            GuessPanelState[revealIndex] = GameState.GUESSPANEL_LETTER_REVEALED;
        }

        public void RemoveLetter()
        {
            // Pick a letter that is not a part of the actual word
			Random random = new Random((int)DateTime.Now.Ticks);
            int removeIndex = -1;
            //Convert.ToInt32(Math.Floor(random.NextDouble() * CharacterPanelState.Length)));

            // Set it to removed
            CharacterPanelState[removeIndex] = GameState.CHARACTERPANEL_LETTER_REMOVED;
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

		public static void WriteGameDataToFile(string gameData)
		{
			FileAccess.WriteDataToFileAsync(gameData, "gamestate.txt");
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
