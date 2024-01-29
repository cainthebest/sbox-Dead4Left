using System;
using System.Linq;
using Sandbox;

public sealed class MP5 : Component
{
	[Property] public SkinnedModelRenderer gun { get; set; }
	[Property] public GameObject impact { get; set; }
	[Property] public GameObject eye { get; set; }
	[Property] public SoundEvent gunSound { get; set; }
	[Property] public SoundEvent reloadSound { get; set; }
	[Property] public GameObject decalGo { get; set; }
	[Property] public float ammo { get; set; } = 30;
	[Property] public float fullAmmo { get; set; } = 60;
	[Property] public float ShootDamage { get; set; } = 10;
	[Property] public GameObject zombieRagdoll { get; set; }
	public TimeSince timeSinceShoot = 0;
	Manager manager => Scene.GetAllComponents<Manager>().FirstOrDefault();
	bool ableToShoot;
	bool reloading;
	public TimeSince timeSinceReload = 3;
	
	protected override void OnUpdate()
	{
		if (fullAmmo < 0)
		{
			fullAmmo = 0;
		}
		
		if (Input.Pressed("reload") && fullAmmo != 0 && ammo != 30)
		{
			gun.Set("b_reload", true);
			fullAmmo -= 30;
			ammo = 30;
			timeSinceReload = 1;
			Sound.Play(reloadSound, GameObject.Transform.Position);
		}

	}
	protected override void OnFixedUpdate()
	{
		
		if (Input.Down("attack1") && ammo > 0 && timeSinceReload > 3 && timeSinceShoot > 0.1)
		{

			Shoot();
			gun.Set("b_attack", true);
			timeSinceShoot = 0;
		}

	}
	void Shoot()
	{
		var attachment = gun.GetAttachment( "muzzle" );
		
		var ray = Scene.Camera.ScreenNormalToRay( 0.5f );
		var tr = Scene.Trace.Ray( eye.Transform.Position, eye.Transform.Position + eye.Transform.Rotation.Forward * 8000).WithoutTags("player").Run();
		if (tr.Hit)
		{
			ammo -= 1;
			Log.Info(tr.GameObject);
			impact.Clone(tr.HitPosition);
			Sound.Play(gunSound, GameObject.Transform.Position);
			var decal = decalGo.Clone(new Transform(tr.HitPosition + tr.Normal * 2.0f, Rotation.LookAt(-tr.Normal, Vector3.Random), Random.Shared.Float(0.8f, 1.2f)));
			decal.SetParent( tr.GameObject );
			if (tr.GameObject.Tags.Has("bad"))
			{
				tr.GameObject.Parent.Destroy();
				manager.AddScore();
				fullAmmo += 15;
				
			}
			var damage = new DamageInfo( ShootDamage, GameObject, GameObject );
			
		if ( tr.Body is not null )
		{
			tr.Body.ApplyImpulseAt( tr.HitPosition, tr.Direction * 200.0f * tr.Body.Mass.Clamp( 0,200 ) );
		}

		foreach ( var damageable in tr.GameObject.Components.GetAll<IDamageable>() )
		{
			damageable.OnDamage( damage );
		}
		}
	}
}
