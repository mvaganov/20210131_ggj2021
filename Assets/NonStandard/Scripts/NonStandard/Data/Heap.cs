using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NonStandard.Data {
	public abstract class Heap<T> : IEnumerable<T> {
		private T[] _heap = new T[0];
		private int _count = 0;
		public int Count { get { return _count; } }
		public int Capacity { get { return _heap.Length; } }

		protected Comparer<T> Comparer { get; private set; }
		protected abstract bool Dominates(T x, T y);

		protected Heap() : this(Comparer<T>.Default) { }
		protected Heap(Comparer<T> comparer) : this(Enumerable.Empty<T>(), comparer) { }
		protected Heap(IEnumerable<T> collection) : this(collection, Comparer<T>.Default) { }
		protected Heap(Heap<T> toCopy) {
			_heap = toCopy._heap.Clone() as T[];
			_count = toCopy._count;
		}
		protected Heap(IEnumerable<T> collection, Comparer<T> comparer) {
			if (collection == null) throw new ArgumentNullException("collection");
			if (comparer == null) throw new ArgumentNullException("comparer");
			Comparer = comparer;
			foreach (var item in collection) {
				if (Count == Capacity) { Grow(); }
				_heap[_count++] = item;
			}
			for (int i = Parent(_count - 1); i >= 0; i--) { BubbleDown(i); }
		}

		public void Add(T item) {
			if (Count == Capacity) { Grow(); }
			_heap[_count] = item;
			BubbleUp(_count++);
		}
		public T Peek() {
			if (Count == 0) throw new InvalidOperationException("Heap is empty");
			return _heap[0];
		}
		public T Pop() {
			if (Count == 0) throw new InvalidOperationException("Heap is empty");
			T ret = _heap[0];
			_count--;
			_heap[0] = default(T);
			Swap(_count, 0);
			BubbleDown(0);
			return ret;
		}

		private void BubbleUp(int i) {
			if (i == 0 || Dominates(_heap[Parent(i)], _heap[i])) { return; }
			Swap(i, Parent(i));
			BubbleUp(Parent(i));
		}
		private void BubbleDown(int i) {
			int dominatingNode = Dominating(i);
			if (dominatingNode == i) return;
			Swap(i, dominatingNode);
			BubbleDown(dominatingNode);
		}
		private int Dominating(int i) {
			int dominatingNode = i;
			dominatingNode = GetDominating(YoungChild(i), dominatingNode);
			dominatingNode = GetDominating(OldChild(i), dominatingNode);
			return dominatingNode;
		}
		private int GetDominating(int newNode, int dominatingNode) {
			if (newNode < _count && !Dominates(_heap[dominatingNode], _heap[newNode])) { return newNode; }
			return dominatingNode;
		}
		private void Swap(int i, int j) { T tmp = _heap[i]; _heap[i] = _heap[j]; _heap[j] = tmp; }
		private static int Parent(int i) { return (i + 1) / 2 - 1; }
		private static int YoungChild(int i) { return (i + 1) * 2 - 1; }
		private static int OldChild(int i) { return YoungChild(i) + 1; }
		private void Grow() {
			int newCapacity = _heap.Length * 2 + 1;
			var newHeap = new T[newCapacity];
			Array.Copy(_heap, newHeap, _heap.Length);
			_heap = newHeap;
		}

		/// <summary>
		/// this is not a destructive call, the heap is maintained
		/// </summary>
		/// <returns>array with in place heap order</returns>
		public T[] ToArray() {
			T[] result = new T[Count];
			Array.Copy(_heap, result, Count);
			return result;
		}
		/// <summary>
		/// this is a destructive call, clearing out the heap.
		/// </summary>
		/// <returns>array of elements in order, smallest to largest</returns>
		public T[] ToSortedArray() {
			T[] result = new T[Count]; int i = Count - 1;
			while (Count > 0) { result[i--] = Pop(); }
			return result;
		}
		/// <summary>
		/// this is a destructive call, clearing out the heap.
		/// </summary>
		/// <returns>array of elements in order, smallest to largest</returns>
		public V[] ToSortedArray<V>(Func<T, V> converter) {
			V[] result = new V[Count]; int i = Count - 1;
			while (Count > 0) { result[i--] = converter(Pop()); }
			return result;
		}
		/// <summary>
		/// this is not a destructive call, the heap is maintained
		/// </summary>
		/// <returns>array with in place heap order</returns>
		public V[] ToArray<V>(Func<T, V> converter) {
			V[] result = new V[Count];
			for (int i = 0; i < Count; ++i) { result[i] = converter(_heap[i]); }
			return result;
		}

		public IEnumerator<T> GetEnumerator() { return _heap.Take(Count).GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	public class MaxHeap<T> : Heap<T> {
		public MaxHeap() : this(Comparer<T>.Default) { }
		public MaxHeap(Heap<T> toCopy) : base(toCopy) { }
		public MaxHeap(Comparer<T> comparer) : base(comparer) { }
		public MaxHeap(IEnumerable<T> collection, Comparer<T> comparer) : base(collection, comparer) { }
		public MaxHeap(IEnumerable<T> collection) : base(collection) { }
		protected override bool Dominates(T x, T y) { return Comparer.Compare(x, y) >= 0; }
	}

	public class MinHeap<T> : Heap<T> {
		public MinHeap() : this(Comparer<T>.Default) { }
		public MinHeap(Heap<T> toCopy) : base(toCopy) { }
		public MinHeap(Comparer<T> comparer) : base(comparer) { }
		public MinHeap(IEnumerable<T> collection) : base(collection) { }
		public MinHeap(IEnumerable<T> collection, Comparer<T> comparer) : base(collection, comparer) { }
		protected override bool Dominates(T x, T y) { return Comparer.Compare(x, y) <= 0; }
	}

	public static class HeapSearchExtension {
		struct DistanceEntry<T> {
			public float distance; public T value;
			public DistanceEntry(T v, float d) { value = v; distance = d; }
		}
		/// <typeparam name="T"></typeparam>
		/// <param name="list">unsorted list of elements</param>
		/// <param name="count">how many closest elements to get</param>
		/// <param name="DistanceFunction">heuristic to determine how close an element is</param>
		/// <param name="sorted">if true, minor additional process cost (another O(c log c) function) un-heaping, to sort</param>
		/// <returns>a list of count elements if list length is greater count. these are the closest to 0 according to the given distance heuristic. if sorted is false, they are not in gauranteed order.</returns>
		public static T[] GetClosest<T>(this IEnumerable<T> list, int count, Func<T, float> DistanceFunction, bool sorted = true) {
			List<T> result = new List<T>();
			Comparer<DistanceEntry<T>> comparer = Comparer<DistanceEntry<T>>.Create((a, b) => a.distance.CompareTo(b.distance));
			MaxHeap<DistanceEntry<T>> maxheap = new MaxHeap<DistanceEntry<T>>(comparer);
			foreach (T e in list) {
				DistanceEntry<T> de = new DistanceEntry<T>(e, DistanceFunction(e));
				maxheap.Add(de);
				if (maxheap.Count > count) { maxheap.Pop(); }
			}
			return sorted ? maxheap.ToSortedArray(sortedElement => sortedElement.value)
				: maxheap.ToArray(partiallySortedElement => partiallySortedElement.value);
		}
	}
}