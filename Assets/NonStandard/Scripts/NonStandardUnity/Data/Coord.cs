using UnityEngine;

namespace NonStandard.Data {
    public partial struct Coord {
		public static implicit operator Coord(Vector2Int v) {
			return new Coord(v.x, v.y);
		}
		public static implicit operator Vector2Int(Coord c) {
			return new Vector2Int(c.x, c.y);
		}
		public static implicit operator Coord(Vector2 v) {
			return new Coord((int)v.x, (int)v.y);
		}
		public static implicit operator Vector2(Coord c) {
			return new Vector2(c.x, c.y);
		}

		public bool Equals(Vector2Int v) => row == v.y && col == v.x;
		public static bool operator ==(Coord a, Vector2Int b) => a.Equals(b);
		public static bool operator ==(Vector2Int a, Coord b) => b.Equals(a);
		public static bool operator !=(Coord a, Vector2Int b) => !a.Equals(b);
		public static bool operator !=(Vector2Int a, Coord b) => !b.Equals(a);

		public bool Equals(Vector2 v) => row == v.y && col == v.x;
		public static bool operator ==(Coord a, Vector2 b) => a.Equals(b);
		public static bool operator ==(Vector2 a, Coord b) => b.Equals(a);
		public static bool operator !=(Coord a, Vector2 b) => !a.Equals(b);
		public static bool operator !=(Vector2 a, Coord b) => !b.Equals(a);
	}
}