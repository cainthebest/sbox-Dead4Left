using System;
using System.Linq;
using Sandbox;
using Sandbox.Citizen;
namespace Kicks;

/// <summary>
/// Represents a player controller component.
/// Implementing health and score components along with movement and animation.
/// </summary>
public sealed class PlayerController : Component, IHealthComponent, IScoreComponent
{
	// Synchronized properties between server and client
	[Sync] public float Health { get; set; } = 100;
	[Sync] public float MaxHealth { get; set; } = 100;
	[Sync] public long Score { get; set; } = 0;
	[Sync] public long HighScore { get; set; } = 0;
	[Sync] public long SteamId { get; set; }
	[Sync] public bool Crouching { get; set; }
	[Sync] public Angles EyeAngles { get; set; }
	[Sync] public Vector3 WishVelocity { get; set; }

	// Exposed properties for game object configuration
	[Property] public float CrouchSpeed { get; set; } = 64.0f;
	[Property] public float WalkSpeed { get; set; } = 190f;
	[Property] public float RunSpeed { get; set; } = 190f;
	[Property] public float SprintSpeed { get; set; } = 320f;
	[Property] public GameObject eye { get; set; }
	[Property] public SkinnedModelRenderer body { get; set; }
	[Property] public CharacterController controller { get; set; }
	[Property] public CitizenAnimationHelper animationHelper { get; set; }
	[Property] public Weapon weapon { get; set; }
	[Property] public float EyeHight = 64;

	// Non-synchronized fields for internal logic
	public bool WishCrouch;
	public CameraComponent camera;
	public RealTimeSince jumpTime;
	private float CrouchHeight = 64 - 36;

	/// <summary>
	/// Initializes the player controller component, setting the player's position to a random spawn point.
	/// </summary>
	protected override void OnStart()
	{
		// Select a random spawn point from available spawn points in the scene
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		var randomSpawnPoint = Random.Shared.FromArray(spawnPoints);
		// Set player's position to the selected spawn point
		GameObject.Transform.Position = randomSpawnPoint.Transform.Position;
	}

	/// <summary>
	/// Updates the player controller each frame, handling input and updating the player's view.
	/// </summary>
	protected override void OnUpdate()
	{
		if (!IsProxy)
		{
			// Process mouse input for looking around
			MouseInput();
			// Update player's rotation based on eye angles, only yaw is used for body orientation
			Transform.Rotation = new Angles(0, EyeAngles.yaw, 0);
		}
		// Update player animations based on movement and actions
		UpdateAnimation();
		// Adjust eye rotation based on mouse movement deltas
		var eyeRot = eye.Transform.Rotation.Angles();
		eyeRot.pitch = Input.MouseDelta.y;
		eyeRot.yaw = Input.MouseDelta.x;
	}

	/// <summary>
	/// Handles fixed update logic for movement and crouching.
	/// </summary>
	protected override void OnFixedUpdate()
	{
		if (IsProxy) return; // Skip update for proxy instances

		// Process movement and crouching actions
		Movement();
		Crouch();
	}

	/// <summary>
	/// Processes mouse input to adjust the player's eye angles.
	/// </summary>
	private void MouseInput()
	{
		// Adjust eye angles based on analog look input, clamping pitch for realistic limits
		var e = EyeAngles;
		e += Input.AnalogLook;
		e.pitch = e.pitch.Clamp(-90, 90);
		e.roll = 0; // Reset roll to maintain upright orientation
		EyeAngles = e;
	}

	/// <summary>
	/// Calculates the current move speed based on player state and input.
	/// </summary>
	private float CurrentMoveSpeed => Crouching ? CrouchSpeed : Input.Down("run") ? SprintSpeed : Input.Down("walk") ? WalkSpeed : RunSpeed;

	/// <summary>
	/// Determines the friction value based on whether the player is on the ground or in the air.
	/// </summary>
	/// <returns>The friction coefficient.</returns>
	private float GetFriction() => controller.IsOnGround ? 6.0f : 0.2f; // Ground vs air friction

	/// <summary>
	/// Handles the logic for crouching, including adjusting the character controller's height.
	/// </summary>
	private void Crouch()
	{
		// Toggle crouch state and adjust controller height based on input and ability to uncrouch
		if (Input.Down("duck") && CanUncrouch())
		{
			// Reduce controller height for crouching
			controller.Height = 36;
			Crouching = true;
		}
		else
		{
			// Reset controller height when not crouching
			Crouching = false;
			controller.Height = 64;
		}
	}

	/// <summary>
	/// Manages the player's movement, including jumping and applying friction.
	/// </summary>
	private void Movement()
	{
		if (controller is null) return; // Ensure we have a controller to manage movement

		// Apply gravity adjustment for smoother jump and fall
		Vector3 halfgrav = Scene.PhysicsWorld.Gravity * Time.Delta * 0.5f;

		// Determine movement direction based on input
		WishVelocity = Input.AnalogMove;
		if (Input.Pressed("jump") && controller.IsOnGround)
		{
			// Apply an upward force for jumping
			controller.Punch(Vector3.Up * 300);
		}

		if (!WishVelocity.IsNearlyZero())
		{
			// Adjust wish velocity based on eye orientation and move speed
			WishVelocity = new Angles(0, EyeAngles.yaw, 0).ToRotation() * WishVelocity;
			WishVelocity = WishVelocity.WithZ(0); // Remove vertical component
			WishVelocity = WishVelocity.ClampLength(1); // Normalize to prevent excessive speed
			WishVelocity *= CurrentMoveSpeed; // Scale by current move speed

			if (!controller.IsOnGround)
			{
				// Limit air control to prevent unrealistic air maneuvering
				WishVelocity = WishVelocity.ClampLength(50);
			}
		}

		// Apply ground friction or air resistance
		controller.ApplyFriction(GetFriction());

		// Handle acceleration and jumping logic
		if (controller.IsOnGround)
		{
			// Accelerate based on wish velocity and reset jump timer
			controller.Accelerate(WishVelocity);
			controller.Velocity = controller.Velocity.WithZ(0); // Prevent accumulation of vertical speed
			jumpTime = 0;
		}
		else
		{
			// Apply gravity and accelerate in air
			controller.Velocity += halfgrav;
			controller.Accelerate(WishVelocity);
		}

		// Perform movement based on velocity
		controller.Move();
		if (!controller.IsOnGround)
		{
			// Apply additional gravity if still in air after movement calculations
			controller.Velocity += halfgrav;
		}
		else
		{
			// Reset vertical velocity when on ground
			controller.Velocity = controller.Velocity.WithZ(0);
		}
	}

	/// <summary>
	/// Determines if the player can uncrouch by checking for obstacles above.
	/// </summary>
	/// <returns>true if uncrouching is possible; otherwise, false.</returns>
	private bool CanUncrouch()
	{
		if (!Crouching) return true; // If not crouching, no need to check

		// Perform a trace upwards to check for clearance
		var tr = controller.TraceDirection(Vector3.Up * CrouchHeight);
		return !tr.Hit; // Return true if no obstacles are found
	}

	/// <summary>
	/// Updates the camera position and rotation to match the player's eye height and angles.
	/// </summary>
	private void UpdateCamera()
	{
		// Find the main camera component in the scene
		camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault(x => x.IsMainCamera);
		if (camera is null) return; // Skip if no main camera is found

		// Adjust eye height based on crouching state
		var targetEyeHeight = Crouching ? 32 : 64;
		EyeHight = EyeHight.LerpTo(targetEyeHeight, RealTime.Delta * 10.0f);

		// Calculate target camera position based on player position and eye height
		var targetCameraPos = Transform.Position + new Vector3(0, 0, EyeHight);

		// Update camera position and rotation to follow the player's view
		eye.Transform.Rotation = EyeAngles;
		camera.Transform.Position = targetCameraPos;
		camera.Transform.Rotation = EyeAngles;
	}

	/// <summary>
	/// Prepares the player model for rendering before each frame is drawn.
	/// </summary>
	protected override void OnPreRender()
	{
		// Update visibility of player model components
		UpdateBodyVisibility();

		if (IsProxy)
			return; // Skip further updates for proxy instances

		// Update camera to reflect current player state
		UpdateCamera();
	}

	/// <summary>
	/// Updates the visibility of the player's body and associated components.
	/// </summary>
	private void UpdateBodyVisibility()
	{
		if (animationHelper is null) return; // Skip if animation helper is not set

		// Determine render mode based on whether this is a proxy instance
		var renderMode = IsProxy ? ModelRenderer.ShadowRenderType.On : ModelRenderer.ShadowRenderType.ShadowsOnly;

		// Apply render mode to the player model and clothing components
		animationHelper.Target.RenderType = renderMode;
		foreach (var clothing in animationHelper.Target.Components.GetAll<ModelRenderer>(FindMode.InChildren))
		{
			if (!clothing.Tags.Has("clothing"))
				continue; // Skip non-clothing components

			// Apply render mode to clothing components
			clothing.RenderType = renderMode;
		}
	}

	/// <summary>
	/// Updates animation parameters based on current player state and actions.
	/// </summary>
	private void UpdateAnimation()
	{
		if (animationHelper is null) return; // Skip if animation helper is not set

		// Update animation helper with movement and state information
		var wv = WishVelocity.Length;
		animationHelper.WithWishVelocity(WishVelocity);
		animationHelper.WithVelocity(controller.Velocity);
		animationHelper.IsGrounded = controller.IsOnGround;
		animationHelper.DuckLevel = Crouching ? 1.0f : 0.0f;

		// Determine movement style based on velocity
		animationHelper.MoveStyle = wv < 160f ? CitizenAnimationHelper.MoveStyles.Walk : CitizenAnimationHelper.MoveStyles.Run;

		// Update look direction for animations
		var lookDir = EyeAngles.ToRotation().Forward * 1024;
		animationHelper.WithLook(lookDir, 1, 0.5f, 0.25f);
	}

	/// <summary>
	/// Applies damage to the player, reducing health.
	/// </summary>
	/// <param name="damage">The amount of damage to apply.</param>
	public void TakeDamage(float damage)
	{
		Health -= damage; // Subtract damage from health
		Log.Info(Health); // Log current health for debugging
	}

	/// <summary>
	/// Increases the player's score.
	/// </summary>
	/// <param name="addScore">The amount to add to the player's score.</param>
	public void AddScore(long addScore)
	{
		Score += addScore; // Add to the player's score
	}
}
