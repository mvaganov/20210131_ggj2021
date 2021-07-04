using System;
using System.Collections.Generic;

namespace NonStandard.Procedure {
	public class TimeKeeper {
		public delegate long GetTimeFunction();
		public GetTimeFunction HowToGetTime = FileSystemTimeNow;
		public static long FileSystemTimeNow() { return System.Environment.TickCount; }
		public static long UtcMilliseconds() { return System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond; }

		List<Incident> schedule = new List<Incident>();
		private List<Incident> _doneRightNow = new List<Incident>();
		public TimeKeeper() { }
		public TimeKeeper(GetTimeFunction howToGetTime) { HowToGetTime = howToGetTime; }
		public void Update() {
			long now = HowToGetTime();
			_doneRightNow.Clear();
			lock (schedule) {
				for (int i = 0; i < schedule.Count; ++i) {
					if (schedule[i].Timestamp > now) { break; }
					_doneRightNow.Add(schedule[i]);
				}
				schedule.RemoveRange(0, _doneRightNow.Count);
			}
			for (int i = 0; i < _doneRightNow.Count; ++i) {
				Incident incident = _doneRightNow[i];
				switch (incident.Source) {
				case int code: Proc.NotifyIncident(code, incident); break;
				case string id: Proc.NotifyIncident(id, incident); break;
				case Action act: act.Invoke(); break;
				case Proc.edure r: r.Invoke(incident); break;
				}
			}
		}
		public class IncidentComparer : IComparer<Incident> {
			public int Compare(Incident a, Incident b) {
				int comp = a.Timestamp.CompareTo(b.Timestamp);
				if (comp != 0) { return comp; }
				if (a.Identifier != null && b.Identifier != null) {
					comp = a.Identifier.CompareTo(b.Identifier);
				} else if (a.Identifier == null && b.Identifier != null) {
					comp = 1;
				} else if (a.Identifier != null && b.Identifier == null) {
					comp = -1;
				}
				return comp;
			}
		}
		public static readonly IncidentComparer Comparer = new IncidentComparer();
		public void AddToSchedule(Incident incident) {
			lock (schedule) {
				int index = schedule.BinarySearch(incident, Comparer);
				if (index < 0) {
					index = ~index;
				}
				schedule.Insert(index, incident);
			}
		}
	}
}