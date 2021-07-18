using NonStandard.Data.Parse;
using NonStandard.Commands;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Data {
	public class DictionaryKeeperManager : MonoBehaviour {
		public void Awake() {
			Commander.Instance.AddCommands(new Dictionary<string, Command.Handler>() {
				["assertnum"] = AssertNum,
				["++"] = Increment,
				["--"] = Decrement,
				["set"] = SetVariable,
			});
		}
		public List<DictionaryKeeper> keepers = new List<DictionaryKeeper>();
		public DictionaryKeeper Keeper;
		public void Register(DictionaryKeeper keeper) { keepers.Add(keeper); if (Keeper == null) Keeper = keeper; }
		public void Increment(string name) { Keeper.AddTo(name, 1); }
		public void Decrement(string name) { Keeper.AddTo(name, -1); }
		public void Increment(Command.Exec e) { Increment(e.tok.GetStr(1)); }
		public void Decrement(Command.Exec e) { Decrement(e.tok.GetStr(1)); }
		public void SetVariable(Command.Exec e) {
			if (e.tok.tokens.Count <= 1) { e.tok.AddError("set missing variable name"); return; }
			string key = e.tok.GetStr(1, Keeper.Dictionary);
			if (e.tok.tokens.Count <= 2) { e.tok.AddError("set missing variable value"); return; }
			object value = e.tok.GetResolvedToken(2, Keeper.Dictionary);
			string vStr = value as string;
			float f;
			if (vStr != null && float.TryParse(vStr, out f)) { value = f; }
			Keeper.Dictionary.Set(key, value);
		}
		public void AssertNum(Command.Exec exec) {
			string itemName = exec.tok.GetStr(1, Keeper.Dictionary);
			//Show.Log("!!!!%^ asserting " + itemName+"     ("+tok.str+")");
			if (itemName != null && Keeper.Dictionary.ContainsKey(itemName)) return;
			//Show.Log("!!!!%^ getting value ");
			//Show.Log("!!!!%^ checking "+tok.tokens[2]+" in "+Scope);
			object itemValue = exec.tok.GetResolvedToken(2, Keeper.Dictionary);
			//Show.Log("!!!!%^ value is " + itemValue);
			Keeper.Dictionary.Set(itemName, itemValue);
		}
	}
}
