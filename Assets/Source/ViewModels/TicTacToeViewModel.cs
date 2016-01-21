//---------------------------------------------------------------------------------------
// Copyright 2014 North Carolina State University
//
// Center for Educational Informatics
// http://www.cei.ncsu.edu/
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
//   * Redistributions of source code must retain the above copyright notice, this 
//     list of conditions and the following disclaimer.
//   * Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//---------------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Collections.Generic;

namespace IntelliMedia
{
	public class TicTacToeViewModel : ActivityViewModel
	{
		private static Random rnd = new Random();
		public static readonly string[] PlayersTurnMessages = new string[]
		{
			"Your turn.",
			"Go ahead.",
			"You're up.",
			"Pick a good one.",
			"Take your time.",
			"Waiting for you.",
			"Ok. Go."
		};

		public static readonly string[] BotsTurnMessages = new string[]
		{
			"My turn!",
			"Hrmmm...",
			"Thinking...",
		};

		public static readonly char[] Glyphs = new char[] { 'X', 'O' };

		private readonly char[,] boardState = new char[3,3];

		public int PlayerId { get; private set; }
		public int BotId { get; private set; }

		public readonly BindableProperty<TicTacToeState.GameState> State = new BindableProperty<TicTacToeState.GameState>();
		public readonly BindableProperty<int> GamesPlayed = new BindableProperty<int>();
		public readonly BindableProperty<int> GamesWon = new BindableProperty<int>();
		public readonly BindableProperty<double> WinningPercentage = new BindableProperty<double>();
		public readonly BindableProperty<string> Message = new BindableProperty<string>();

		public delegate void GlyphPlacedHandler(int column, int row, char glyph);
		public GlyphPlacedHandler GlyphPlaced;

		public void PlaceGlyph(int playerId, int column, int row)
		{
			Contract.Argument("Out of range", "playerId", playerId >= 0 && playerId < Glyphs.Length);

			boardState[column, row] = Glyphs[playerId];
			if (GlyphPlaced != null)
			{
				GlyphPlaced(column, row, Glyphs[playerId]);
			}
			ProcessTurn(playerId);
		}
						
		public TicTacToeViewModel(StageManager navigator, ActivityService activityService) : base(navigator, activityService)
		{
			State.ValueChanged += OnGameStateChange;
			GamesPlayed.ValueChanged += UpdateWinningPercentage;
			GamesWon.ValueChanged += UpdateWinningPercentage;
		}

		public override void OnStartReveal()
		{
			InitializeFromSaveData();
			base.OnStartReveal();
		}

		public void NewGame()
		{
			if (PlayerId == 0)
			{
				PlayerId = 1;
				BotId = 0;
			}
			else
			{
				PlayerId = 0;
				BotId = 1;
			}
			ClearBoard();
			OnEntireBoardChanged();

			State.Value = (PlayerId == 0 ? TicTacToeState.GameState.PlayersTurn : TicTacToeState.GameState.BotsTurn);			
		}

		private void ClearBoard()
		{
			for (int column = 0; column < 3; ++column)
			{
				for (int row = 0; row < 3; ++row)
				{
					boardState[column, row] = '\0';
				}
			}
		}

		private void InitializeFromSaveData()
		{
			TicTacToeState ticTacToeState = DeserializeActivityData<TicTacToeState>();
			
			State.Value = ticTacToeState.State;
			PlayerId = ticTacToeState.PlayerId;
			BotId = ticTacToeState.BotId;
			Message.Value = ticTacToeState.Message;
			GamesWon.Value = ticTacToeState.GamesWon;
			GamesPlayed.Value = ticTacToeState.GamesPlayed;

			ClearBoard();
			foreach (TicTacToeState.BoardCellState cell in ticTacToeState.BoardData)
			{
				boardState[cell.Column, cell.Row] = cell.Glyph;
			}

			OnEntireBoardChanged();
		}
		
		public void SaveAndQuit()
		{
			TicTacToeState ticTacToeState = DeserializeActivityData<TicTacToeState>();

			ticTacToeState.State = State.Value;
			ticTacToeState.PlayerId = PlayerId;
			ticTacToeState.BotId = BotId;
			ticTacToeState.Message = Message.Value;
			ticTacToeState.GamesWon = GamesWon.Value;
			ticTacToeState.GamesPlayed = GamesPlayed.Value;

			ticTacToeState.BoardData.Clear();
			for (int column = 0; column < 3; ++column)
			{
				for (int row = 0; row < 3; ++row)
				{
					if (GlyphPlaced != null)
					{
						if (Glyphs.Any(g => g == boardState[column, row]))
						{
							ticTacToeState.BoardData.Add(new TicTacToeState.BoardCellState()
							{
								Column = column,
								Row = row,
								Glyph = boardState[column, row]
							});
						}
					}
				}
			}

			SerializeActivityData(ticTacToeState);
			SaveActivityStateAndTransition<MainMenuViewModel>();
		}

		public void Restart()
		{
			SerializeActivityData(new TicTacToeState());
			InitializeFromSaveData();
		}

		private void OnGameStateChange(TicTacToeState.GameState oldState, TicTacToeState.GameState newState)
		{
			switch(newState)
			{
			case TicTacToeState.GameState.PlayersTurn:
				Message.Value = PlayersTurnMessages[rnd.Next(PlayersTurnMessages.Length)];
				break;

			case TicTacToeState.GameState.BotsTurn:
				Message.Value = BotsTurnMessages[rnd.Next(BotsTurnMessages.Length)];
				DispatcherTimer.Invoke(3, () =>
				{
					BotsTurn();
				});
				break;
			}
		}

		private void OnEntireBoardChanged()
		{
			for (int column = 0; column < 3; ++column)
			{
				for (int row = 0; row < 3; ++row)
				{
					if (GlyphPlaced != null)
					{
						GlyphPlaced(column, row, boardState[column, row]);
					}
				}
			}
		}

		private void UpdateWinningPercentage(int oldValue, int newValue)
		{
			WinningPercentage.Value = (GamesPlayed.Value > 0 ? ((double)GamesWon.Value / GamesPlayed.Value) : 0) * 100d;
		}

		private void ProcessTurn(int playerId)
		{
			char winningGlyph = '\0';

			// Check for winner
			for (int index = 0; index < 3; ++index)
			{
				// Check columns
				if (boardState[index, 0] != '\0' && boardState[index, 0] == boardState[index, 1] && boardState[index, 0] == boardState[index, 2])
				{
					winningGlyph = boardState[index, 0];
					break;
				}

				// Check rows
				if (boardState[0, index] != '\0' && boardState[0, index] == boardState[1, index] && boardState[0, index] == boardState[2, index])
				{
					winningGlyph = boardState[0, index];
					break;
				}

				// Check diagonals
				if (index == 0)
				{
					if (boardState[0, 0] != '\0' && boardState[0, 0] == boardState[1, 1] && boardState[0, 0] == boardState[2, 2])
					{
						winningGlyph = boardState[0, 0];
						break;
					}

					if (boardState[0, 2] != '\0' && boardState[0, 2] == boardState[1, 1] && boardState[0, 2] == boardState[2, 0])
					{
						winningGlyph = boardState[0, 2];
						break;
					}
				}
			}

			// Do we have a winner?
			if (winningGlyph != '\0')
			{
				++GamesPlayed.Value;
				if (Glyphs[PlayerId] == winningGlyph)
				{
					++GamesWon.Value;
					Message.Value = "You won!";
				}
				else
				{
					Message.Value = "You lost.";
				}
				State.Value = TicTacToeState.GameState.GameOver;
				return;
			}

			// Are all the spots taken?
			bool catsGame = true;
			for (int column = 0; column < 3; ++column)
			{
				for (int row = 0; row < 3; ++row)
				{
					if (Glyphs.All( g => g != boardState[column, row]))
					{
						catsGame  = false;
						break;
					}
				}
			}
			if (catsGame)
			{
				++GamesPlayed.Value;
				Message.Value = "Tie.";
				State.Value = TicTacToeState.GameState.GameOver;
				return;
			}

			// Next player
			if (playerId == PlayerId)
			{
				State.Value = TicTacToeState.GameState.BotsTurn;
			}
			else
			{
				State.Value = TicTacToeState.GameState.PlayersTurn;
			}
		}

		public void BotsTurn() 
		{
			List<KeyValuePair<int,int>> availableSpots = new List<KeyValuePair<int, int>>();
			for (int column = 0; column < 3; ++column)
			{
				for (int row = 0; row < 3; ++row)
				{
					if (Glyphs.All( g => g != boardState[column, row]))
					{
						availableSpots.Add(new KeyValuePair<int, int>(column, row));
					}
				}
			}

			if (availableSpots.Count == 0)
			{
				State.Value = TicTacToeState.GameState.GameOver;
			}

			KeyValuePair<int,int> spot = availableSpots[rnd.Next(availableSpots.Count)];
			PlaceGlyph(BotId, spot.Key, spot.Value);
		}
	}
}
