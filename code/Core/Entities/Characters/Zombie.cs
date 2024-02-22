using System.Linq;
using System.Threading.Tasks;
using Kicks;
using Sandbox;
using Sandbox.Citizen;

/// <summary>
/// Represents a zombie entity with behaviors such as targeting players, moving, jumping, and attacking.
/// </summary>
public sealed class Zombie : Component, IHealthComponent
{
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	[Property] public NavMeshAgent NavMeshAgent { get; set; }
	[Property] public CharacterController CharacterController { get; set; }
	[Property] public GameObject body { get; set; }
	[Property] public SoundEvent hitSound { get; set; }
	[Sync] public float Health { get; set; } = 100;
	[Sync] public float MaxHealth { get; set; } = 100;
	public PlayerController targetPlayer;
	public bool NeedsToJump = false;

	/// <summary>
	/// Sets initial animation states and targets a random player.
	/// </summary>
	protected override void OnStart()
	{
		// Initialize animation helper with movement and attack types
		AnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Auto;
		AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Punch;

		// Target a random player from the list of players
		var players = GameManager.ActiveScene.GetAllComponents<PlayerController>().ToList();
		targetPlayer = Game.Random.FromList(players);
		Log.Info($"Targeting {targetPlayer}");

		// Initially disable character controller
		CharacterController.Enabled = false;
	}

	/// <summary>
	/// Updates the zombie's behavior, such as moving towards and attacking the target player.
	/// </summary>
	protected override void OnUpdate()
	{
		// Move towards the player if they are beyond a certain distance
		if (Vector3.DistanceBetween(targetPlayer.Transform.Position, body.Transform.Position) >= 150f)
		{
			MoveToTarget();
		}
		else
		{
			NavMeshAgent?.Stop(); // Stop the nav mesh agent when close enough
		}

		// Check elevation for jump requirement
		var target = targetPlayer.Transform.Position;
		if (target.z > GameObject.Transform.Position.z + 50)
		{
			UpWardTrace(); // Trace upwards to detect if jumping is needed
		}
		else
		{
			FowardTrace(); // Perform a forward trace to attack the player
		}

		UpdateAnimations(); // Update zombie animations based on movement
	}

	/// <summary>
	/// Commands the zombie to move towards the current target player.
	/// </summary>
	void MoveToTarget()
	{
		NavMeshAgent?.MoveTo(targetPlayer.Transform.Position);
	}

	/// <summary>
	/// Updates the zombie's animations based on its velocity and orientation towards the target player.
	/// </summary>
	void UpdateAnimations()
	{
		// Calculate the velocity and direction towards the target for animation
		var velocity = NavMeshAgent.Enabled ? NavMeshAgent.Velocity : CharacterController.Velocity;
		var targetRot = Rotation.LookAt(targetPlayer.Transform.Position.WithZ(Transform.Position.z) - Transform.Position);
		body.Transform.Rotation = Rotation.Slerp(body.Transform.Rotation, targetRot, Time.Delta * 5);
		AnimationHelper?.WithVelocity(velocity);
	}

	public TimeSince lastAttack = 0;

	/// <summary>
	/// Performs a forward trace to detect and attack the player if within range.
	/// </summary>
	void FowardTrace()
	{
		// Trace forward to detect the player and attack if close enough
		var tr = Scene.Trace.Ray(body.Transform.Position, body.Transform.Position + Vector3.Up * 64 + body.Transform.Rotation.Forward * 150).Run();
		if (tr.Hit && tr.GameObject.Tags.Has("player") && lastAttack > 1.5f)
		{
			AnimationHelper.Target.Set("b_attack", true); // Trigger attack animation
			targetPlayer.TakeDamage(10); // Deal damage to the player
			lastAttack = 0; // Reset attack timer
			Sound.Play(hitSound); // Play attack sound
		}
	}

	/// <summary>
	/// Traces upward to detect if an attack or jump is possible based on the player's elevation.
	/// </summary>
	void UpWardTrace()
	{
		// Similar to FowardTrace but targeting upward direction for elevated targets
		var tr = Scene.Trace.Ray(body.Transform.Position, body.Transform.Position + Vector3.Up * 64 + body.Transform.Rotation.Up * 200).Run();
		if (tr.Hit && tr.GameObject.Tags.Has("player") && lastAttack > 1.5f)
		{
			AnimationHelper.Target.Set("b_attack", true); // Trigger attack animation for upward attack
			targetPlayer.TakeDamage(10); // Deal damage to the player
			lastAttack = 0; // Reset attack timer
			Sound.Play(hitSound); // Play attack sound for upward attack
		}
	}

	/// <summary>
	/// Reduces the zombie's health upon taking damage and destroys the zombie if health is depleted.
	/// </summary>
	/// <param name="damage">The amount of damage to apply.</param>
	public void TakeDamage(float damage)
	{
		Health -= damage; // Subtract damage from health
		if (Health <= 0)
		{
			GameObject.Destroy(); // Destroy the zombie if health falls to zero or below
		}
	}

	/// <summary>
	/// Checks if the zombie needs to perform a jump based on obstacles in its path.
	/// </summary>
	void JumpTrace()
	{
		// Perform a trace to detect if jumping is required to overcome obstacles
		var tr = Scene.Trace.Ray(body.Transform.Position, body.Transform.Position + Vector3.Up * 10 + body.Transform.Rotation.Forward * 5).WithoutTags("player").Run();
		NeedsToJump = tr.Hit; // Set jump flag based on trace result
		if (NeedsToJump)
		{
			Log.Info("Needs to jump"); // Log jump requirement for debugging
		}
	}
}