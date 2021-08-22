﻿using System;

namespace NonStandard.Ui {
	[Flags] public enum Direction2D {
		None = 0, Bottom = 1, Left = 2, Top = 4, Right = 8,
		TopLeft = Top | Left, TopRight = Top | Right,
		BottomLeft = Bottom | Left, BottomRight = Bottom | Right,
		Horizontal = Left | Right, Vertical = Bottom | Top,
		HorizontalBottom = ~Top & All, HorizontalTop = ~Bottom & All,
		VerticalLeft = ~Right & All, VerticalRight = ~Left & All,
		All = Bottom | Top | Left | Right
	};

	[Flags]	public enum Direction3D {
		None = 0, Down = 1, Left = 2, Back = 4, Up = 8, Right = 16, Forward = 32,
		DownBack = Down | Back, DownForward = Down | Forward,
		LeftBack = Left | Back, LeftForward = Left | Forward,
		UpBack = Up | Back, UpForward = Up | Forward,
		RightBack = Right | Back, RightForward = Right | Forward,
		UpLeft = Up | Left, UpRight = Up | Right,
		UpLeftBack = Up | Left | Back, UpRightBack = Up | Right | Back,
		UpLeftForward = Up | Left | Forward, UpRightForward = Up | Right | Forward,
		DownLeft = Down | Left, DownRight = Down | Right,
		DownLeftBack = Down | Left | Back, DownRightBack = Down | Right | Back,
		DownLeftForward = Down | Left | Forward, DownRightForward = Down | Right | Forward,
		Horizontal = Left | Right, Vertical = Down | Up, Profile = Back | Forward,
		HorizontalDown = ~Up & All, HorizontalUp = ~Down & All,
		VerticalLeft = ~Right & All, VerticalRight = ~Left & All,
		ProfileForward = ~Forward & All, ProfileBack = ~Back & All,
		All = Down | Up | Left | Right | Back | Forward
	};

	public static class DirectionExtension {
		public static Direction2D Opposite(this Direction2D orig) { return (Direction2D)(((int)orig >> 2) | ((int)orig << 2)) & Direction2D.All; }
		public static Direction3D Opposite(this Direction3D orig) { return (Direction3D)(
				((int)orig >> 3) | ((int)orig << 3)) & Direction3D.All; }
		public static bool HasFlag(this Direction2D cs, Direction2D flag) { return ((int)cs & (int)flag) != 0; }
		public static bool HasFlag(this Direction3D cs, Direction3D flag) { return ((int)cs & (int)flag) != 0; }
	}
}
