using Sandbox;

/// <summary>
/// Manages the viewmodel, particularly the positioning and rotation of the arms during specific actions like ducking.
/// </summary>
public sealed class Viewmodel : Component
{
	[Property] public GameObject eye { get; set; }
	[Property] public GameObject arms { get; set; }
	public Rotation rotation;
	public Vector3 vector3;

	/// <summary>
	/// Captures the initial local rotation and position of the arms when the component is first initialized.
	/// </summary>
	protected override void OnAwake()
	{
		// Store the initial local rotation and position of the arms for later use
		rotation = arms.Transform.LocalRotation;
		vector3 = arms.Transform.LocalPosition;
	}

	/// <summary>
	/// Updates the position and rotation of the arms based on player input, specifically for the ducking action.
	/// </summary>
	protected override void OnUpdate()
	{
		// If the Duck key is pressed, move the arms down to simulate ducking
		if (Input.Pressed("Duck"))
		{
			arms.Transform.Position -= Vector3.Down * -32;
		}

		// When the Duck key is released, reset the arms' position and rotation to their original state
		if (Input.Released("Duck"))
		{
			arms.Transform.LocalPosition = vector3;
			arms.Transform.LocalRotation = rotation;
		}
	}
}