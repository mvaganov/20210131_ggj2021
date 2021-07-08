using UnityEngine;

namespace NonStandard.Extension {
	public static class Vector3Extension {
		public static float Distance(this Vector3 self, Vector3 other) { return Vector3.Distance(self, other); }
		public static float DistanceSquared(this Vector3 self, Vector3 other) { return (self - other).sqrMagnitude; }
		public static float MagnitudeManhattan(this Vector3 self) { return Mathf.Abs(self.x) + Mathf.Abs(self.y) + Mathf.Abs(self.z); }
		public static float DistanceManhattan(this Vector3 self, Vector3 other) { return (self - other).MagnitudeManhattan(); }
	}
}
