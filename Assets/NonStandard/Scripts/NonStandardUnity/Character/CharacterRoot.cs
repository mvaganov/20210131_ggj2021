using NonStandard.Data;
using UnityEngine;

namespace NonStandard.Character {
	// TODO reference this instead of CharacterMove everywhere that a character refrence is needed, especially the DataSheet
	public class CharacterRoot : MonoBehaviour
	{
		public CharacterMove move;
		public ScriptedDictionary dict;

		public void Start() {
			if (move == null) { move = GetComponentInChildren<CharacterMove>(); }
			if (dict == null) { dict = GetComponentInChildren<ScriptedDictionary>(); }
		}
	}
}