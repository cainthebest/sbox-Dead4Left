using System.Linq;
using Sandbox;

/// <summary>
/// A component that acts as a trigger for picking up weapons in the game. When a player enters the trigger,
/// a specified weapon can be added to their inventory.
/// </summary>
public sealed class PickUpTrigger : Component, Component.ITriggerListener
{
	[Property] public Weapon weapon { get; set; }
	[Property] public SoundEvent pickupSound { get; set; }
	[Property, ToggleGroup("Item")] public bool SpawnShotgun { get; set; } = false;
	[Property, ToggleGroup("Item")] public bool SpawnSmg { get; set; } = false;
	private string itemToPlace;

	/// <summary>
	/// Determines which weapon item to place based on the component's properties.
	/// </summary>
	protected override void OnAwake()
	{
		// Select the weapon type to spawn based on component properties
		if (SpawnShotgun)
		{
			itemToPlace = "weapon_shotgun";
		}
		if (SpawnSmg)
		{
			itemToPlace = "weapon_smg";
		}
	}

	/// <summary>
	/// Adds the specified weapon to the player's inventory when they enter the trigger area,
	/// provided they do not already possess the item.
	/// </summary>
	/// <param name="other">The collider of the entity entering the trigger.</param>
	void ITriggerListener.OnTriggerEnter(Sandbox.Collider other)
	{
		// Check if the entity is a player and if they do not already have the item in their inventory
		if (other.Tags.Has("player") && !weapon.Inventory.Contains(itemToPlace))
		{
			// Find an empty slot in the inventory to place the item
			for (int i = 0; i < weapon.Inventory.Length; i++)
			{
				if (string.IsNullOrEmpty(weapon.Inventory[i]))
				{
					weapon.Inventory[i] = itemToPlace; // Add the item to the inventory
					Sound.Play(pickupSound, GameObject.Transform.Position); // Play the pickup sound
					break; // Exit the loop once the item is added
				}
			}
		}
	}

	/// <summary>
	/// Currently, no action is taken when an entity exits the trigger area.
	/// </summary>
	/// <param name="other">The collider of the entity exiting the trigger.</param>
	void ITriggerListener.OnTriggerExit(Sandbox.Collider other)
	{
		// Actions for when an entity exits the trigger area could be implemented here if needed.
	}
}