namespace Quinn
{
	public enum AttackPhase
	{
		None,
		/// <summary>
		/// The wind-up before the actual swing.
		/// </summary>
		Charging,
		/// <summary>
		/// The entire animation of the swing.<br/>
		/// Damage is usually dealt at the start of this phase.
		/// </summary>
		Attacking,
		Recovering
	}
}
