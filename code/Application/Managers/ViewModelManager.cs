using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Kicks;
using Sandbox;


/// <summary>
/// Manages the view model for weapons and arms, including switching between different weapon models and animations.
/// </summary>
public sealed class ViewModelManager : Component
{
	[Property] SkinnedModelRenderer weaponRender { get; set; }
	[Property] public SkinnedModelRenderer armRender { get; set; }
	[Property] Model mp5Model { get; set; }
	[Property] Model shotgunModel { get; set; }
	[Property] Model fistsModel { get; set; }
	[Property] public GameObject eye { get; set; }
	private Weapon weapon;
	private Vector3 StartPos;
	[Property] public AnimationGraph punchGraph { get; set; }
	[Property] public AnimationGraph normalGraph { get; set; }
	PlayerController playerController;
	public Model blankModel;

	/// <summary>
	/// Initializes component state when the component awakens.
	/// </summary>
	protected override void OnAwake()
	{
		// Record the initial local position of the game object
		StartPos = GameObject.Transform.LocalPosition;
	}

	/// <summary>
	/// Initializes the view model manager, finding the active weapon and player controller in the scene.
	/// </summary>
	protected override void OnStart()
	{
		// Find the active weapon and player controller components in the scene, excluding proxies
		weapon = GameManager.ActiveScene.GetAllComponents<Weapon>().FirstOrDefault(x => !x.IsProxy);
		playerController = GameManager.ActiveScene.GetAllComponents<PlayerController>().FirstOrDefault(x => !x.IsProxy);
	}

	/// <summary>
	/// Updates the view model based on the current weapon selection and player actions.
	/// </summary>
	protected override void OnUpdate()
	{
		if (playerController is null) return;

		// Align the game object's rotation with the player's eye angles
		GameObject.Transform.Rotation = playerController.EyeAngles;

		// Show fists or the SMG model based on the active weapon slot
		if (weapon.Inventory[weapon.ActiveSlot] != "weapon_smg")
		{
			ShowFists();
		}
		else
		{
			ShowSmg();
		}

		// Enable or disable the game object based on whether this is a proxy
		foreach (var arms in GameObject.Components.GetAll<ModelRenderer>(FindMode.EverythingInSelfAndDescendants))
		{
			GameObject.Enabled = !IsProxy;
		}
	}

	/// <summary>
	/// Shows the SMG model, enabling the weapon render and setting the appropriate model.
	/// </summary>
	void ShowSmg()
	{
		weaponRender.Enabled = true;
		armRender.BoneMergeTarget = weaponRender;
		weaponRender.Model = mp5Model;
		armRender.Enabled = true;
		GameObject.Transform.LocalPosition = StartPos;
	}

	/// <summary>
	/// Shows the fists, applying the punch animation graph and hiding the weapon render.
	/// </summary>
	void ShowFists()
	{
		armRender.SceneModel.AnimationGraph = punchGraph;
		armRender.BoneMergeTarget = null;
		weaponRender.Enabled = false;
		GameObject.Transform.LocalPosition = new Vector3(StartPos.x, 0, StartPos.z - 2);
	}
}