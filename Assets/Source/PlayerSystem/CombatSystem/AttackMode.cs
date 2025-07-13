namespace Quinn.CombatSystem
{
	public enum AttackMode
	{
		/// <summary>
		/// The player doesn't move for th attack.
		/// </summary>
		Stationary,
		/// <summary>
		/// The player is pushed for the attack.
		/// </summary>
		Instant,
		/// <summary>
		/// The player dashes forward during the charge phase.
		/// </summary>
		Continuous
	}
}
