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
		[DataMember]
		public int CurrentLevel { get; set; }

		[DataMember]
		public int Coins { get; set; }

		[DataMember]
		public String FBMessage { get; set; }

		[DataMember]
		public String GuessPanelState { get; set; }

		[DataMember]
		public String CharacterPanelState { get; set; }

		public async void LoadGameStateFromFile()
		{
			String gameData = await FileAccess.LoadDataFromFileAsync("ms-appdata:///local/gamestate.txt");
			GameState gs = new GameState();
			MemoryStream stream = new MemoryStream();
			DataContractJsonSerializer ser = new DataContractJsonSerializer(this.GetType());
			gs = ser.ReadObject(stream) as GameState;
			stream.Close();

			// TODO: This is the wrong way to do this
			this.CurrentLevel = gs.CurrentLevel;
			this.Coins = gs.Coins;
			this.FBMessage = gs.FBMessage;
			this.GuessPanelState = gs.GuessPanelState;
			this.CharacterPanelState = gs.CharacterPanelState;
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
	}
}
