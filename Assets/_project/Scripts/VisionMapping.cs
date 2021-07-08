using NonStandard;
using NonStandard.Data;
using System;
using System.Collections;

public class VisionMapping {
	public delegate Coord SizeCalculation();
	SizeCalculation sizeCalc;
	Coord size;
	BitArray seen;
	public Action<Coord, bool> onChange;
	public Coord Size { get { return size; } }
	public VisionMapping(SizeCalculation sizeCalc) {
		seen = new BitArray(32);
		this.sizeCalc = sizeCalc;
		Reset();
	}
	public void Reset() {
		size = sizeCalc.Invoke();
		//Show.Log("size" + size);
		seen.Length = size.Area;
		seen.SetAll(false);
		//if (onChange != null) { size.ForEach(c => onChange.Invoke(c, false)); }
	}
	public bool this[Coord c] {
		get { return seen[c.Y * size.X + c.X]; }
		set {
			int i = c.Y * size.X + c.X;
			if (i < 0 || i >= seen.Length) {
				Show.Error(c + " -> " + i + " " + size);
			}
			if(seen[i] != value) {
				seen[i] = value;
				onChange?.Invoke(c, value);
			}
		}
	}
}
