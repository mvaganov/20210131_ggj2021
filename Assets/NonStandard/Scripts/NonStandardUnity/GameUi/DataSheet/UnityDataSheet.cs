using NonStandard.Character;
using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.Extension;
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
	//[
	//["" "braille" null "nothing" (30)]
	//["name" "label" (na¤) "collabel" (100) "nameless"]
	//["speed" "txticon" (move.move.speed/5) "collabel" (30) 0.0]
	//// skill from skill table
	//["handicraft" "input" (data.handi¤) "collabel" (80) 0.0]
	//// TODO add columns by creating new variables in data
	//["+" "braille" (null) "addcol" (30)]
	//]
	/*
{label:"", columnUi:"braille", valueScript:null, headerUi:nothing, widthOfColumn:30}
{label:"name", columnUi:"label", valueScript:na¤, headerUi:collabel, widthOfColumn:100}
{l¤:"speed", c¤:"txticon", v¤:(move.move.speed/5), h¤:collabel, w¤:30, d¤:0.0}
{l¤:"handicraft", c¤:"input", v¤:(data.handicraft), h¤:collabel, w¤:80, d¤:0.0}
{l¤:"+", c¤:"braille", v¤:null, h¤:addcol, w¤:30}
	*/
	public class UnityDataSheetColumnInitStructure {
		public string label;
		/// <summary>
		/// could be a string, number, expression
		/// </summary>
		public Token valueScript;
		public object defaultValue;
		public Type typeOfValue;
		public string columnUi;
		public float widthOfColumn;
		public string headerUi;
		public bool alwaysLast;
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
		public string columnSetup;

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
			Show.Log(columnSetup);
			CodeConvert.TryParse(columnSetup, out UnityDataSheetColumnInitStructure[] columns, null, tokenizer);
			if (tokenizer.HasError()) {
				Show.Error("error: " + tokenizer.ErrorString());
				return;
			}

			data = new Udash();
			int index = 0;
			data.AddRange(list, tokenizer);
			for (int i = 0; i < columns.Length; ++i) {
				UnityDataSheetColumnInitStructure c = columns[i];
				c.typeOfValue = c.defaultValue != null ? c.defaultValue.GetType() : null;
				data.SetColumn(index, new Udash.ColumnSetting {
					fieldToken = c.valueScript,
					data = new UnityColumnData {
						label = c.label,
						uiBase = uiPrototypes.GetElement(c.columnUi),
						headerBase = uiPrototypes.GetElement(c.headerUi),
						width = -1,
					},
					type = c.typeOfValue,
					defaultValue = c.defaultValue
				});
				if (c.widthOfColumn > 0) {
					data.columnSettings[index].data.width = c.widthOfColumn;
				}
				++index;
			}
		}

		void GenerateHeaders() {
			if (headerRectangle == null) return;
			Vector2 cursor = Vector2.zero;
			List<GameObject> unusedHeaders = new List<GameObject>();
			for (int i = 0; i < data.columnSettings.Count; ++i) {
				Udash.ColumnSetting colS = data.columnSettings[i];
				GameObject header = null;
				if (i < headers.Count) { header = headers[i]; }
				while (i >= headers.Count) { headers.Add(null); }
				if (header != null) {
					header.transform.SetParent(null, false);
					unusedHeaders.Add(header);
					header = null;
				}
				for(int h=0;h<unusedHeaders.Count; ++h) {
					GameObject hdr = unusedHeaders[h];
					if (hdr.name.StartsWith(colS.data.headerBase.name) && UiText.GetText(hdr) == colS.data.label) {
						header = hdr;
						unusedHeaders.RemoveAt(h);
						//Debug.Log("recycling "+hdr.name);
						break;
					}
				}
				if (header == null) {
					header = Instantiate(colS.data.headerBase);
					UiText.SetText(header, colS.data.label);
				}
				header.SetActive(true);
				header.transform.SetParent(headerRectangle, false);
				header.transform.SetSiblingIndex(i);
				headers[i] = header;
				RectTransform rect = header.GetComponent<RectTransform>();
				float w = colS.data.width > 0 ? colS.data.width : rect.sizeDelta.x;
				//rect.sizeDelta = new Vector2(w, rt.sizeDelta.y);
				rect.anchoredPosition = cursor;
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
				//rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rt.rect.height);
				cursor.x += w * rt.localScale.x;
			}
			for(int i = 0; i < unusedHeaders.Count; ++i) {
				GameObject header = unusedHeaders[i];
				Destroy(header);
			}
			unusedHeaders.Clear();
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cursor.x);
		}

		private void Start() {
			if (uiPrototypes == null) {
				uiPrototypes = Global.GetComponent<UiTypedEntryPrototype>();
			}
			Init();
			GenerateHeaders();
			Proc.Enqueue(() => {
				NpcCreation npcs = Global.GetComponent<NpcCreation>();
				CharacterProxy charMove = Global.GetComponent<CharacterProxy>();
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

		GameObject CreateRow(RowData rowData, float yPosition = float.NaN) {
			GameObject rowUi = Instantiate(prefab_dataRow.gameObject);
			RowObject rObj = rowUi.GetComponent<RowObject>();
			if (rObj == null) {
				throw new Exception("RowUI prefab must have " + nameof(RowObject) + " component");
			}
			rObj.obj = rowData.model;
			if (rObj.obj == null) {
				throw new Exception("something bad. where is the object that this row is for?");
			}
			rowUi.SetActive(true);
			return UpdateRowData(rObj, rowData, yPosition);
		}
		public GameObject UpdateRowData(RowObject rObj, RowData rowData, float yPosition = float.NaN) {
			object[] columns = rowData.columns;
			//StringBuilder sb = new StringBuilder();
			Vector2 rowCursor = Vector2.zero;
			RectTransform rect;
			List<GameObject> unusedColumns = new List<GameObject>();
			for (int c = 0; c < columns.Length; ++c) {
				Udash.ColumnSetting colS = data.columnSettings[c];
				GameObject fieldUi = null;
				// if there's already a field for this column, delete it if it's not the right one.
				if(rObj.transform.childCount > c) {
					fieldUi = rObj.transform.GetChild(c).gameObject;
					if (!fieldUi.name.StartsWith(colS.data.uiBase.name)) {
						fieldUi.transform.SetParent(null, false);
						unusedColumns.Add(fieldUi);
						fieldUi = null;
					}
					//else { Debug.Log("keep using "+colS.data.uiBase.name); }
				}
				// if the field UI needs to be created
				if (fieldUi == null) {
					// check if there's a version of it from earlier
					for (int i = 0; i < unusedColumns.Count; ++i) {
						if (unusedColumns[i].name.StartsWith(colS.data.uiBase.name)) {
							fieldUi = unusedColumns[i];
							unusedColumns.RemoveAt(i);
							//Debug.Log("recycling "+ unusedColumns[i].name);
							break;
						}
					}
					// otherwise create it
					if (fieldUi == null) {
						fieldUi = Instantiate(colS.data.uiBase);
					}
					fieldUi.SetActive(true);
					fieldUi.transform.SetParent(rObj.transform, false);
					fieldUi.transform.SetSiblingIndex(c);
				}
				object value = columns[c];
				if (value != null) {
					UiText.SetText(fieldUi, value.ToString());
					//sb.Append(value.ToString() + ", ");
				}
				rect = fieldUi.GetComponent<RectTransform>();
				float w = colS.data.width > 0 ? colS.data.width : rect.sizeDelta.x;
				rect.anchoredPosition = rowCursor;
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
				rowCursor.x += w * rt.localScale.x;
			}
			for(int i = 0; i < unusedColumns.Count; ++i) { Destroy(unusedColumns[i]); }
			unusedColumns.Clear();
			//Show.Log(sb);
			rect = rObj.GetComponent<RectTransform>();
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rowCursor.x);
			rect.transform.SetParent(contentRectangle, false);
			if (!float.IsNaN(yPosition)) {
				rect.anchoredPosition = new Vector2(0, yPosition);
			}
			dataRows.Add(rObj.gameObject);
			return rObj.gameObject;
		}

		void GenerateDataRows() {
			dataRows.ForEach(go => { go.transform.SetParent(null); Destroy(go); });
			dataRows.Clear();
			Vector2 cursor = Vector2.zero;
			for (int r = 0; r < data.rows.Count; ++r) {
				GameObject rowUi = CreateRow(data.rows[r]);
				RectTransform rect = rowUi.GetComponent<RectTransform>();
				rect.anchoredPosition = cursor;
				cursor.y -= rect.rect.height;
			}
			contentRectangle.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, -cursor.y);
		}
		public void RefreshRows() {
			// map list elements to row UI
			Dictionary<object, RowObject> srcToRowUiMap = new Dictionary<object, RowObject>();
			for(int i = 0; i < contentRectangle.childCount; ++i) {
				RowObject rObj = contentRectangle.GetChild(i).GetComponent<RowObject>();
				if (rObj == null) { continue; }
				if (rObj.obj == null) {
					throw new Exception("found a row (" + rObj.transform.HierarchyPath() + ") without source object at index "+i);
				}
				srcToRowUiMap[rObj.obj] = rObj;
			}
			Vector2 cursor = Vector2.zero;
			// go through all of the row elements and put the row UI elements in the correct spot
			for(int i = 0; i < data.rows.Count; ++i) {
				RowData rd = data.rows[i];
				if (!srcToRowUiMap.TryGetValue(rd.model, out RowObject uiElement)) {
					throw new Exception("could not find "+rd.model+", call "+nameof(SyncSpreadSheetUiWith)+"?");
				}
				uiElement.transform.SetSiblingIndex(i);
				RectTransform rect = uiElement.GetComponent<RectTransform>();
				rect.anchoredPosition = cursor;
				cursor.y -= rect.rect.height;
			}
			contentRectangle.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, -cursor.y);
		}
		public void SetSortState(int column, SortState sortState) {
			SyncSpreadSheetUiWith(list);
			data.SetSortState(column, sortState);
			RefreshRows();
		}
		public void SyncSpreadSheetUiWith(IList<object> list) {
			// go through the list. if it has an element missing from the spreadsheet, add it to the spreadsheet (including UI)
			for (int i = 0; i < list.Count; ++i) {
				RowData rd = data.GetRowData(list[i]); // TODO O^2 operation. should a dictionary cache these relationships? does it matter in practice?
				//if (rd != null) { Debug.Log(rd.model+" skip!"); continue; }
				if (rd == null) { rd = data.AddData(list[i], errLog); }
				int uiIndex = contentRectangle.IndexOfChild(c => {
					RowObject rObj = c.GetComponent<RowObject>();
					if (rObj != null && rObj.obj == rd.model) { return true; }
					return false;
				});
				bool haveUi = uiIndex >= 0;
				if (!haveUi) {
					Debug.Log("new row");
					GameObject rowUi = CreateRow(rd);
				} else {
					Debug.Log("updating "+rd.model);
					RowObject rObj = contentRectangle.GetChild(uiIndex).GetComponent<RowObject>();
					// if this row doesn't have the right number of columns, or the columns are not the right ones, remake them.
					//if(rd.columns.Length != rObj.transform.childCount) {
						UpdateRowData(rObj, rd);
					//}
				}
			}
			// go through the spreadsheet. if it has an element missing from the list, remove it from the spreadsheet (including UI)
			for (int i = data.rows.Count-1; i >= 0; --i) {
				object m = data.rows[i].model;
				if (list.IndexOf(m) >= 0) { continue; }
				// if this spreadsheet row object is not in the expected list, remove it
				data.rows.RemoveAt(i);
				// also, remove the UI element
				for(int c = 0; c < contentRectangle.childCount; ++c) {
					RowObject rObj = contentRectangle.GetChild(c).GetComponent<RowObject>();
					if (rObj == null) continue;
					if (rObj.obj == m) { rObj.transform.SetParent(null); Destroy(rObj); }
				}
			}
		}
		public void Sort() {
			SyncSpreadSheetUiWith(list);
			if (data.Sort()) {
				RefreshRows();
			}
		}
		public Udash.ColumnSetting GetColumn(int index) { return data.GetColumn(index); }

		public Udash.ColumnSetting AddColumn() {
			//Show.Log("TODO add column");
			Udash.ColumnSetting column = new Udash.ColumnSetting {
				fieldToken = new Token("",0,0),
				data = new UnityColumnData {
					label = "new data",
					uiBase = uiPrototypes.GetElement("input"),
					headerBase = uiPrototypes.GetElement("collabel"),
					width = -1,
				},
				type = typeof(double),
				defaultValue = (double)0
			};
			data.AddColumn(column);
			GenerateHeaders();
			SyncSpreadSheetUiWith(list);
			return column;
		}

		// TODO allow editing with a special menu
		public void EditColumn(int index) {
			// data script: text input field
			// column label text: text input field
			// column field type: dropdown referencing the children of 'typed item prototype'
			// default value: text input field
			// default value type: dropdown referencing types allowed in column field type
			Show.Log("TODO allow editing with a special menu");
		}

		// TODO allow columns to be removed
		public void RemoveColumn(int index) {
			Show.Log("TODO allow columns to be removed after a confirmation popup");
		}
	}
}