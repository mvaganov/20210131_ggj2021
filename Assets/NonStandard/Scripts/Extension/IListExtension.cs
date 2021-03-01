using System;
using System.Collections.Generic;
using System.Text;

public static class IListExtension {
	public static Int32 BinarySearchIndexOf<T>(this IList<T> list, T value, IComparer<T> comparer = null) {
		if (list == null)
			throw new ArgumentNullException("list");
		if (comparer == null) { comparer = Comparer<T>.Default; }
		Int32 lower = 0, upper = list.Count - 1;
		while (lower <= upper) {
			Int32 middle = lower + (upper - lower) / 2, comparisonResult = comparer.Compare(value, list[middle]);
			if (comparisonResult == 0)
				return middle;
			else if (comparisonResult < 0)
				upper = middle - 1;
			else
				lower = middle + 1;
		}
		return ~lower;
	}
	public static void ForEach<T>(this IList<T> source, Action<T> action) { ForEach(source, action, 0, source.Count); }
	public static void ForEach<T>(this IList<T> source, Action<T> action, int index, int length) {
		for(int i = 0; i < length; ++i) { action.Invoke(source[index + i]); }
	}
	public static void SetEach<T>(this IList<T> source, T value) { for(int i = 0; i < source.Count; ++i) { source[i] = value; } }
	public static void SetEach<T>(this IList<T> source, Func<int,T> action) { SetEach(source, action, 0, source.Count); }
	public static void SetEach<T>(this IList<T> source, Func<int,T> action, int index, int length) {
		for (int i = 0; i < length; ++i) { source[i] = action.Invoke(index + i); }
	}
	public static T[] GetRange<T>(this IList<T> source, int index, int length) {
		T[] list = new T[length];
		for (int i = 0; i < length; ++i) { list[i] = source[index + i]; }
		return list;
	}
	public static int FindIndex<T>(this IList<T> list, Func<T, bool> predicate) {
		for(int i = 0; i < list.Count; ++i) { if (predicate(list[i])) return i; }
		return -1;
	}
	public static T Find<T>(this IList<T> list, Func<T, bool> predicate) {
		for (int i = 0; i < list.Count; ++i) { if (predicate(list[i])) return list[i]; }
		return default(T);
	}
	public static int CountEach<T>(this IList<T> list, Func<T, bool> predicate) {
		int count = 0;
		for (int i = 0; i < list.Count; ++i) { if (predicate(list[i])) ++count; }
		return count;
	}
	public static int Sum<T>(this IList<T> list, Func<T,int> valueFunction) {
		int sum = 0; for(int i = 0; i < list.Count; ++i) { sum += valueFunction(list[i]); } return sum;
	}
	public static float Sum<T>(this IList<T> list, Func<T, float> valueFunction) {
		float sum = 0; for (int i = 0; i < list.Count; ++i) { sum += valueFunction(list[i]); } return sum;
	}
	public static int[] GetNestedIndex<T>(this IList<IList<T>> list, int flatIndex) {
		int[] path = new int[2] { -1, -1 };
		int original = flatIndex;
		if (flatIndex >= 0) {
			for (int i = 0; i < list.Count; ++i) {
				if (flatIndex < list[i].Count) { path[0] = i; path[1] = flatIndex; break; }
				flatIndex -= list[i].Count;
			}
		}
		if(path[0] < 0 || path[1] < 0) {
			throw new Exception("could not convert "+original+" into index from "+list.Count+" lists totalling "+
				list.Sum(l=>l.Count)+" elements");
		}
		return path;
	}
	public static T GetFromNestedIndex<T>(this IList<IList<T>> list, int[] nestedIndex) {
		return list[nestedIndex[0]][nestedIndex[1]];
	}
	public static string JoinToString<T>(this IList<T> source, string separator, Func<T, string> toString = null) {
		string[] strings = new string[source.Count];
		if (toString == null) { toString = o => o.ToString(); }
		for (int i = 0; i < strings.Length; ++i) {
			strings[i] = toString.Invoke(source[i]);
		}
		return string.Join(separator, strings);
	}
	public static void JoinToString<T>(this IList<T> source, StringBuilder sb, string separator, 
		Func<T, string> toString = null) {
		if (toString == null) { toString = o => o.ToString(); }
		bool somethingPrinted = false;
		for (int i = 0; i < source.Count; ++i) {
			if (source[i] != null) {
				if (somethingPrinted) sb.Append(separator);
				sb.Append(toString.Invoke(source[i]));
			}
		}
	}
}
