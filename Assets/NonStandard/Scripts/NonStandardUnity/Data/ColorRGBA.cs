using UnityEngine;

namespace NonStandard.Data {
	[System.Serializable]
	partial struct ColorRGBA {
		public ColorRGBA(UnityEngine.Color c) : this((byte)(c.r*255), (byte)(c.g * 255), (byte)(c.b * 255), (byte)(c.a * 255)) { }
		public ColorRGBA(UnityEngine.Color32 c) : this(c.r,c.g,c.b,c.a) { }

		public static implicit operator ColorRGBA(UnityEngine.Color c) { return new ColorRGBA(c); }
		public static implicit operator ColorRGBA(UnityEngine.Color32 c) { return new ColorRGBA(c); }
		public static implicit operator UnityEngine.Color(ColorRGBA c) { return new Color(c.r/255f,c.g/255f,c.b/255f,c.a/255f); }
		public static implicit operator UnityEngine.Color32(ColorRGBA c) { return new Color32(c.r, c.g, c.b, c.a); }
	}
}