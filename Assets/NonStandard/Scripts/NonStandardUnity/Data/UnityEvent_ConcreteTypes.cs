using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace NonStandard.Data {
	[System.Serializable] public class UnityEvent_GameObject : UnityEvent<GameObject> { }
	[System.Serializable] public class UnityEvent_string : UnityEvent<string> { }
	[System.Serializable] public class UnityEvent_Vector3 : UnityEvent<Vector3> { }
	[System.Serializable] public class UnityEvent_Vector2 : UnityEvent<Vector2> { }
	[System.Serializable] public class UnityEvent_List_object : UnityEvent<List<object>> { }
}