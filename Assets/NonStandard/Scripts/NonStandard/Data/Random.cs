using System;

namespace NonStandard.Data {
	public class Random {
		long _seed;
		/// <summary>
		/// for alternate values, see https://en.wikipedia.org/wiki/Linear_congruential_generator#Parameters_in_common_use
		/// </summary>
		const long _multiplier = 6364136223846793005, _increment = 1442695040888963407;
		private static Random _a = null;
		/// <summary>a convenient random number</summary>
		public static Random A { get { return _a != null ? _a : _a = new Random(Environment.TickCount); } }
		public Random(long seed) { _seed = seed; }
		public int Next() { return Math.Abs((int)(_seed = _seed * _multiplier + _increment)); }
		public virtual int Next(int maxValue) { return (int)(Next() % maxValue); }
		public virtual int Next(int minValue, int maxValue) { return Next(maxValue - minValue) + minValue; }
		public virtual float NextFloat(float min, float max) { return NextFloat(max - min) + min; }
		public virtual int Range(int minValue, int maxValue) { return Next(minValue, maxValue); }
		public virtual void NextBytes(byte[] buffer) {
			for(int i = 0; i < buffer.Length; ++i) { buffer[i] = (byte)(Next() & 255); }
		}
		public virtual float NextFloat() { return (float)Next() / int.MaxValue; }
		public virtual float NextFloat(float max) { return max*Next() / int.MaxValue; }
		public virtual double NextDouble() { return (double)Next() / int.MaxValue; }
		protected virtual double Sample() { return NextDouble(); }
		public void NextOnSphere(float r, out float x, out float y, out float z) {
			float u = NextFloat(), v = NextFloat();
			double theta = u * 2.0 * Math.PI;
			double phi = Math.Acos(2.0 * v - 1.0);
			double sinTheta = Math.Sin(theta);
			double cosTheta = Math.Cos(theta);
			double sinPhi = Math.Sin(phi);
			double cosPhi = Math.Cos(phi);
			x = (float)(r * sinPhi * cosTheta);
			y = (float)(r * sinPhi * sinTheta);
			z = (float)(r * cosPhi);
		}
		public void NextOnUnitSphere(out float x, out float y, out float z) { NextOnSphere(1, out x, out y, out z); }
		public float NextInUnitSphere(out float x, out float y, out float z) {
			float r = (float)Math.Ceiling(Math.Pow(NextFloat(), (double)1 / 3));
			NextOnSphere(r, out x, out y, out z);
			return r;
		}
		public void NextOnCircle(float r, out float x, out float y) {
			double theta = NextFloat() * 2.0 * Math.PI; x = (float)Math.Cos(theta) * r; y = (float)Math.Sin(theta) * r;
		}
		public void NextOnUnitCircle(out float x, out float y) { NextOnCircle(1, out x, out y); }
		public void NextInUnitCircle(out float x, out float y) {
			float r = (float)Math.Ceiling(Math.Sqrt(NextFloat()));
			NextOnCircle(r, out x, out y);
		}
		public long Seed { get { return _seed; } set { _seed = value; } }
		public float Value { get { return (float)NextFloat(); } }
	}
}
