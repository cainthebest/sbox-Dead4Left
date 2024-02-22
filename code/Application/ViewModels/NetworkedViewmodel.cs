using System.Linq;
using System.Threading;
using Kicks;
using Sandbox;

/// <summary>
/// Manages the networked view model for weapons and arms, including animations and positioning.
/// </summary>
public sealed class NetworkedViewmodel : Component
{
	[Property] public SkinnedModelRenderer arms { get; set; }
	[Property] public SkinnedModelRenderer gun { get; set; }
	[Property] public GameObject eye { get; set; }
	[Property] public Weapon weapon { get; set; }
	[Property] public AnimationGraph punchGraph { get; set; }
	[Property] public AnimationGraph normalGraph { get; set; }
	[Property] public Model smgModel { get; set; }
	PlayerController playerController;
	public Vector3 StartPos;
	public Vector3 ArmsPos;

	/// <summary>
	/// Initializes the component, setting up references to the player controller and initial positions.
	/// </summary>
	protected override void OnStart()
	{
		// Find the player controller in the scene, excluding proxies
		playerController = GameManager.ActiveScene.GetAllComponents<PlayerController>().FirstOrDefault(x => !x.IsProxy);
		// Record the initial local position of the game object
		StartPos = GameObject.Transform.LocalPosition;
		// Set the initial local position offset for the arms
		ArmsPos = new Vector3(3.4f, 6.3f, -2f);
	}

	/// <summary>
	/// Updates the view model position, rotation, and active model based on the current weapon.
	/// </summary>
	protected override void OnUpdate()
	{
		// Disable rendering shadows for arms and gun
		arms.RenderType = ModelRenderer.ShadowRenderType.Off;
		gun.RenderType = ModelRenderer.ShadowRenderType.Off;

		if (weapon.Inventory[weapon.ActiveSlot] == "weapon_smg")
		{
			// Configure view model for SMG
			gun.Model = smgModel;
			arms.BoneMergeTarget = gun;
			arms.SceneModel.AnimationGraph = normalGraph;
			// Lerp rotation towards the player's eye angles for smooth aiming
			GameObject.Transform.Rotation = Rotation.Lerp(GameObject.Transform.Rotation, playerController.EyeAngles, Time.Delta * 100f);
			gun.Enabled = true;
			// Reset positions to align with the player's view
			GameObject.Transform.LocalPosition = StartPos;
			gun.Transform.LocalPosition = Vector3.Zero;
			arms.Transform.LocalPosition = Vector3.Zero;
		}

		if (weapon.Inventory[weapon.ActiveSlot] == "")
		{
			// Configure view model for fists/punching
			gun.Enabled = false;
			arms.BoneMergeTarget = null;
			arms.SceneModel.AnimationGraph = punchGraph;
			// Adjust arms position for punching animation
			arms.Transform.LocalPosition = ArmsPos;
			// Lerp rotation towards the player's eye angles for consistent aiming
			GameObject.Transform.Rotation = Rotation.Lerp(GameObject.Transform.Rotation, playerController.EyeAngles, Time.Delta * 100f);
		}

		// Disable the game object if this is a proxy to prevent rendering on remote clients
		if (IsProxy)
		{
			GameObject.Enabled = false;
		}
	}
}