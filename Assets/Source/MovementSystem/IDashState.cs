namespace Quinn.MovementSystem
{
	/// <summary>
	/// Can be implemented by any class that needs to know if it's currently dashing.
	/// </summary>
	public interface IDashState
	{
		public bool IsDashing { get; }
	}
}
