namespace Quinn.DamageSystem
{
	public interface IDamageable
	{
		/// <summary>
		/// Attempt to apply damage.
		/// </summary>
		/// <param name="info">The description of the damage to try and apply.</param>
		/// <returns>True, if the damage was applied, false if it was ignored.</returns>
		public bool TakeDamage(DamageInfo info);
	}
}
