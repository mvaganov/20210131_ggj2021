﻿using NonStandard.Data.Parse;
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
		public void Increment(Tokenizer tok, object src, Show.PrintFunc print) { Increment(tok.GetStr(1)); }
		public void Decrement(Tokenizer tok, object src, Show.PrintFunc print) { Decrement(tok.GetStr(1)); }
		public void SetVariable(Tokenizer tok, object src, Show.PrintFunc print) {
			if (tok.tokens.Count <= 1) { tok.AddError("set missing variable name"); return; }
			string key = tok.GetStr(1, Keeper.Dictionary);
			if (tok.tokens.Count <= 2) { tok.AddError("set missing variable value"); return; }
			object value = tok.GetResolvedToken(2, Keeper.Dictionary);
			string vStr = value as string;
			float f;
			if (vStr != null && float.TryParse(vStr, out f)) { value = f; }
			Keeper.Dictionary.Set(key, value);
		}
		public void AssertNum(Tokenizer tok, object src, Show.PrintFunc print) {
			string itemName = tok.GetStr(1, Keeper.Dictionary);
			//Show.Log("!!!!%^ asserting " + itemName+"     ("+tok.str+")");
			if (itemName != null && Keeper.Dictionary.ContainsKey(itemName)) return;
			//Show.Log("!!!!%^ getting value ");
			//Show.Log("!!!!%^ checking "+tok.tokens[2]+" in "+Scope);
			object itemValue = tok.GetResolvedToken(2, Keeper.Dictionary);
			//Show.Log("!!!!%^ value is " + itemValue);
			Keeper.Dictionary.Set(itemName, itemValue);
		}
	}
}
