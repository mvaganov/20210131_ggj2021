using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard {
	public partial class Global {
		public static Dictionary<Type, object> directory = new Dictionary<Type, object>();
		public static T Get<T>(T defaultValue = default(T)) {
			if (directory.TryGetValue(typeof(T), out object obj)) { return (T)obj; }
			return defaultValue;
		}
	}
}
