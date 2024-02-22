using Sandbox;

namespace Kicks;

/// <summary>
/// Defines a contract for components that manage scoring, including operations for adding to the score and tracking high scores.
/// </summary>
public interface IScoreComponent
{
	/// <summary>
	/// Gets the current score value.
	/// </summary>
	public long Score { get; }

	/// <summary>
	/// Gets the highest score achieved.
	/// </summary>
	public long HighScore { get; }

	/// <summary>
	/// Adds a specified amount to the current score.
	/// </summary>
	/// <param name="score">The amount of score to add.</param>
	public void AddScore(long score);
}
