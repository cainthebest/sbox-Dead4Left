using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox;
using Sandbox.Network;
using Sandbox.Citizen;
using System.Linq;
using Kicks;

/// <summary>
/// Manages network operations, including server creation, player connections, and spawning player entities.
/// Implements INetworkListener for handling network events.
/// </summary>
public sealed class NetworkManager : Component, Component.INetworkListener
{
	public static NetworkManager Instance { get; private set; }

	/// <summary>
	/// Determines whether a server should be started.
	/// </summary>
	[Property] public bool StartServer { get; set; } = true;

	/// <summary>
	/// The player prefab to spawn for controlling by the player.
	/// </summary>
	[Property] public GameObject PlayerPrefab { get; set; }

	/// <summary>
	/// Scene file to load when disconnecting or needed.
	/// </summary>
	[Property] public SceneFile menuScene { get; set; }

	/// <summary>
	/// List of current game connections.
	/// </summary>
	public List<Connection> Connections = new();

	/// <summary>
	/// The host connection.
	/// </summary>
	public Connection Host = null;

	/// <summary>
	/// Steam ID of the host for identification.
	/// </summary>
	[Sync] public long HostSteamId { get; set; }

	/// <summary>
	/// List of player controllers currently active in the scene.
	/// </summary>
	public List<PlayerController> Players => GameManager.ActiveScene.Components.GetAll<PlayerController>(FindMode.EnabledInSelfAndDescendants).ToList();

	/// <summary>
	/// Sets up the instance reference upon component awakening.
	/// </summary>
	protected override void OnAwake()
	{
		base.OnAwake();
		Instance = this;
	}

	/// <summary>
	/// Initializes the server if the scene is not in editor mode and a server is not already active.
	/// </summary>
	protected override async Task OnLoad()
	{
		if (Scene.IsEditor)
			return;

		if (StartServer && !GameNetworkSystem.IsActive)
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds(0.1f); // Delay to ensure network readiness
			GameNetworkSystem.CreateLobby(); // Create a new game lobby
		}
	}

	/// <summary>
	/// Handles a new player connection by adding them to the connections list and spawning a player entity.
	/// </summary>
	/// <param name="channel">The connection channel of the newly connected player.</param>
	public void OnActive(Connection channel)
	{
		Log.Info($"Player '{channel.DisplayName}' has joined the game");
		Connections.Add(channel);

		// Assign the first connecting player as the host
		if (Connections.Count == 1)
		{
			Host = channel;
			HostSteamId = (long)channel.SteamId;
			if (!string.IsNullOrEmpty(LaunchArguments.Map))
				Scene.Title = LaunchArguments.Map; // Set the scene title based on launch arguments, if applicable
		}

		if (PlayerPrefab is null) return; // Exit if no player prefab is defined

		SpawnPlayer(channel); // Spawn a player entity for the connected player
	}

	/// <summary>
	/// Spawns a player entity for a given connection and assigns it to the player.
	/// </summary>
	/// <param name="channel">The connection channel for which to spawn the player entity.</param>
	[Broadcast]
	public async void SpawnPlayer(Connection channel)
	{
		// Determine a spawn location, defaulting to the world origin or using a random spawn point
		var startLocation = Transform.World;
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToList();
		if (spawnPoints.Count > 0)
		{
			startLocation = spawnPoints[Random.Shared.Int(0, spawnPoints.Count - 1)].Transform.World;
		}

		startLocation.Scale = 1; // Ensure the spawn scale is normalized

		// Clone the PlayerPrefab at the determined location and network spawn it for the connecting player
		var player = PlayerPrefab.Clone(startLocation, name: $"Player - {channel.DisplayName}");
		player.Network.Spawn(channel); // Network spawn the cloned player entity

		// The PlayerController component retrieval is intended for future use i think - cainthebest
		var playerController = player.Components.Get<PlayerController>(FindMode.EverythingInSelfAndDescendants);
	}

	/// <summary>
	/// Handles player disconnection, cleaning up by loading the menu scene.
	/// </summary>
	/// <param name="channel">The connection channel of the disconnecting player.</param>
	public void OnDisconnected(Connection channel)
	{
		GameManager.ActiveScene.Load(menuScene); // Load the menu scene upon player disconnection
	}
}