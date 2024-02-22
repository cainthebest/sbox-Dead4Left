using Sandbox;

/// <summary>
/// A component that listens for mouse clicks ("attack1") and performs a ray trace from the camera through the mouse position.
/// If a "zombie" tagged object is hit, it spawns a blood particle at the hit location, plays a sound, and destroys the zombie.
/// </summary>
public sealed class EasterEggTrace : Component
{
	[Property] public GameObject bloodParticle { get; set; }
	[Property] public SoundEvent soundEvent { get; set; }

	/// <summary>
	/// Checks each frame for a mouse click and performs a ray trace from the camera.
	/// Destroys the hit "zombie" object, plays a sound, and spawns a blood particle effect.
	/// </summary>
	protected override void OnUpdate()
	{
		// Perform a ray trace from the camera through the mouse position.
		var tr = Scene.Trace.Ray(Scene.Camera.ScreenPixelToRay(Mouse.Position), 10000f).Run();

		// Check if the left mouse button was pressed, the trace hit something, and the hit object has a "zombie" tag.
		if (Input.Pressed("attack1") && tr.Hit && tr.GameObject.Tags.Has("zombie"))
		{
			Log.Info(tr.GameObject); // Log the hit GameObject for debugging purposes.
			bloodParticle.Clone(tr.HitPosition); // Spawn a blood particle effect at the hit position.
			soundEvent.UI = true; // Set the sound event to play through the UI channel (may vary depending on desired effect).
			var sound = Sound.Play(soundEvent); // Play the defined sound event.
			tr.GameObject.Parent.Destroy(); // Destroy the parent GameObject of the hit object, assuming it's the zombie entity.
		}
	}
}
