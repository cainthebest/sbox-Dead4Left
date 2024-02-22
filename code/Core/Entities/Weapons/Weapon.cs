using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using Sandbox;
using Sandbox.UI;

/// <summary>
/// Manages the weapon inventory for an entity, handling weapon selection and inventory management.
/// </summary>
public sealed class Weapon : Component
{
	[Property] public List<Texture> InventoryImages { get; set; } = new List<Texture>();
	[Property] public List<string> Weapons { get; set; } = new List<string>() { "weapon_smg" };
	[Property, ResourceType("item")] public List<Item> items { get; set; } = new List<Item>();

	public int ActiveSlot = 0;
	public int Slots => 9;
	public int Img => 9;
	public string[] Inventory = new string[9];
	private ActiveWeapon activeWeapon;

	/// <summary>
	/// Initializes the weapon inventory and sets the initial active weapon on component start.
	/// </summary>
	protected override void OnStart()
	{
		// Retrieve the ActiveWeapon component for managing the active weapon in the inventory
		activeWeapon = GameManager.ActiveScene.GetAllComponents<ActiveWeapon>().FirstOrDefault(x => !x.IsProxy);
		activeWeapon.Item = null; // Reset the active weapon item

		// Load player settings to configure initial inventory layout
		var playertext = Sandbox.FileSystem.Data.ReadAllText("player.txt").ToInt();
		Log.Info(playertext);

		// Configure the initial inventory based on the player settings
		SetupInitialInventory(playertext);
	}

	/// <summary>
	/// Updates the active weapon slot based on player input and handles weapon switching.
	/// </summary>
	protected override void OnUpdate()
	{
		if (IsProxy) return; // Skip update if this is a network proxy

		// Handle weapon slot changes based on mouse wheel input
		ChangeWeaponSlotBasedOnInput();

		// Update the active weapon item based on the current active slot
		UpdateActiveWeaponItem();
	}

	/// <summary>
	/// Sets up the initial inventory layout based on saved player settings.
	/// </summary>
	/// <param name="playertext">The player setting indicating the initial weapon configuration.</param>
	private void SetupInitialInventory(int playertext)
	{
		// Simplify the initial inventory setup by initializing all slots to empty and setting the first slot as needed
		for (int i = 0; i < Inventory.Length; i++) Inventory[i] = "";

		switch (playertext)
		{
			case 0:
			case 1:
				Inventory[0] = "weapon_smg";
				break;
			case 2:
				Inventory[0] = "weapon_pistol";
				break;
		}
	}

	/// <summary>
	/// Handles the logic for changing the weapon slot based on mouse wheel input.
	/// </summary>
	private void ChangeWeaponSlotBasedOnInput()
	{
		if (Input.MouseWheel.y != 0)
		{
			// Cycle through the inventory slots with wrap-around
			ActiveSlot = (ActiveSlot + Math.Sign(Input.MouseWheel.y) + Inventory.Length) % Inventory.Length;
			activeWeapon.weaponref.Destroy(); // Destroy the current weapon reference to trigger a change
			activeWeapon.NeedsChange = true; // Flag that a new weapon needs to be set up
		}
	}

	/// <summary>
	/// Updates the active weapon based on the current slot selection.
	/// </summary>
	private void UpdateActiveWeaponItem()
	{
		// Select the appropriate item from the items list based on the inventory slot's weapon identifier
		activeWeapon.Item = items.FirstOrDefault(item => item.weaponName == Inventory[ActiveSlot]);
	}
}