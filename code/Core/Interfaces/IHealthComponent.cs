using System;
using Sandbox;

namespace Kicks
{
	/// <summary>
	/// Defines a contract for components that manage health, supporting operations such as taking damage.
	/// </summary>
	public interface IHealthComponent
	{
		/// <summary>
		/// Gets the maximum health value.
		/// </summary>
		public float MaxHealth { get; }

		/// <summary>
		/// Gets the current health value.
		/// </summary>
		public float Health { get; }

		/// <summary>
		/// Applies damage to the component, reducing its health.
		/// </summary>
		/// <param name="damage">The amount of damage to apply.</param>
		public void TakeDamage(float damage);
	}
}
