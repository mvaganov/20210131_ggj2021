﻿using NonStandard.Commands;
using NonStandard.Extension;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Data {
	public class ScriptedDictionaryManager : MonoBehaviour {
		public void Awake() {
			Commander.Instance.AddCommands(new Command[] {
				new Command("assertnum", AssertNum, new Argument[]{
					new Argument("-n","variableName","name of the variable",type:typeof(string),order:1,required:true),
					new Argument("-v","variableValue","initial value to assign to the variable if it doesn't exist",type:typeof(object),order:2,required:true)
				}, "Ensures the given variable exists. If it didn't, it does now, with the given value."),
				new Command("++", Increment, new Argument[]{
					new Argument("-n","variableName","name of the variable",type:typeof(string),order:1,required:true),
				},"Increments a variable"),
				new Command("--", Decrement, new Argument[]{
					new Argument("-n","variableName","name of the variable",type:typeof(string),order:1,required:true),
				},"Decrements a variable"),
				new Command("set", SetVariable, new Argument[]{
					new Argument("-n","variableName","name of the variable",type:typeof(string),order:1,required:true),
					new Argument("-v","variableValue","value to assign the variable",type:typeof(object),order:2,required:true)
				}, "Ensures the given variable exists with the given value."),
			});
		}
		public List<ScriptedDictionary> dictionaries = new List<ScriptedDictionary>();
		private ScriptedDictionary mainDictionary;
		public ScriptedDictionary Main { get => mainDictionary; set => mainDictionary = value; }
		public void Register(ScriptedDictionary keeper) { dictionaries.Add(keeper); if (mainDictionary == null) mainDictionary = keeper; }
		public void Increment(string name) { mainDictionary.AddTo(name, 1); }
		public void Decrement(string name) { mainDictionary.AddTo(name, -1); }
		public void Increment(Command.Exec e) { Increment(e.tok.GetStr(1)); }
		public void Decrement(Command.Exec e) { Decrement(e.tok.GetStr(1)); }
		public void SetVariable(Command.Exec e) {
			if (e.tok.tokens.Count <= 1) { e.tok.AddError("set missing variable name"); return; }
			string key = e.tok.GetStr(1, mainDictionary.Dictionary);
			if (e.tok.tokens.Count <= 2) { e.tok.AddError("set missing variable value"); return; }
			object value = e.tok.GetResolvedToken(2, mainDictionary.Dictionary);
			string vStr = value as string;
			float f;
			if (vStr != null && float.TryParse(vStr, out f)) { value = f; }
			mainDictionary.Dictionary.Set(key, value);
		}
		public void AssertNum(Command.Exec exec) {
			string itemName = exec.tok.GetStr(1, mainDictionary.Dictionary);
			//Show.Log("!!!!%^ asserting " + itemName+"     ("+tok.str+")");
			if (itemName != null && mainDictionary.Dictionary.ContainsKey(itemName)) return;
			//Show.Log("!!!!%^ getting value ");
			//Show.Log("!!!!%^ checking "+tok.tokens[2]+" in "+Scope);
			object itemValue = exec.tok.GetResolvedToken(2, mainDictionary.Dictionary);
			//Show.Log("!!!!%^ "+mainDictionary.transform.HierarchyPath()+"["+ itemName + "] is " + itemValue);
			mainDictionary.Dictionary.Set(itemName, itemValue);
		}
		public void SetMainDicionary(ScriptedDictionary scriptedDictionary) { mainDictionary = scriptedDictionary; }
	}
}
