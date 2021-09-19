﻿using NonStandard.Data;
using UnityEngine;

namespace NonStandard.Character {
	public class CharacterRoot : MonoBehaviour
	{
		public CharacterMove move;
		public Object data;
		public UnityEvent_GameObject activateFunction;

		public void Init() {
			if (move == null) { move = GetComponentInChildren<CharacterMove>(); }
			if (data == null) { data = GetComponentInChildren<ScriptedDictionary>(); }
		}
		public void Awake() { Init(); }
		public void Start() { Init(); }
		public void DoActivateTrigger() { activateFunction.Invoke(gameObject); }
	}
}