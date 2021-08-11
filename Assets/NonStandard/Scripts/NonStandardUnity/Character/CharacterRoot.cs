using NonStandard.Data;
using UnityEngine;

namespace NonStandard.Character {
	// TODO reference this instead of CharacterMove everywhere that a character refrence is needed, especially the DataSheet
	public class CharacterRoot : MonoBehaviour
	{
		public CharacterMove move;
		public ScriptedDictionary data;
		public void Init() {
			if (move == null) { move = GetComponentInChildren<CharacterMove>(); }
			if (data == null) { data = GetComponentInChildren<ScriptedDictionary>(); }
		}
		public void Awake() { Init(); }
		public void Start() { Init(); }
	}
}