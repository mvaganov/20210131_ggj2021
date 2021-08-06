using NonStandard.Character;
using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.Extension;
using NonStandard.Process;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NonStandard.Ui {

	public class UnityColumnData : ColumnData {
		public GameObject uiBase;
		public GameObject headerBase;
		public float width;
	}
	public class Udash : DataSheet<UnityColumnData> { }
	public class UnityDataSheet : MonoBehaviour {
		const int columnTitleIndex = 0, uiTypeIndex = 1, valueIndex = 2, headerUiType = 3, columnWidth = 4;
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
			// TODO implement a sorting UI also, so the rows can be sorted

			data = new Udash();
			//data.InitFormat(tokenizer, valueIndex);
			List<Token> fieldFormat = DataSheet.GetValueTokens(tokenizer, valueIndex);
			data.AddRange(list, tokenizer);
			List<Token> tokenizedList = tokenizer.tokens[0].GetTokenSublist();
			for (int i = 1; i < tokenizedList.Count - 1; ++i) {
				List<Token> list = tokenizedList[i].GetTokenSublist();
				int index = i - 1;
				string uiName = list[uiTypeIndex + 1].Resolve(tokenizer, null, true, true).ToString();
				string headerName = list[headerUiType + 1].Resolve(tokenizer, null, true, true).ToString();
				string label = list[columnTitleIndex + 1].Resolve(tokenizer, null, true, true).ToString();
				//Debug.Log(uiPrototypes+"    "+index + "  uiName "+uiName+"   headerName "+headerName);
				data.SetColumn(index, new Udash.ColumnData {
					data = new UnityColumnData {
						label = label,
						uiBase = uiPrototypes.GetElement(uiName),
						headerBase = uiPrototypes.GetElement(headerName),
						width = -1
					},
					type = null,
				}, list[valueIndex + 1]);
				//Debug.Log(uiName + "::: " + data.columns[index].data.uiBase +"\n"
				//	+ headerName + "::: " + data.columns[index].data.headerBase);
				if (list.Count > columnWidth + 1) {
					object resolved = list[columnWidth + 1].Resolve(tokenizer, null, true, true);
					//Debug.Log(label+" w: "+resolved);
					float w = Convert.ToSingle(resolved);
					data.columns[index].data.width = w;
				}
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
				CharacterMoveProxy charMove = Global.Get<CharacterMoveProxy>();
				List<object> chars = new List<object>();
				chars.Add(charMove.Target);
				chars.AddRange(npcs.npcs);
				Show.Log("listing "+chars.JoinToString());
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
	}
}