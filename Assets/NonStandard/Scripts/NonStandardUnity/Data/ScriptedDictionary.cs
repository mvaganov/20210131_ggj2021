﻿using NonStandard.Data.Parse;
using NonStandard.Extension;
using NonStandard.Process;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.Data {

	[System.Serializable, StringifyHideType]
	public class HashTable_stringobject : BurlyHashTable<string, object> { }
	public class ScriptedDictionary : MonoBehaviour {
		[SerializeField, HideInInspector] protected HashTable_stringobject dict = new HashTable_stringobject();
		public HashTable_stringobject Dictionary { get { return dict; } }
#if UNITY_EDITOR
		[TextArea(3, 10)]
		public string values;
		[TextArea(1, 10)]
		public string parseResults;

		bool validating = false;
		void OnValidate() {
			Tokenizer tok = new Tokenizer();
			if (Application.isPlaying) {
				CodeConvert.TryFill(values, ref dict, null, tok);
			} else {
				CodeConvert.TryParse(values, out dict, null, tok);
			}
			if (tok.errors.Count > 0) {
				parseResults = tok.errors.JoinToString("\n");
			} else {
				//parseResults = dict.Show(true);
				string newResults = dict.Stringify(pretty:true,showType:true,showNulls:true);
				if (parseResults != newResults) { parseResults = newResults; }
			}
		}
		public void GiveInventoryTo(TMPro.TMP_Text textElement) {
			string outText = dict.Stringify(true);
			Debug.Log(outText);
			textElement.text = outText;
		}
		void ShowChange() {
			if (!validating) {
				validating = true;
				// TODO make this work better... current;y giving strings without quotes
				Proc.Delay(0, () => { values = dict.Show(true); validating = false; });
			}
		}
#else
		void ShowChange(){}
#endif
		public StringEvent dictionaryTostringChangeListener;

		[System.Serializable] public class StringEvent : UnityEvent<string> { }

		void Awake() {
			Global.Get<ScriptedDictionaryManager>().Register(this);
		}
		void Start() {
#if UNITY_EDITOR
			dict.onChange += (k, a, b) => { ShowChange(); };
#endif
			dict.onChange += (k, a, b) => {
				if (dictionaryTostringChangeListener.GetPersistentEventCount() > 0) {
					string s = dict.Stringify(true);
					//Debug.Log(s);
					dictionaryTostringChangeListener.Invoke(s);
				}
			};
			dict.FunctionAssignIgnore();
			//string[] mainStats = new string[] { "str", "con", "dex", "int", "wis", "cha" };
			//int[] scores = { 8, 8, 18, 12, 9, 14 };
			//for(int i = 0; i < mainStats.Length; ++i) {
			//	dict[mainStats[i]] = scores[i];
			//}
			//for (int i = 0; i < mainStats.Length; ++i) {
			//	string s = mainStats[i];
			//	dict.Set("_"+s, ()=>CalcStatModifier(s));
			//}
			//AddTo("cha", 4);
			dict.NotifyStart();
		}
		public float NumValue(string fieldName) {
			object val;
			if (!dict.TryGetValue(fieldName, out val)) return 0;
			CodeConvert.TryConvert(ref val, typeof(float));
			return (float)val;
		}
		public void AddTo(string fieldName, float bonus) {
			//Show.Log(fieldName + " " + bonus + " " + Show.GetStack(6));
			dict[fieldName] = NumValue(fieldName) + bonus;
		}
		private int CalcStatModifier(string s) {
			return (int)Mathf.Floor((NumValue(s) - 10) / 2);
		}

		public string Format(string text) {
			Tokenizer tok = new Tokenizer();
			string resolvedText = CodeConvert.Format(text, dict, tok);
			if (tok.errors.Count > 0) {
				Show.Error(tok.errors.JoinToString(", "));
			}
			return resolvedText;
		}
	}
}
