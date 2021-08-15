using NonStandard.Character;
using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.Process;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Ui {

	public class UnityColumnData : ColumnData {
		public GameObject uiBase;
		public GameObject headerBase;
		public float width;
	}
	public class Udash : DataSheet<UnityColumnData> { }
	public class UnityDataSheet : MonoBehaviour {
		const int columnTitleIndex = 0, uiTypeIndex = 1, valueIndex = 2, headerUiType = 3, columnWidth = 4, defaultValueIndex = 5;
		public RectTransform headerRectangle;
		public RectTransform contentRectangle;
		public GameObject prefab_dataRow;
		public Udash data = new Udash();
		public List<GameObject> headers = new List<GameObject>();
		public List<GameObject> dataRows = new List<GameObject>();
		public UiTypedEntryPrototype uiPrototypes;
		protected RectTransform rt;
		protected Tokenizer errLog = new Tokenizer();

		[TextArea(1, 10)]
		public string fields;

		public List<object> list = new List<object>();

		public int GetRow(GameObject rowObject) {
			return dataRows.IndexOf(rowObject);
		}

		private void Awake() {
			rt = GetComponent<RectTransform>();
		}

		private void Init() {
			rt = GetComponent<RectTransform>();
			Tokenizer tokenizer = new Tokenizer();
			tokenizer.Tokenize(fields);
			if (tokenizer.errors.Count > 0) {
				Debug.LogWarning(tokenizer.ErrorString());
			}
			data = new Udash();
			data.AddRange(list, tokenizer);
			List<Token> tokenizedList = tokenizer.tokens[0].GetTokenSublist();
			int index = 0;
			// offset, required because tokenizer lists include the beginning and ending tokens, like "[" and "]"
			const int o = 1;
			for (int i = o; i < tokenizedList.Count - 1; ++i) {
				List<Token> list = tokenizedList[i].GetTokenSublist();
				if (!DataSheet.IsValidColumnDescription(list)) { continue; }
				string uiName = list[uiTypeIndex + o].Resolve(tokenizer, null, true, true).ToString();
				string headerName = list[headerUiType + o].Resolve(tokenizer, null, true, true).ToString();
				string label = list[columnTitleIndex + o].Resolve(tokenizer, null, true, true).ToString();
				object defaultValue = list.Count > defaultValueIndex ? list[defaultValueIndex + o].Resolve(tokenizer, null, true, true) : null;
				//Debug.Log(uiPrototypes+"    "+index + "  uiName "+uiName+"   headerName "+headerName);
				data.SetColumn(index, new Udash.ColumnData {
					fieldToken = list[valueIndex + o],
					data = new UnityColumnData {
						label = label,
						uiBase = uiPrototypes.GetElement(uiName),
						headerBase = uiPrototypes.GetElement(headerName),
						width = -1,
					},
					type = null,
					defaultValue = defaultValue
				});
				//Debug.Log(uiName + "::: " + data.columns[index].data.uiBase +"\n"
				//	+ headerName + "::: " + data.columns[index].data.headerBase);
				if (list.Count > columnWidth + o) {
					object resolved = list[columnWidth + o].Resolve(tokenizer, null, true, true);
					//Debug.Log(label+" w: "+resolved);
					float w = Convert.ToSingle(resolved);
					data.columns[index].data.width = w;
				}
				++index;
			}
		}

		void GenerateHeaders() {
			if (headerRectangle == null) return;
			Vector2 cursor = Vector2.zero;
			for (int i = 0; i < data.columns.Count; ++i) {
				Udash.ColumnData cold = data.columns[i];
				GameObject go = null;
				if (i < headers.Count) { go = headers[i]; }
				while (i >= headers.Count) { headers.Add(null); }
				if (go != null) {
					Destroy(go);
					go = null;
				}
				if (go == null) {
					go = Instantiate(cold.data.headerBase);
					go.SetActive(true);
				}
				headers[i] = go;
				go.transform.SetParent(headerRectangle, false);
				UiText.SetText(go, cold.data.label);
				RectTransform rect = go.GetComponent<RectTransform>();
				float w = cold.data.width > 0 ? cold.data.width : rect.sizeDelta.x;
				//rect.sizeDelta = new Vector2(w, rt.sizeDelta.y);
				rect.anchoredPosition = cursor;
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
				//rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rt.rect.height);
				cursor.x += w * rt.localScale.x;
			}
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cursor.x);
		}

		private void Start() {
			if (uiPrototypes == null) {
				uiPrototypes = Global.Get<UiTypedEntryPrototype>();
			}
			Init();
			GenerateHeaders();
			Proc.Enqueue(() => {
				NpcCreation npcs = Global.Get<NpcCreation>();
				CharacterProxy charMove = Global.Get<CharacterProxy>();
				List<object> chars = new List<object>();
				chars.Add(charMove.Target);
				chars.AddRange(npcs.npcs);
				//Show.Log("listing "+chars.JoinToString());
				Load(chars);
			});
		}

		public void Load(List<object> source) {
			list = source;
			data.InitData(list, errLog);
			GenerateDataRows();
		}

		void GenerateDataRows() {
			dataRows.ForEach(go => { go.transform.SetParent(null); Destroy(go); });
			dataRows.Clear();
			Vector2 cursor = Vector2.zero;
			for (int r = 0; r < data.data.Count; ++r) {
				GameObject rowUi = Instantiate(prefab_dataRow.gameObject);
				rowUi.SetActive(true);
				dataRows.Add(rowUi);
				RectTransform rect = rowUi.GetComponent<RectTransform>();
				rect.transform.SetParent(contentRectangle, false);
				rect.anchoredPosition = cursor;
				object[] row = data.data[r];
				Vector2 rowCursor = Vector2.zero;
				cursor.y -= rect.rect.height;
				//StringBuilder sb = new StringBuilder();
				for (int c = 0; c < row.Length; ++c) {
					Udash.ColumnData cold = data.columns[c];
					GameObject fieldUi = Instantiate(cold.data.uiBase);
					fieldUi.SetActive(true);
					fieldUi.transform.SetParent(rowUi.transform, false);
					object value = row[c];
					if (value != null) {
						UiText.SetText(fieldUi, value.ToString());
						//sb.Append(value.ToString() + ", ");
					}
					rect = fieldUi.GetComponent<RectTransform>();
					float w = cold.data.width > 0 ? cold.data.width : rect.sizeDelta.x;
					rect.anchoredPosition = rowCursor;
					rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
					rowCursor.x += w * rt.localScale.x;
				}
				//Show.Log(sb);
				rect = rowUi.GetComponent<RectTransform>();
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rowCursor.x);
			}
			contentRectangle.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, -cursor.y);
		}
		public void SetSortState(int column, SortState sortState) {
			data.SetSortState(column, sortState);
			GenerateDataRows();
		}

		public Udash.ColumnData GetColumn(int index) { return data.GetColumn(index); }

		// TODO
		public void RemoveColumn(int index) { }
		public void AddColumn() {

		}
	}
}