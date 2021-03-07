using System;

namespace NonStandard.Data {
	public class Random {
		long _seed;
		private static Random _a = null;
		public static Random A { get { return _a != null ? _a : _a = new Random(System.Environment.TickCount); } }
		public Random(long seed) { _seed = seed; }
		public int Next() { return Math.Abs((int)(_seed = _seed * 6364136223846793005 + 1442695040888963407)); }
		public virtual int Next(int maxValue) { return (int)(Next() % maxValue); }
		public virtual int Next(int minValue, int maxValue) { return Next(maxValue - minValue) + minValue; }
		public virtual float NextFloat(float min, float max) { return NextFloat(max - min) + min; }
		public virtual int Range(int minValue, int maxValue) { return Next(minValue, maxValue); }
		public virtual void NextBytes(byte[] buffer) {
			for(int i = 0; i < buffer.Length; ++i) {
				buffer[i] = (byte)(Next() & 255);
			}
		}
		public virtual float NextFloat() { return (float)Next() / int.MaxValue; }
		public virtual float NextFloat(float max) { return max*Next() / int.MaxValue; }
		public virtual double NextDouble() { return (double)Next() / int.MaxValue; }
		protected virtual double Sample() { return NextDouble(); }
		public long Seed { get { return _seed; } set { _seed = value; } }
		public float Value { get { return (float)NextFloat(); } }
	}
}
