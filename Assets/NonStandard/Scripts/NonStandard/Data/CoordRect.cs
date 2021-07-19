using System;

namespace NonStandard.Data {
	public interface IRect : IPosition { CoordRect GetRect(); }

	// AABB : Axis Aligned Bounding Box
	[System.Serializable]
	public struct CoordRect : IPosition, IRect {
		public Coord min, max;

		public static readonly CoordRect Invalid = new CoordRect(Coord.One, Coord.Zero);
		public CoordRect(Coord min, Coord max) {
			this.min = min;
			this.max = max;
		}

		public CoordRect(int x, int y, int width, int height) {
			min = new Coord(x, y);
			max = new Coord(x + width, y + height);
		}

		public Coord Min { get => min; set => min = value; }
		public Coord Max { get => max; set => max = value; }
		public int X { get => min.col; set => min.col = (short)value; }
		public int Y { get => min.row; set => min.row = (short)value; }
		public int Width { get => max.col - min.col; set => max.col = (short)(min.col + value); }
		public int Height { get => max.row - min.row; set => max.row = (short)(min.row + value); }

		public int Top { get => min.row; set => min.row = (short)value; }
		public int Left { get => min.col; set => min.col = (short)value; }
		public int Bottom { get => max.row; set => max.row = (short)value; }
		public int Right { get => max.col; set => max.col = (short)value; }
		public Coord Size { get => max - min; set => max = min + value; }
		public Coord Position { get => min; set { Coord size = Size; min = value; max = min + size; } }
		public short PositionX { get => min.col; set { int w = Width; min.x = value; max.x = min.x + (short)w; } }
		public short PositionY { get => min.row; set { int h = Height; min.y = value; max.y = min.y + (short)h; } }

		public Coord GetPosition() => Position;
		public Coord GetSize() => Size;
		public CoordRect GetRect() => this;

		public override string ToString() { return "[" + Left + "," + Top + "," + Right + "," + Bottom + "]"; }

		public CoordRect Intersect(CoordRect r) {
			GetRectIntersect(min, max, r.min, r.max, out Coord iMin, out Coord iMax);
			return new CoordRect(iMin, iMax);
		}

		public bool Contains(Coord coord) {
			return coord.col >= min.col && coord.col < max.col && coord.row >= min.row && coord.row < max.row;
		}

		public bool MoveToContain(Coord coord) {
			if (Contains(coord)) return false;
			Coord boundry = coord;
			if (coord.X < Left) { boundry.x = Left; }
			if (coord.Y < Top) { boundry.y = Top; }
			if (coord.X >= Right) { boundry.x = Right-1; }
			if (coord.Y >= Bottom) { boundry.y = Bottom-1; }
			Position += coord - boundry;
			return true;
		}

		public bool TryGetIntersect(CoordRect r, out CoordRect intersection) => TryGetIntersect(this, r, out intersection);

		/// <summary>
		/// warning: delegates may cause memory leaks! be careful about allocated memory scope.
		/// </summary>
		/// <param name="locationAction"></param>
		public void ForEach(Action<Coord> locationAction) { Coord.ForEach(min, max, locationAction); }

		/// <summary>
		/// stops iterating as soon as action returns true.
		/// </summary>
		/// <param name="action">runs till the first return true</param>
		/// <returns>true if action returned true even once</returns>
		public bool ForEach(Func<Coord, bool> action) { return Coord.ForEach(min, max, action); }

		public bool IsIntersect(CoordRect other) { return IsRectIntersect(min, max, other.min, other.max); }

		public static CoordRect Sum(CoordRect a, CoordRect b) {
			b.Expand(a);
			return b;
		}

		public static bool IsRectIntersect(Coord aMin, Coord aMax, Coord bMin, Coord bMax) {
			return aMin.col < bMax.col && bMin.col < aMax.col && aMin.row < bMax.row && bMin.row < aMax.row;
		}

		public static bool IsSizeRectIntersect(Coord aMin, Coord aSize, Coord bMin, Coord bSize) {
			return IsRectIntersect(aMin, aMin + aSize, bMin, bMin + bSize);
		}

		public static bool GetRectIntersect(Coord aMin, Coord aMax, Coord bMin, Coord bMax, out Coord oMin, out Coord oMax) {
			oMin = new Coord { col = Math.Max(aMin.col, bMin.col), row = Math.Max(aMin.row, bMin.row) };
			oMax = new Coord { col = Math.Min(aMax.col, bMax.col), row = Math.Min(aMax.row, bMax.row) };
			return oMin.col < oMax.col && oMin.row < oMax.row;
		}

		public static bool TryGetIntersect(CoordRect a, CoordRect b, out CoordRect o) {
			return GetRectIntersect(a.min, a.max, b.min, b.max, out o.min, out o.max);
		}

		public static bool GetSizeRectIntersect(Coord aMin, Coord aSize, Coord bMin, Coord bSize, out Coord oMin, out Coord oSize) {
			bool result = GetRectIntersect(aMin, aMin + aSize, bMin, bMin + bSize, out oMin, out oSize);
			oSize -= oMin;
			return result;
		}

		/// <param name="nMin">needle min corner</param>
		/// <param name="nMax">needle max corner</param>
		/// <param name="hMin">haystack min corner</param>
		/// <param name="hMax">haystack max corner</param>
		/// <returns></returns>
		public static bool IsRectContained(Coord nMin, Coord nMax, Coord hMin, Coord hMax) {
			return nMin.col >= hMin.col && hMax.col >= nMax.col && nMin.row >= hMin.row && hMax.row >= nMax.row;
		}

		public static bool IsSizeRectContained(Coord nMin, Coord nSize, Coord hMin, Coord hSize) {
			return IsRectContained(nMin, nMin + nSize, hMin, hMin + hSize);
		}

		public static bool ExpandRectangle(Coord pMin, Coord pMax, ref Coord min, ref Coord max) {
			bool change = false;
			if (pMin.col < min.col) { min.col = pMin.col; change = true; }
			if (pMin.row < min.row) { min.row = pMin.row; change = true; }
			if (pMax.col > max.col) { max.col = pMax.col; change = true; }
			if (pMax.row > max.row) { max.row = pMax.row; change = true; }
			return change;
		}

		public bool Expand(CoordRect p) => ExpandRectangle(p.min, p.max, ref min, ref max);

		public bool Expand(Coord p) => ExpandRectangle(p, p, ref min, ref max);
	}
}