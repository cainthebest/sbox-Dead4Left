using System.Collections.Generic;
using System.ComponentModel.Design;
using Sandbox;
using Sandbox.UI;
using System.Linq;
using Kicks;
using Sandbox.Network;
using System;
using Sandbox.Navigation;

/// <summary>
/// Manages game state, including player status, score, and high score. 
/// Also responsible for handling game start and end logic, score processing, and leaderboard interaction.
/// </summary>
public sealed class Manager : Component
{
	[Property] public SceneFile menuScene { get; set; }
	public bool Playing { get; private set; } = false;
	public long Score { get; private set; } = 0;
	public long HighScore { get; private set; } = 0;
	[Property] public bool testBool { get; set; }
	[Property] public bool ableToInput { get; set; } = false;
	public bool ShouldAddScore { get; set; } = false;

	// Refs to player controller and weapon components
	PlayerController playerController;
	Weapon weapon;

	// Leaderboard for tracking player scores
	public Sandbox.Services.Leaderboards.Board Leaderboard;

	/// <summary>
	/// Initializes game components and state at the start of the game. This includes
	/// setting up the player controller, weapon components, and resetting ammo counts.
	/// </summary>
	protected override void OnStart()
	{
		// Find the player controller and weapon that are not proxies
		playerController = GameManager.ActiveScene.GetAllComponents<PlayerController>().FirstOrDefault(x => !x.IsProxy);
		weapon = GameManager.ActiveScene.GetAllComponents<Weapon>().FirstOrDefault(x => !x.IsProxy);
		// Mark the navigation mesh as dirty to trigger a rebuild
		Scene.NavMesh.SetDirty();
		// Reset ammo files to initial values
		FileSystem.Data.DeleteFile("mp5ammo.txt");
		FileSystem.Data.DeleteFile("mp5maxammo.txt");
		FileSystem.Data.DeleteFile("pistolammo.txt");
		FileSystem.Data.DeleteFile("pistolmaxammo.txt");
		FileSystem.Data.WriteAllText("mp5ammo.txt", "30");
		FileSystem.Data.WriteAllText("mp5maxammo.txt", "60");
		FileSystem.Data.WriteAllText("pistolammo.txt", "60");
		FileSystem.Data.WriteAllText("pistolmaxammo.txt", "120");
	}

	/// <summary>
	/// Performs initial setup operations when the component awakes.
	/// </summary>
	protected override void OnAwake()
	{
		StartGame();
	}

	/// <summary>
	/// Updates game state every frame, checking for player health and managing score processing.
	/// </summary>
	protected override void OnUpdate()
	{
		if (playerController is null) return;
		// End game if player health drops to zero or below
		if (playerController.Health <= 0)
		{
			EndGame();
		}
		// Ensure player health does not go below zero
		if (playerController.Health < 0)
		{
			playerController.Health = 0;
		}

		// Process player score updates
		ProcessScore();
	}

	/// <summary>
	/// Starts the game, resetting the score and fetching leaderboard information.
	/// </summary>
	public void StartGame()
	{
		Playing = true;
		Score = 0;

		FetchLeaderboardInfo();
	}

	/// <summary>
	/// Ends the game, updating stats and handling player respawn.
	/// </summary>
	public void EndGame()
	{
		Playing = false;
		// Update player stats and log disconnection
		Sandbox.Services.Stats.SetValue("zombieskilled", playerController.Score);
		Log.Info("Disconnected from server");
		Respawn();
	}

	/// <summary>
	/// Processes and updates the score, setting a new high score if necessary.
	/// </summary>
	public void ProcessScore()
	{
		Score = playerController.Score;
		if (Score > HighScore) HighScore = Score;
	}

	/// <summary>
	/// Fetches leaderboard information asynchronously and updates high score accordingly.
	/// </summary>
	async void FetchLeaderboardInfo()
	{
		Leaderboard = Sandbox.Services.Leaderboards.Get("mostzombieskill");
		Leaderboard.MaxEntries = 10;
		await Leaderboard.Refresh();
		foreach (var entry in Leaderboard.Entries)
		{
			if (entry.SteamId == Game.SteamId)
			{
				HighScore = (long)entry.Value;
			}
		}
	}

	/// <summary>
	/// Respawns the player at a random spawn point, resetting their score and health.
	/// </summary>
	void Respawn()
	{
		playerController.Score = 0;
		playerController.Health = 100;
		var Spawns = GameManager.ActiveScene.GetAllComponents<SpawnPoint>().ToArray();
		var randomSpawnPoint = Random.Shared.FromArray(Spawns);
		playerController.Transform.Position = randomSpawnPoint.Transform.Position;
	}
}
