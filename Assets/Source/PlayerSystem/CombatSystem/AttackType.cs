namespace Quinn
{
	public enum AttackType
	{
		/// <summary>
		/// An attack that begins a chain.
		/// </summary>
		Starter,
		/// <summary>
		/// An attack that follows a <see cref="Starter"/> attack, but doesn't end a chain.
		/// </summary>
		Chain,
		/// <summary>
		/// An attack that consumes the last attack points.
		/// </summary>
		Finisher
	}
}
