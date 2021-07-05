using System;
using System.Collections.Generic;

namespace NonStandard.Procedure {
	[System.Serializable]
	public class TimeKeeper {
		public const string ScheduledId = "chrono";
		public delegate long GetTimeFunction();
		public GetTimeFunction GetTime = FileSystemTimeNow;
		public static long FileSystemTimeNow() { return System.Environment.TickCount; }
		public static long UtcMilliseconds() { return System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond; }

		public long now;
		/// <summary>
		/// how many milliseconds doing scheduled tasks is too long. we don't want to be a blocking bottleneck here.
		/// </summary>
		public long MaximumTimeProcessing = 10;
		public List<Incident> schedule = new List<Incident>();
		private List<Incident> _doRightNow = new List<Incident>();
		public TimeKeeper() { }
		public TimeKeeper(GetTimeFunction howToGetTime) { GetTime = howToGetTime; }
		public void Update() {
			now = GetTime();
			lock (schedule) {
				int itemsToDo = 0;
				while(itemsToDo < schedule.Count && schedule[itemsToDo].Timestamp < now) {
					_doRightNow.Add(schedule[itemsToDo]); 
					++itemsToDo;
				}
				schedule.RemoveRange(0, itemsToDo);
			}
			long tooLong = now + MaximumTimeProcessing;
			int doneItems = 0;
			Exception error = null;
			while(doneItems < _doRightNow.Count) {
				Incident incident = _doRightNow[doneItems];
				++doneItems;
				error = Do(incident);
				if (error != null || GetTime() >= tooLong) {
					break;
				}
			}
			if (doneItems == _doRightNow.Count) {
				_doRightNow.Clear();
			} else {
				_doRightNow.RemoveRange(0, doneItems);
			}
			if(error != null) { throw error; }
		}
		public Exception Do(Incident incident) {
			try {
				switch (incident.Detail) {
				case int code: Proc.NotifyIncident(code, incident); break;
				case string id: Proc.NotifyIncident(id, incident); break;
				case Action act: act.Invoke(); break;
				case Proc.edure r: r.Invoke(incident); break;
				case Strategy s: s.Invoke(incident); break;
				default: throw new Exception("Don't know how to execute '" + incident.Detail + "'");
				}
			}catch(Exception e) {
				return e;
			}
			return null;
		}
		public Incident AddToSchedule(long when, object whatIsBeingScheduled) {
			return AddToSchedule(new Incident(when, ScheduledId, null, whatIsBeingScheduled));
		}
		public Incident AddToSchedule(long when, Action whatIsBeingScheduled) {
			return AddToSchedule(new Incident(when, ScheduledId, null, whatIsBeingScheduled));
		}
		/// <summary>
		/// int and string values translate to Incident triggers. <see cref="Action"/>, <see cref="Proc.edure"/>, and <see cref="Strategy"/> values are Invoked
		/// </summary>
		public Incident Delay(long delay, object whatIsBeingScheduled) {
			return AddToSchedule(GetTime() + delay, whatIsBeingScheduled);
		}
		public Incident Delay(long delay, Action whatIsBeingScheduled) {
			return AddToSchedule(GetTime() + delay, whatIsBeingScheduled);
		}
		public Incident RemoveScheduled(Action actionBeingScheduled) {
			for (int i = 0; i < schedule.Count; ++i) {
				if (schedule[i].Detail is Action a && (a == actionBeingScheduled || a.Method == actionBeingScheduled.Method)) {
					Incident incident = schedule[i];
					schedule.RemoveAt(i);
					return incident;
				}
			}
			return null;
		}
		public Incident RemoveScheduled(object somethingBeingScheduled) {
			for(int i = 0; i < schedule.Count; ++i) {
				if(schedule[i].Detail == somethingBeingScheduled) {
					Incident incident = schedule[i];
					schedule.RemoveAt(i);
					return incident;
				}
			}
			return null;
		}
		public Incident AddToSchedule(Incident incident) {
			lock (schedule) {
				int index = schedule.BinarySearch(incident, Incident.TimeComparer.Instance);
				if (index < 0) {
					index = ~index;
				}
				schedule.Insert(index, incident);
			}
			return incident;
		}
		public delegate void LerpMethod(float progress);
		public void Lerp(LerpMethod action, long durationMs = 1000, float calculations = 10, float start = 0, float end = 1) {
			long started = GetTime();
			float iterations = 0;
			void DoIt() {
				float delta = end - start;
				float p = (delta * iterations) / calculations + start;
				action.Invoke(p);
				long delay = (long)(durationMs / calculations);
				if (iterations < calculations) {
					long nextTime = (long)(durationMs * (iterations + 1) / calculations) + started;
					Delay(nextTime, DoIt);
				}
				++iterations;
			}
			DoIt();
		}
	}
}