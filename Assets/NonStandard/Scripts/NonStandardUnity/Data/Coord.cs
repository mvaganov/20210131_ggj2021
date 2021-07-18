using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(NonStandard.Data.Coord))]
public class CoordDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		//label = EditorGUI.BeginProperty(position, label, property);
		SerializedProperty colProp = property.FindPropertyRelative("col");
		SerializedProperty rowProp = property.FindPropertyRelative("row");
		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
		Rect colRect = position;
		colRect.width /= 2;
		Rect rowRect = colRect;
		rowRect.position += new Vector2(colRect.width, 0);
		const short labelWidth = 14;
		colRect.xMin += labelWidth * (1 - EditorGUI.indentLevel);
		rowRect.xMin += labelWidth * (1 - EditorGUI.indentLevel);
		colProp.intValue = EditorGUI.IntField(colRect, colProp.intValue);
		rowProp.intValue = EditorGUI.IntField(rowRect, rowProp.intValue);
		colRect.position += new Vector2(-labelWidth, 0); EditorGUI.LabelField(colRect, "X");
		rowRect.position += new Vector2(-labelWidth, 0); EditorGUI.LabelField(rowRect, "Y");
	}
}
#endif