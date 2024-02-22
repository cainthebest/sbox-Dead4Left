using System.Linq;
using System.Runtime.CompilerServices;
using Kicks;
using Sandbox;
using Sandbox.Citizen;

/// <summary>
/// Controls navigation for AI entities using the navigation mesh, guiding them towards a player controller.
/// </summary>
public sealed class NavMeshController : Component
{
	public NavMeshAgent agent;
	private Vector3 _destination;
	public PlayerController playerController;
	public Vector3 testVector = new Vector3(0, 0, 0);
	RealTimeSince timeSinceUpdate = 0;

	/// <summary>
	/// Initializes the navigation mesh agent and finds the first player controller in the scene.
	/// </summary>
	protected override void OnAwake()
	{
		// Retrieve the NavMeshAgent component attached to the same GameObject
		agent = Components.Get<NavMeshAgent>();
		// Find the first PlayerController component within the scene
		playerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault();

		// Commented out to avoid null reference in OnAwake, I think? - cainthebest
		//_destination = playerController.Transform.Position;
	}

	/// <summary>
	/// Updates the destination each frame to follow the player controller's position.
	/// </summary>
	protected override void OnUpdate()
	{
		// Continuously update the destination to the player's current position
		_destination = playerController.Transform.Position;
	}

	/// <summary>
	/// Regularly updates the agent's movement towards the destination with a fixed time interval.
	/// </summary>
	protected override void OnFixedUpdate()
	{
		// Orient the GameObject to face towards the destination
		GameObject.Transform.Rotation = Rotation.LookAt(_destination - GameObject.Transform.Position);
		// Move the agent towards the destination at fixed intervals to smooth out the movement
		if (timeSinceUpdate > 0.1 && agent != null)
		{
			timeSinceUpdate = 0; // Reset the timer
			agent.MoveTo(_destination); // Command the agent to move towards the updated destination
		}
	}
}