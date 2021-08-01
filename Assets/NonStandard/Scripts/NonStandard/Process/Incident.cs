using System;
using System.Collections.Generic;

namespace NonStandard.Process {
	[System.Serializable]
	public class Incident {
		/// <summary>
		/// when was this happened (or will happen)
		/// </summary>
		public readonly long Timestamp;
		/// <summary>
		/// what caused this to happen (maybe. or other meta data). it could be the subject of an event
		/// </summary>
		public object Source;
		/// <summary>
		/// additional meta data about what is happening. it could be the verb (function) of an event
		/// </summary>
		public object Detail;
		/// <summary>
		/// book-keeping identifier, used for process introspection and analysis
		/// </summary>
		public string Identifier;

		public override string ToString() { return Identifier + "@" + Timestamp; }

		public Incident(long timestamp, string identifier, object source, object detail) {
			Timestamp = timestamp;
			Identifier = identifier;
			Source = source;
			Detail = detail;
		}
		public Incident(string identifier, object source = null, object detail = null) {
			Timestamp = Proc.Time;
			Identifier = identifier;
			Source = source;
			Detail = detail;
		}
		public Incident(long newTimestamp, Incident toCopy) {
			Timestamp = newTimestamp;
			Identifier = toCopy.Identifier;
			Source = toCopy.Source;
			Detail = toCopy.Detail;
		}

		public class TimeComparer : IComparer<Incident> {
			public int Compare(Incident a, Incident b) {
				int comp = a.Timestamp.CompareTo(b.Timestamp);
				if (comp != 0) { return comp; }
				if (a.Identifier != null && b.Identifier != null) {
					comp = String.Compare(a.Identifier, b.Identifier, StringComparison.Ordinal);
				} else if (a.Identifier == null && b.Identifier != null) {
					comp = 1;
				} else if (a.Identifier != null && b.Identifier == null) {
					comp = -1;
				}
				return comp;
			}
			public static readonly TimeComparer Instance = new TimeComparer();
		}
	}
}