using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace NonStandard {
	public static class ReflectionExtension {
		public static Type GetICollectionType(this Type type) {
			foreach (Type i in type.GetInterfaces()) {
				if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>)) {
					return i.GetGenericArguments()[0];
				}
			}
			return null;
		}
		public static Type GetIListType(this Type type) {
			foreach (Type i in type.GetInterfaces()) {
				if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)) {
					return i.GetGenericArguments()[0];
				}
			}
			return null;
		}
		public static KeyValuePair<Type, Type> GetIDictionaryType(this Type type) {
			foreach (Type i in type.GetInterfaces()) {
				if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)) {
					return new KeyValuePair<Type, Type>(i.GetGenericArguments()[0], i.GetGenericArguments()[1]);
				}
			}
			if (type.BaseType != null) { return GetIDictionaryType(type.BaseType); }
			return new KeyValuePair<Type, Type>(null, null);
		}
		public static bool TryCompare(this Type type, object a, object b, out int compareValue) {
			foreach (Type i in type.GetInterfaces()) {
				if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IComparable<>)) {
					MethodInfo compareTo = type.GetMethod("CompareTo", new Type[]{ type });
					compareValue = (int)compareTo.Invoke(a, new object[] { b });
					return true;
				}
			}
			compareValue = 0;
			return false;
		}
		public static Type[] GetSubClasses(this Type type) {
			Type[] allLocalTypes;
			try {
				allLocalTypes = type.Assembly.GetTypes();
			}catch(Exception e) {
				Show.Error("unable to get assembly subclasses of "+type+":"+e);
				return Type.EmptyTypes;
			}
			List<Type> subTypes = new List<Type>();
			for (int i = 0; i < allLocalTypes.Length; ++i) {
				Type t = allLocalTypes[i];
				if (t.IsClass && !t.IsAbstract && t.IsSubclassOf(type)) { subTypes.Add(t); }
			}
			return subTypes.ToArray();
		}

		public static object GetNewInstance(this Type t) { return Activator.CreateInstance(t); }

		public static System.Type[] GetTypesInNamespace(this Assembly assembly, string nameSpace, bool includeComponentTypes = false) {
			if (assembly == null) {
				assembly = System.Reflection.Assembly.GetExecutingAssembly();
			}
			System.Type[] types = assembly.GetTypes().Where(t =>
				System.String.Equals(t.Namespace, nameSpace, System.StringComparison.Ordinal)
				&& (includeComponentTypes || !t.ToString().Contains('+'))).ToArray();
			return types;
		}
		public static List<string> TypeNamesWithoutNamespace(System.Type[] validTypes, string namespaceToClean) {
			List<string> list = new List<string>();
			for (int i = 0; i < validTypes.Length; ++i) {
				string typename = validTypes[i].ToString();
				typename = typename.RemoveFromFront(namespaceToClean + ".");
				list.Add(typename);
			}
			return list;
		}

	}
}