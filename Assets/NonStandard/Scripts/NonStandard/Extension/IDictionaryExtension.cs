using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonStandard.Extension {
	public static class IDictionaryExtension {
		public static List<KeyValuePair<TKey,TValue>> GetPairs<TKey, TValue>(this IDictionary<TKey,TValue> dict) {
			List<KeyValuePair<TKey, TValue>> list = new List<KeyValuePair<TKey, TValue>>();
			foreach(KeyValuePair<TKey, TValue> kvp in dict){
				list.Add(kvp);
			}
			return list;
		}
	}
}
