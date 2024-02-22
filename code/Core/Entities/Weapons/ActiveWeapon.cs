using System.Linq;
using Kicks;
using Sandbox;

/// <summary>
/// Manages the active weapon for an entity, facilitating the swapping and initialization of weapon prefabs
/// based on a specified Item resource.
/// </summary>
public sealed class ActiveWeapon : Component
{
	[Property, ResourceType("item")] public Item Item { get; set; }
	[Property] public SkinnedModelRenderer body { get; set; }
	public GameObject weaponref;
	public GameObject viewmodelref;
	public bool NeedsChange = true;

	/// <summary>
	/// Checks if a weapon change is needed and applies the change by cloning the specified weapon prefab
	/// and setting it as a child of the specified body part.
	/// </summary>
	protected override void OnUpdate()
	{
		// Check if a change is needed and an item is specified
		if (NeedsChange && Item != null)
		{
			// Retrieve the prefab scene for the weapon from the Item resource
			var weapon = SceneUtility.GetPrefabScene(Item.weaponPrefab);
			// Optionally retrieve a ModularWeapon component from the prefab if specific operations are needed
			var modularWeapon = weapon?.Components.Get<ModularWeapon>();

			// Clone the weapon prefab to create a new instance in the scene
			var weaponClone = weapon?.Clone();

			// Reference the cloned weapon for potential future use
			weaponref = weaponClone;

			// Set the cloned weapon as a child of the specified body part to attach it to the entity
			weaponClone?.SetParent(body.GameObject);

			// After setting up the new weapon, indicate that no further change is needed until next trigger
			NeedsChange = false;
		}
	}
}