using Sandbox;

/// <summary>
/// Represents an ammo pickup component that interacts with players or entities that enter its trigger area.
/// Designed to provide additional ammunition for the MP5 weapon upon collision.
/// </summary>
public sealed class AmmoPickup : Component, Component.ITriggerListener
{
	/// <summary>
	/// Updates the component state each frame. Currently, this method is empty as the component's primary functionality
	/// is handled through trigger interactions.
	/// </summary>
	protected override void OnUpdate()
	{
		// Frame-based logic for the ammo pickup can be implemented here, if needed.
	}

	/// <summary>
	/// Triggered when an entity enters the trigger area of this ammo pickup. If the entity has an MP5 component,
	/// this method increases its ammunition.
	/// </summary>
	/// <param name="other">The collider of the entity that entered the trigger area.</param>
	void ITriggerListener.OnTriggerEnter(Sandbox.Collider other)
	{
		// Ensure the collider's GameObject is valid to prevent interactions with invalid or destroyed objects.
		if (!other.GameObject.IsValid) return;

		// Attempt to retrieve the MP5 component from the collider or its parent.
		var ammo = other.GameObject.Components.GetInParentOrSelf<MP5>();

		// If an MP5 component is found and is valid, increase its ammo.
		if (ammo.IsValid)
		{
			ammo.fullAmmo += 30; // Increase the MP5's full ammo by 30 units.
			Log.Info($"Ammo picked up by {other.GameObject.Name}. New full ammo: {ammo.fullAmmo}"); // Log the ammo pickup event.
		}
	}

	/// <summary>
	/// Triggered when an entity exits the trigger area of this ammo pickup.
	/// Currently, this method does not perform any actions when entities exit the trigger area.
	/// </summary>
	/// <param name="other">The collider of the entity that exited the trigger area.</param>
	void ITriggerListener.OnTriggerExit(Sandbox.Collider other)
	{
		// Actions to perform when an entity exits.
	}
}