using System.Linq;
using System.Runtime.CompilerServices;
using Kicks;
using Sandbox;

public sealed class NavMeshController : Component
{
	
	public NavMeshAgent agent;
	private Vector3 _destination;
	public CharacterController playerController;
	RealTimeSince timeSinceUpdate = 0;
	protected override void OnAwake()
	{
		agent = Components.Get<NavMeshAgent>();
		playerController = Scene.GetAllComponents<CharacterController>().FirstOrDefault();
		_destination = playerController.Transform.Position;
	}

	protected override void OnUpdate()
	{
		_destination = playerController.Transform.Position;
		
		if ( timeSinceUpdate > 1 && agent != null)
		{
			timeSinceUpdate = 0;
			agent.MoveTo( _destination );
		}
		if (Vector3.DistanceBetween(_destination, GameObject.Transform.Position) < 200 && agent != null)
		{
			Log.Info("Reached Destination");
		}
}
}

