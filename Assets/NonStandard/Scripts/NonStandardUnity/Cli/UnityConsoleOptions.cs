using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityConsoleOptions : MonoBehaviour
{
	[System.Serializable] public class Options {
		public bool StandardInput = true;
		public bool ExecuteCommands = true;
		public bool ToggleUserInterface = true;
		public bool TraverseGameObjectsInScene = true;
	}
}
