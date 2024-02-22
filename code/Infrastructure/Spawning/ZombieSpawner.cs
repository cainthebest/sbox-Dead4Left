using System;
using System.Linq;
using Sandbox;
using Sandbox.Navigation;

/// <summary>
/// Manages spawning of zombie entities within a scene at random intervals and locations.
/// </summary>
public sealed class ZombieSpawner : Component
{
	/// <summary>
	/// Prefab for creating new zombie entities.
	/// </summary>
	[Property] public GameObject ZombiePrefab { get; set; }

	/// <summary>
	/// Generates a random float number between 1 and 100.
	/// </summary>
	/// <returns>A random float number.</returns>
	public float GetRandom() => Random.Shared.Float(1, 100);

	/// <summary>
	/// Array of current zombies in the scene.
	/// </summary>
	public Zombie[] zombies;

	/// <summary>
	/// Updates the zombies array with the current zombie components in the scene.
	/// </summary>
	protected override void OnUpdate()
	{
		zombies = Scene.GetAllComponents<Zombie>().ToArray();
	}

	/// <summary>
	/// Spawns a zombie at a random spawn point determined by the navigation mesh.
	/// </summary>
	void SpawnZombie()
	{
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		var randomSpawnPoint = Scene.NavMesh.GetRandomPoint().GetValueOrDefault();
		var zombie = ZombiePrefab.Clone(randomSpawnPoint);
		zombie.NetworkSpawn();
	}

	/// <summary>
	/// Timer for controlling the spawn rate of zombies.
	/// </summary>
	TimeUntil nextSecond = 0f;

	/// <summary>
	/// Spawns zombies based on a random time interval, ensuring the total number of zombies does not exceed 30.
	/// </summary>
	protected override void OnFixedUpdate()
	{
		float time = GetRandom();
		if (zombies is null) return;
		if (nextSecond && zombies.Length <= 30 && time > 80f)
		{
			SpawnZombie();
			nextSecond = 1; // Reset timer for the next spawn
			GetRandom(); // Generate a new random time for the next check
		}
	}
}