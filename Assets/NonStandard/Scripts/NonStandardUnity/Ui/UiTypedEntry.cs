using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.Extension;
using NonStandard.Utility.UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NonStandard.Ui {
	public class UiTypedEntry : MonoBehaviour {

		[TextArea(1,10)]
		public string fields;
		public ObjectPtr ptr;

		const int columnTitleIndex = 0, uiTypeIndex = 1, valueIndex = 2;

		public class UiTypedColumnData : ColumnData {
			public GameObject uiBase;
		}

		public void Start() {
			Tokenizer tokenizer = new Tokenizer();
			tokenizer.Tokenize(fields);
			if (tokenizer.errors.Count > 0) {
				Debug.LogWarning(tokenizer.ErrorString());
			}
			Debug.Log(tokenizer);
			object obj = ptr.data;

			// TODO make a copy of the tokenizer with only the value comoponents, for faster token resolution.
			// TODO have DataSheet calculate the things
			// TODO implement a sorting UI also, so the rows can be sorted

			DataSheet_< UiTypedColumnData> ds = new DataSheet_<UiTypedColumnData>(tokenizer, valueIndex);
			ds.AddData(obj);
			List<Token> tokenizedList = tokenizer.tokens[0].GetTokenSublist();
			for(int i = 1; i < tokenizedList.Count-1; ++i) {
				List<Token> list = tokenizedList[i].GetTokenSublist();
				DataSheet_<UiTypedColumnData>.Column col = ds.columns[i - 1];
				col.data.label = list[columnTitleIndex + 1].ToString();
				string uiName = list[uiTypeIndex + 1].ToString();
				col.data.uiBase = Global.Get<UiTypedEntryPrototype>().GetElement(uiName);
			}

			//Tokenizer justValues = GetTokenizerOfValues(tokenizer);
			//Show.Log($"0:{e0}  1:{e1}");

			object r = tokenizer.GetResolvedToken(0, obj);
			List<object> elementList = r as List<object>;
			Vector2 cursor = Vector2.zero;
			for(int i = 0; i < elementList.Count; ++i) {
				List<object> list = elementList[i] as List<object>;
				Show.Log(list[columnTitleIndex] + "(" + list[uiTypeIndex] + "): " + list[valueIndex].GetType().ToString());
				GameObject prototype = Global.Get<UiTypedEntryPrototype>().GetElement(list[uiTypeIndex].ToString());
				GameObject uiPart = Instantiate(prototype);
				RectTransform rect = uiPart.GetComponent<RectTransform>();
				if(rect != null) {
					rect.SetParent(transform,false);
					rect.anchoredPosition = cursor;
					//rect.localPosition = cursor;
					cursor.x += rect.rect.width;
					UiText.SetText(uiPart, list[valueIndex].ToString());
				}
			}
		}
	}
}