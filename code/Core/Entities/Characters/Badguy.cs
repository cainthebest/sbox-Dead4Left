using System;
using System.Linq;
using Kicks;
using Sandbox;
using Sandbox.Citizen;

/// <summary>
/// Represents a "bad guy" NPC with behavior for targeting, movement, and attacking a player.
/// </summary>
public sealed class Badguy : Component
{
	[Property] public SkinnedModelRenderer body { get; set; }
	[Property] public CitizenAnimationHelper citizenAnimationHelper { get; set; }
	[Property] public SoundEvent hitSound { get; set; }
	private PlayerController controller => Scene.GetAllComponents<PlayerController>().FirstOrDefault();
	public Vector3 target;
	public Vector3 WishVelocity;
	[Property] public float Speed { get; set; }
	[Property] public CharacterController characterController { get; set; }
	private TimeSince timeSinceHit = 0;
	private MP5 mP5 => Scene.GetAllComponents<MP5>().FirstOrDefault();

	/// <summary>
	/// Initializes NPC animations on start.
	/// </summary>
	protected override void OnStart()
	{
		citizenAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Swing;
	}

	/// <summary>
	/// Regularly updates NPC behavior including movement towards the player and attacking.
	/// </summary>
	protected override void OnUpdate()
	{
		target = controller.GameObject.Transform.Position; // Update the target position to the player's position.
		BuildWishVelocity(); // Calculate the desired velocity towards the player.
		UpdateMovement(); // Apply movement based on the calculated wish velocity.
		UpdateAnimations(); // Update animations based on current movement and actions.
		Trace(); // Perform a trace to detect if the player is within attack range.
	}

	/// <summary>
	/// Calculates the desired velocity towards the target.
	/// </summary>
	private void BuildWishVelocity()
	{
		WishVelocity = (target - body.Transform.Position).Normal;
		WishVelocity *= Speed;
	}

	/// <summary>
	/// Updates NPC's movement based on the wish velocity and applies gravity.
	/// </summary>
	private void UpdateMovement()
	{
		var gravity = Scene.PhysicsWorld.Gravity;
		characterController.ApplyFriction(GetFriction());

		if (characterController.IsOnGround)
		{
			characterController.Accelerate(WishVelocity);
			characterController.Velocity = characterController.Velocity.WithZ(0); // Ignore vertical velocity.
			body.Transform.Rotation = Rotation.LookAt(characterController.Velocity.WithZ(0), Vector3.Up);
		}
		else
		{
			characterController.Velocity += gravity * Time.Delta * 0.5f; // Apply gravity if in the air.
		}
		characterController.Move();
	}

	/// <summary>
	/// Calculates friction based on whether the NPC is on the ground.
	/// </summary>
	/// <returns>The friction value.</returns>
	private float GetFriction()
	{
		return characterController.IsOnGround ? 6.0f : 0.2f;
	}

	/// <summary>
	/// Updates NPC animations based on current velocity and direction.
	/// </summary>
	private void UpdateAnimations()
	{
		if (target != Vector3.Zero)
		{
			var targetRot = Rotation.LookAt(target.WithZ(Transform.Position.z) - Transform.Position, Vector3.Up);
			body.Transform.Rotation = Rotation.Slerp(body.Transform.Rotation, targetRot, Time.Delta * 10f);
		}
		citizenAnimationHelper.WithWishVelocity(WishVelocity);
		citizenAnimationHelper.WithVelocity(characterController.Velocity);
	}

	/// <summary>
	/// Performs a trace to detect if the player is within attacking range and applies damage if so.
	/// </summary>
	private void Trace()
	{
		var lookDir = body.Transform.Rotation;
		var tr = Scene.Trace.Ray(body.Transform.Position, body.Transform.Position + lookDir.Forward * 50).WithoutTags("bad").Run();
		if (tr.Hit && tr.GameObject.Tags.Has("player"))
		{
			if (timeSinceHit > 2.5f)
			{
				// Play hit sound and apply damage to the player.
				Sound.Play(hitSound, tr.GameObject.Transform.Position);
				timeSinceHit = 0;
				controller.TakeDamage(25); // Placeholder for applying damage to the player.
				citizenAnimationHelper.Target.Set("b_attack", true); // Trigger attack animation.
			}
		}
	}
}