using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox;
using Sandbox.Network;
using Sandbox.Citizen;
using System.Linq;
using Kicks;

/// <summary>
/// Manages network interactions such as server creation, player connections, and dynamically spawning player entities.
/// Implements the INetworkListener to handle network events.
/// </summary>
public sealed class NetworkHelper : Component, Component.INetworkListener
{
	public static NetworkManager Instance { get; private set; }

	/// <summary>
	/// Flag to determine if a server should be started.
	/// </summary>
	[Property] public bool StartServer { get; set; } = true;

	/// <summary>
	/// Prefab for spawning player entities.
	/// </summary>
	[Property] public GameObject PlayerPrefab { get; set; }

	/// <summary>
	/// Scene file for the menu, used when destroying the server.
	/// </summary>
	[Property] public SceneFile menuScene { get; set; }

	/// <summary>
	/// List of current connections.
	/// </summary>
	public List<Connection> Connections = new();

	/// <summary>
	/// The host connection.
	/// </summary>
	public Connection Host = null;

	/// <summary>
	/// The Steam ID of the host.
	/// </summary>
	[Sync] public long HostSteamId { get; set; }

	/// <summary>
	/// Retrieves a list of all active player controllers.
	/// </summary>
	public List<PlayerController> Players => GameManager.ActiveScene.Components.GetAll<PlayerController>(FindMode.EnabledInSelfAndDescendants).ToList();

	/// <summary>
	/// Attempts to start a server if not already active upon component load.
	/// </summary>
	protected override async Task OnLoad()
	{
		if (StartServer && !GameNetworkSystem.IsActive)
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelaySeconds(0.1f); // Delay to ensure UI can update
			GameNetworkSystem.CreateLobby(); // Initiate server creation
		}
	}

	/// <summary>
	/// Handles new player connections by adding them to the connection list and spawning their player entity.
	/// </summary>
	/// <param name="channel">The connection channel for the new player.</param>
	public void OnActive(Connection channel)
	{
		Log.Info($"Player '{channel.DisplayName}' has joined the game");
		Connections.Add(channel);

		// Set the first player as the host
		if (Connections.Count == 1)
		{
			Host = channel;
			HostSteamId = (long)channel.SteamId;
		}

		// Do not proceed if the PlayerPrefab is not set
		if (PlayerPrefab is null) return;

		SpawnPlayer(channel); // Spawn a player entity for the new connection
	}

	/// <summary>
	/// Spawns a player entity for the given connection.
	/// </summary>
	/// <param name="channel">The player's connection channel.</param>
	public async void SpawnPlayer(Connection channel)
	{
		// Find a random spawn point or use the world origin if none are available
		var startLocation = Transform.World;
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToList();
		if (spawnPoints.Count > 0)
		{
			startLocation = spawnPoints[Random.Shared.Int(0, spawnPoints.Count - 1)].Transform.World;
		}

		// Ensure the scale is set to 1 for consistency
		startLocation.Scale = 1;

		// Clone the PlayerPrefab at the start location and assign it to the player's connection
		var player = PlayerPrefab.Clone(startLocation, name: $"Player - {channel.DisplayName}");
		player.NetworkSpawn(channel);

		// Set the player controller's name to match the player's display name
		var playerController = player.Components.Get<PlayerController>(FindMode.EverythingInSelfAndDescendants);
		playerController.GameObject.Name = channel.DisplayName;
	}

	/// <summary>
	/// Cleans up after a player disconnects, removing their player entity.
	/// </summary>
	/// <param name="channel">The player's connection channel.</param>
	public void OnDisconnected(Connection channel)
	{
		// Find and destroy the player entity associated with the disconnected Steam ID
		foreach (var player in Players)
		{
			if (player.SteamId == (long)channel.SteamId)
			{
				player.GameObject.Destroy();
			}
		}

		Connections.Remove(channel); // Remove the connection from the list
	}

	/// <summary>
	/// Handles the transition of host duties when the current host disconnects.
	/// </summary>
	/// <param name="previousHost">The previous host's connection channel.</param>
	public void OnBecameHost(Connection previousHost)
	{
		DestroyServer(); // Clean up server resources and return to the menu
	}

	/// <summary>
	/// Destroys the server and loads the menu scene. Broadcasted to all clients.
	/// </summary>
	[Broadcast]
	public void DestroyServer()
	{
		GameManager.ActiveScene.Load(menuScene); // Load the menu scene, effectively ending the game session
	}
}
