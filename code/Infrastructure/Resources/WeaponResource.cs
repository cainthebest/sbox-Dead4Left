using Sandbox;

/// <summary>
/// Defines a game resource for items, specifically for weapons in this context.
/// This resource includes properties for the weapon's prefab, name, damage, model,
/// ammo capacity, full ammo capacity, and fire rate.
/// </summary>
[GameResource("Item", "item", "A item game resource", Icon = "track_changes")]
public class Item : GameResource
{
	/// <summary>
	/// Prefab file for the weapon, allowing for instantiation in the game world.
	/// </summary>
	public PrefabFile weaponPrefab { get; set; }

	/// <summary>
	/// The name of the weapon.
	/// </summary>
	public string weaponName { get; set; }

	/// <summary>
	/// The damage output of the weapon. Limited to a range between 0 and 500.
	/// </summary>
	[Property, Range(0, 500)] public float weaponDamage { get; set; } = 50;

	/// <summary>
	/// The model used to visually represent the weapon in the game.
	/// </summary>
	public Model weaponModel { get; set; }

	/// <summary>
	/// The current ammo capacity of the weapon. Limited to a range between 0 and 120.
	/// </summary>
	[Property, Range(0, 120)] public float ammo { get; set; } = 30;

	/// <summary>
	/// The full ammo capacity of the weapon. Limited to a range between 0 and 120.
	/// </summary>
	[Property, Range(0, 120)] public float fullAmmo { get; set; } = 60;

	/// <summary>
	/// The fire rate of the weapon, indicating how fast it can shoot. Limited to a range between 0.1 and 1.
	/// </summary>
	[Property, Range(0.1f, 1)] public float fireRate { get; set; } = 0.1f;
}

