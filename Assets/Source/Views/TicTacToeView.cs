using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;
using IntelliMedia;
using UnityEngine.Events;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace IntelliMedia
{
	public class TicTacToeView : UnityGuiView
	{
		public InputField gamesPlayedText;
		public InputField gamesWonText;
		public InputField winningPercentageText;
		public Text messageArea;
		public CanvasGroup buttonGrid;
		public Button saveAndQuitButton;
		public Button newGameButton;
		public Button restartButton;

		public TicTacToeViewModel ViewModel { get { return (TicTacToeViewModel)BindingContext; }}

		protected override void OnBindingContextChanged(ViewModel oldViewModel, ViewModel newViewModel)
		{
			Contract.PropertyNotNull("gamesPlayedText", gamesPlayedText);
			Contract.PropertyNotNull("gamesWonText", gamesWonText);
			Contract.PropertyNotNull("winningPercentageText", winningPercentageText);
			Contract.PropertyNotNull("newGameButton", newGameButton);
			Contract.PropertyNotNull("saveAndQuitButton", saveAndQuitButton);
			Contract.PropertyNotNull("restartButton", restartButton);

			base.OnBindingContextChanged(oldViewModel, newViewModel);

			TicTacToeViewModel oldVm = oldViewModel as TicTacToeViewModel;
			if (oldVm != null)
			{
				oldVm.State.ValueChanged -= OnGameStateChanged;
				oldVm.GamesPlayed.ValueChanged -= UpdateGamesPlayed;
				oldVm.GamesWon.ValueChanged -= UpdateGamesWon;
				oldVm.Message.ValueChanged -= OnMessageChanged;
				oldVm.WinningPercentage.ValueChanged -= OnWinningPercentageChanged;
				oldVm.GlyphPlaced -= OnGlyphPlaced;
			}

			if (ViewModel != null)
			{
				ViewModel.State.ValueChanged += OnGameStateChanged;
				ViewModel.GamesPlayed.ValueChanged += UpdateGamesPlayed;
				ViewModel.GamesWon.ValueChanged += UpdateGamesWon;
				ViewModel.Message.ValueChanged += OnMessageChanged;
				ViewModel.WinningPercentage.ValueChanged += OnWinningPercentageChanged;
				ViewModel.GlyphPlaced += OnGlyphPlaced;
			}

			UpdateControls();
		}

		private void OnGameStateChanged(TicTacToeState.GameState oldValue, TicTacToeState.GameState newValue)
		{
			newGameButton.interactable = (newValue == TicTacToeState.GameState.GameOver);
			buttonGrid.interactable = (newValue != TicTacToeState.GameState.BotsTurn
			                           && newValue != TicTacToeState.GameState.GameOver);
		}

		private void UpdateGamesPlayed(int oldValue, int newValue)
		{
			gamesPlayedText.text = newValue.ToString();
		}

		private void UpdateGamesWon(int oldValue, int newValue)
		{
			gamesWonText.text = newValue.ToString();
		}

		private void OnWinningPercentageChanged(double oldValue, double newValue)
		{
			winningPercentageText.text = newValue.ToString();
		}

		private void OnMessageChanged(string oldValue, string newValue)
		{
			messageArea.text = newValue;
		}

		private void OnGlyphPlaced(int column, int row, char glyph)
		{
			string glyphString = glyph.ToString();

			DebugLog.Info("Glyph placed ({0},{1}): {2} ", column, row, glyphString);
			Button button = GetButtonAt(column, row);
			button.GetComponentInChildren<Text>().text = glyphString;
			button.interactable = TicTacToeViewModel.Glyphs.All(g => g != glyph);
		}

		private void UpdateControls()
		{
			if (ViewModel == null)
			{
				return;
			}

			gamesPlayedText.text = ViewModel.GamesPlayed.ToString();
			gamesWonText.text = ViewModel.GamesWon.ToString();
		}

		public void OnNewGameClicked()
		{
			ViewModel.NewGame();	
		}

		public void OnSaveAndQuitClicked()
		{
			ViewModel.SaveAndQuit();	
		}
				
		public void OnRestartClicked()
		{
			ViewModel.Restart();	
		}

		public void OnBoardCellClicked(GameObject button)
		{
			DebugLog.Info("Cell selected: " + button.name);
			Vector2 location = ParseColumnRowFromName(button.name);
			ViewModel.PlaceGlyph(ViewModel.PlayerId, (int)location.x, (int)location.y);
		}

		private Button GetButtonAt(int column, int row)
		{
			Contract.PropertyNotNull("buttonGrid", buttonGrid);

			string buttonName = string.Format("({0},{1})", column, row);
			Transform transform = buttonGrid.transform.FindChild(buttonName);
			if (transform == null)
			{
				throw new Exception(String.Format("Unable to find game object named '{0}'", buttonName));
			}

			Button button = transform.GetComponent<Button>();
			if (button == null)
			{
				throw new Exception(String.Format("Unable to find button component on '{0}' GameObject", buttonName));
			}

			return button;
		}

		private Regex coordinatesRegex = new Regex(@"^\((?<column>\d+),(?<row>\d+)\)$");
		private Vector2 ParseColumnRowFromName(string name)
		{
			Match match = coordinatesRegex.Match(name);
			if (match != null && match.Groups.Count >= 2)
			{
				return new Vector2(int.Parse(match.Groups["column"].Value), 
				                   int.Parse(match.Groups["row"].Value));
			}

			throw new Exception("Unable to parse button name for location. Expecting (columnm,row)");
		}
	}
}