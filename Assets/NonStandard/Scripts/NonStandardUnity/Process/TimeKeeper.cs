namespace NonStandard.Process {
	/// <summary>
	/// if a <see cref="TimeKeeper"/> is used, Unity creates a <see cref="GameClock"/> to connect it to <see cref="UnityEngine.MonoBehaviour"/>'s Update
	/// </summary>
	public partial class TimeKeeper { static TimeKeeper() { GameClock.Instance(); } }
}