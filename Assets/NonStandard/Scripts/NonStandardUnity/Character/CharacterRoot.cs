using NonStandard.Data;
using UnityEngine;

namespace NonStandard.Character {
	public class CharacterRoot : MonoBehaviour
	{
		public CharacterMove move;
		public Object data;
		public void Init() {
			if (move == null) { move = GetComponentInChildren<CharacterMove>(); }
			if (data == null) { data = GetComponentInChildren<ScriptedDictionary>(); }
		}
		public void Awake() { Init(); }
		public void Start() { Init(); }
	}
}