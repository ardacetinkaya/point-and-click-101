using Godot;
using System;
using System.Text.Json;

public partial class GameMenu : CanvasLayer
{
	private const int SlotCount = 3;
	// Preserved in /Users/{user}/Library/Application Support/Godot/app_userdata/PointAndClick-101/
	private const string SavePathPattern = "user://savegame_slot_{0}.json";

	private enum SlotViewMode
	{
		Save,
		Load
	}

	private Control _overlay = null!;
	private Button _resumeButton = null!;
	private Button _newGameButton = null!;
	private Button _saveButton = null!;
	private Button _loadButton = null!;
	private Label _statusLabel = null!;

	private Control _slotView = null!;
	private Label _slotTitle = null!;
	private Label _slotStatusLabel = null!;
	private Button _slotBackButton = null!;
	private readonly Button[] _slotButtons = new Button[SlotCount];
	private SlotViewMode _slotViewMode;

	private Control _overwriteConfirmation = null!;
	private Label _overwriteQuestion = null!;
	private Button _overwriteYesButton = null!;
	private Button _overwriteNoButton = null!;
	private int _pendingOverwriteSlot;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		_overlay = GetNode<Control>("Overlay");
		_resumeButton = GetNode<Button>("Overlay/CenterContainer/MenuPanel/MarginContainer/MenuItems/ResumeButton");
		_newGameButton = GetNode<Button>("Overlay/CenterContainer/MenuPanel/MarginContainer/MenuItems/NewGameButton");
		_saveButton = GetNode<Button>("Overlay/CenterContainer/MenuPanel/MarginContainer/MenuItems/SaveButton");
		_loadButton = GetNode<Button>("Overlay/CenterContainer/MenuPanel/MarginContainer/MenuItems/LoadButton");
		_statusLabel = GetNode<Label>("Overlay/CenterContainer/MenuPanel/MarginContainer/MenuItems/StatusLabel");

		_slotView = GetNode<Control>("Overlay/SlotCenterContainer");
		_slotTitle = GetNode<Label>("Overlay/SlotCenterContainer/SlotPanel/MarginContainer/SlotItems/Title");
		_slotStatusLabel = GetNode<Label>("Overlay/SlotCenterContainer/SlotPanel/MarginContainer/SlotItems/StatusLabel");
		_slotBackButton = GetNode<Button>("Overlay/SlotCenterContainer/SlotPanel/MarginContainer/SlotItems/BackButton");

		for (int index = 0; index < SlotCount; index++)
		{
			int slotNumber = index + 1;
			Button slotButton = GetNode<Button>($"Overlay/SlotCenterContainer/SlotPanel/MarginContainer/SlotItems/Slot{slotNumber}Button");
			_slotButtons[index] = slotButton;
			slotButton.Pressed += () => OnSlotChosen(slotNumber);
		}

		_overwriteConfirmation = GetNode<Control>("Overlay/OverwriteConfirmation");
		_overwriteQuestion = GetNode<Label>("Overlay/OverwriteConfirmation/CenterContainer/ConfirmationPanel/MarginContainer/ConfirmationItems/Question");
		_overwriteYesButton = GetNode<Button>("Overlay/OverwriteConfirmation/CenterContainer/ConfirmationPanel/MarginContainer/ConfirmationItems/Choices/YesButton");
		_overwriteNoButton = GetNode<Button>("Overlay/OverwriteConfirmation/CenterContainer/ConfirmationPanel/MarginContainer/ConfirmationItems/Choices/NoButton");

		_resumeButton.Pressed += CloseMenu;
		_newGameButton.Pressed += StartNewGame;
		_saveButton.Pressed += () => OpenSlotView(SlotViewMode.Save);
		_loadButton.Pressed += () => OpenSlotView(SlotViewMode.Load);
		_slotBackButton.Pressed += CloseSlotView;
		_overwriteYesButton.Pressed += ConfirmOverwrite;
		_overwriteNoButton.Pressed += CancelOverwrite;

		_overlay.Hide();
		_slotView.Hide();
		_overwriteConfirmation.Hide();
		RefreshLoadButton();
	}

	public override void _Input(InputEvent @event)
	{
		if (!@event.IsActionPressed("toggle_game_menu") || @event.IsEcho())
		{
			return;
		}

		ToggleMenu();
		GetViewport().SetInputAsHandled();
	}

	private void ToggleMenu()
	{
		if (!_overlay.Visible)
		{
			OpenMenu();
			return;
		}

		if (_overwriteConfirmation.Visible)
		{
			CancelOverwrite();
			return;
		}

		if (_slotView.Visible)
		{
			CloseSlotView();
			return;
		}

		CloseMenu();
	}

	private void OpenMenu()
	{
		_statusLabel.Text = string.Empty;
		_slotView.Hide();
		_overwriteConfirmation.Hide();
		RefreshLoadButton();
		_overlay.Show();
		GetTree().Paused = true;
		_resumeButton.GrabFocus();
	}

	private void CloseMenu()
	{
		_overwriteConfirmation.Hide();
		_slotView.Hide();
		_overlay.Hide();
		GetTree().Paused = false;
	}

	private void StartNewGame()
	{
		GameState.Instance.Reset();
		InventoryManager.Instance.RestoreItems([]);
		CloseMenu();

		Error result = GetTree().ChangeSceneToFile(Consts.CrashSiteScenePath);

		if (result == Error.Ok)
		{
			return;
		}

		GD.PushError($"Could not start a new game: {result}");
		OpenMenu();
		ShowStatus("New game could not be started.", true);
	}

	private void OpenSlotView(SlotViewMode mode)
	{
		_slotViewMode = mode;
		_slotTitle.Text = mode == SlotViewMode.Save ? "SAVE GAME" : "LOAD GAME";
		_slotStatusLabel.Text = string.Empty;
		RefreshSlotButtons();
		_slotView.Show();

		foreach (Button button in _slotButtons)
		{
			if (!button.Disabled)
			{
				button.GrabFocus();
				return;
			}
		}

		_slotBackButton.GrabFocus();
	}

	private void CloseSlotView()
	{
		_slotView.Hide();
		_statusLabel.Text = string.Empty;
		RefreshLoadButton();
		(_slotViewMode == SlotViewMode.Save ? _saveButton : _loadButton).GrabFocus();
	}

	private void OnSlotChosen(int slotNumber)
	{
		if (_slotViewMode == SlotViewMode.Load)
		{
			LoadGame(slotNumber);
			return;
		}

		if (FileAccess.FileExists(GetSavePath(slotNumber)))
		{
			AskToOverwrite(slotNumber);
			return;
		}

		SaveGame(slotNumber);
	}

	private void AskToOverwrite(int slotNumber)
	{
		_pendingOverwriteSlot = slotNumber;
		_overwriteQuestion.Text = $"Slot#{slotNumber} already has a saved game.\nAre you sure you want to overwrite it?";
		_overwriteConfirmation.Show();
		_overwriteNoButton.GrabFocus();
	}

	private void ConfirmOverwrite()
	{
		int slotNumber = _pendingOverwriteSlot;
		_overwriteConfirmation.Hide();
		_pendingOverwriteSlot = 0;
		SaveGame(slotNumber);
	}

	private void CancelOverwrite()
	{
		int slotNumber = _pendingOverwriteSlot;
		_overwriteConfirmation.Hide();
		_pendingOverwriteSlot = 0;

		if (slotNumber is >= 1 and <= SlotCount)
		{
			_slotButtons[slotNumber - 1].GrabFocus();
		}
	}

	private void SaveGame(int slotNumber)
	{
		Node? currentScene = GetTree().CurrentScene;

		if (currentScene == null || string.IsNullOrWhiteSpace(currentScene.SceneFilePath))
		{
			ShowSlotStatus("This scene cannot be saved.", true);
			return;
		}

		Node2D? player = currentScene.GetNodeOrNull<Node2D>("Player");
		var saveData = new SaveGameData
		{
			SavedAt = DateTimeOffset.Now.ToString("O"),
			ScenePath = currentScene.SceneFilePath,
			PlayerPositionX = player?.GlobalPosition.X,
			PlayerPositionY = player?.GlobalPosition.Y,
			HasLookedAtMap = GameState.Instance.HasLookedAtMap,
			HasTakenMap = GameState.Instance.HasTakenMap,
			HasShownCrashSiteIntro = GameState.Instance.HasShownCrashSiteIntro,
			StationHasPower = GameState.Instance.StationHasPower,
			InventoryItems = [.. InventoryManager.Instance.Items]
		};

		try
		{
			using FileAccess file = FileAccess.Open(GetSavePath(slotNumber), FileAccess.ModeFlags.Write);

			if (file == null)
			{
				ShowSlotStatus($"Save failed: {FileAccess.GetOpenError()}", true);
				return;
			}

			file.StoreString(JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true }));
			RefreshSlotButtons();
			RefreshLoadButton();
			ShowSlotStatus($"Game saved to Slot#{slotNumber}.");
			_slotButtons[slotNumber - 1].GrabFocus();
		}
		catch (Exception exception)
		{
			GD.PushError($"Could not save game: {exception}");
			ShowSlotStatus("Save failed.", true);
		}
	}

	private void LoadGame(int slotNumber)
	{
		SaveGameData? saveData = ReadSaveData(slotNumber, true);

		if (saveData == null)
		{
			return;
		}

		if (string.IsNullOrWhiteSpace(saveData.ScenePath) || !ResourceLoader.Exists(saveData.ScenePath))
		{
			ShowSlotStatus("The saved scene is unavailable.", true);
			return;
		}

		GameState.Instance.Restore(
			saveData.HasLookedAtMap,
			saveData.HasTakenMap,
			saveData.HasShownCrashSiteIntro,
			saveData.StationHasPower
		);
		InventoryManager.Instance.RestoreItems(saveData.InventoryItems);
		GameState.Instance.SetLoadedPlayerPosition(GetSavedPlayerPosition(saveData));

		CloseMenu();

		Error result = GetTree().ChangeSceneToFile(saveData.ScenePath);

		if (result != Error.Ok)
		{
			GameState.Instance.SetLoadedPlayerPosition(null);
			GD.PushError($"Could not load saved scene: {result}");
			OpenMenu();
			OpenSlotView(SlotViewMode.Load);
			ShowSlotStatus("Load failed.", true);
		}
	}

	private SaveGameData? ReadSaveData(int slotNumber, bool showErrors)
	{
		string savePath = GetSavePath(slotNumber);

		if (!FileAccess.FileExists(savePath))
		{
			if (showErrors)
			{
				ShowSlotStatus("No saved game found in this slot.", true);
			}

			return null;
		}

		try
		{
			using FileAccess file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);

			if (file == null)
			{
				if (showErrors)
				{
					ShowSlotStatus($"Load failed: {FileAccess.GetOpenError()}", true);
				}

				return null;
			}

			SaveGameData? saveData = JsonSerializer.Deserialize<SaveGameData>(file.GetAsText());

			if (saveData == null || saveData.Version != 1)
			{
				if (showErrors)
				{
					ShowSlotStatus("The save file is not supported.", true);
				}

				return null;
			}

			return saveData;
		}
		catch (Exception exception)
		{
			GD.PushError($"Could not read Slot#{slotNumber}: {exception}");

			if (showErrors)
			{
				ShowSlotStatus("The save file could not be read.", true);
			}

			return null;
		}
	}

	private void RefreshSlotButtons()
	{
		for (int slotNumber = 1; slotNumber <= SlotCount; slotNumber++)
		{
			string savePath = GetSavePath(slotNumber);
			bool hasSave = FileAccess.FileExists(savePath);
			Button button = _slotButtons[slotNumber - 1];
			button.Text = GetSlotLabel(slotNumber, savePath, hasSave);
			button.Disabled = _slotViewMode == SlotViewMode.Load && !hasSave;
		}
	}

	private string GetSlotLabel(int slotNumber, string savePath, bool hasSave)
	{
		if (!hasSave)
		{
			return $"Slot#{slotNumber}-Empty";
		}

		SaveGameData? saveData = ReadSaveData(slotNumber, false);
		DateTimeOffset savedAt;

		if (saveData != null && DateTimeOffset.TryParse(saveData.SavedAt, out savedAt))
		{
			return $"Slot#{slotNumber}-{savedAt.ToLocalTime():yyyy/MM/dd hh:mm}";
		}

		ulong modifiedTime = FileAccess.GetModifiedTime(savePath);
		if (modifiedTime > 0)
		{
			savedAt = DateTimeOffset.FromUnixTimeSeconds((long)modifiedTime).ToLocalTime();
			return $"Slot#{slotNumber}-{savedAt:yyyy/MM/dd hh:mm}";
		}

		return $"Slot#{slotNumber}-Unavailable";
	}

	private static Vector2? GetSavedPlayerPosition(SaveGameData saveData)
	{
		if (!saveData.PlayerPositionX.HasValue || !saveData.PlayerPositionY.HasValue)
		{
			return null;
		}

		return new Vector2(saveData.PlayerPositionX.Value, saveData.PlayerPositionY.Value);
	}

	private static string GetSavePath(int slotNumber) => string.Format(SavePathPattern, slotNumber);

	private void RefreshLoadButton()
	{
		_loadButton.Disabled = true;

		for (int slotNumber = 1; slotNumber <= SlotCount; slotNumber++)
		{
			if (FileAccess.FileExists(GetSavePath(slotNumber)))
			{
				_loadButton.Disabled = false;
				return;
			}
		}
	}

	private void ShowStatus(string message, bool isError = false)
	{
		_statusLabel.Text = message;
		_statusLabel.Modulate = GetStatusColor(isError);
	}

	private void ShowSlotStatus(string message, bool isError = false)
	{
		_slotStatusLabel.Text = message;
		_slotStatusLabel.Modulate = GetStatusColor(isError);
	}

	private static Color GetStatusColor(bool isError) =>
		isError ? new Color("ff8a80") : new Color("b9f6ca");
}
