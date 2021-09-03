﻿using NonStandard.Character;
using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.Extension;
using NonStandard.Process;
using NonStandard.Ui;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi.DataSheet {

	public class UnityColumnData : ColumnData {
		/// <summary>
		/// prefab used for the column data elements
		/// </summary>
		public GameObject uiBase;
		/// <summary>
		/// prefab used for the column header element
		/// </summary>
		public GameObject headerBase;
		/// <summary>
		/// width of the column
		/// </summary>
		public float width;
		/// <summary>
		/// if true, will move itself to far right end, even if new elements are added
		/// </summary>
		public bool alwaysLast;
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
		public List<GameObject> dataRowsUi = new List<GameObject>();
		public UiTypedEntryPrototype uiPrototypes;
		protected RectTransform rt;
		protected Tokenizer errLog = new Tokenizer();

		[TextArea(1, 10)]
		public string columnSetup;

		public int GetRowUiElement(GameObject rowObject) {
			return dataRowsUi.IndexOf(rowObject);
		}

		private void Awake() {
			rt = GetComponent<RectTransform>();
		}
		private void Init() {
			rt = GetComponent<RectTransform>();
			data = new Udash();
			InitColumnSettings(columnSetup);
		}
		void InitColumnSettings(string columnSetup) {
			Show.Log(columnSetup);
			Tokenizer tokenizer = new Tokenizer();
			CodeConvert.TryParse(columnSetup, out UnityDataSheetColumnInitStructure[] columns, null, tokenizer);
			if (tokenizer.HasError()) {
				Show.Error("error parsing column structure: " + tokenizer.GetErrorString());
				return;
			}
			int index = 0;
			//data.AddRange(list, tokenizer);
			for (int i = 0; i < columns.Length; ++i) {
				UnityDataSheetColumnInitStructure c = columns[i];
				c.typeOfValue = c.defaultValue != null ? c.defaultValue.GetType() : null;
				data.SetColumn(index, new Udash.ColumnSetting (data) {
					fieldToken = c.valueScript,
					data = new UnityColumnData {
						label = c.label,
						uiBase = uiPrototypes.GetElement(c.columnUi),
						headerBase = uiPrototypes.GetElement(c.headerUi),
						width = -1,
						alwaysLast = c.alwaysLast
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
			// put old headers aside. they may be reused.
			List<GameObject> unusedHeaders = new List<GameObject>();
			for (int i = 0; i < headers.Count; ++i) {
				GameObject header = headers[i];
				if (header != null) {
					header.transform.SetParent(null, false);
					unusedHeaders.Add(header);
				}
			}
			headers.Clear();
			for (int i = 0; i < data.columnSettings.Count; ++i) {
				Udash.ColumnSetting colS = data.columnSettings[i];
				GameObject header = null;
				while (i >= headers.Count) { headers.Add(null); }
				// check if the header we need is in the old header list
				for(int h = 0; h < unusedHeaders.Count; ++h) {
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
					header.name = header.name.Replace("Clone", colS.data.label);
					UiText.SetText(header, colS.data.label);
				}
				ColumnHeader ch = header.GetComponent<ColumnHeader>();
				if (ch != null) { ch.columnSetting = colS; }
				header.SetActive(true);
				header.transform.SetParent(headerRectangle, false);
				header.transform.SetSiblingIndex(i);
				headers[i] = header;
				RectTransform rect = header.GetComponent<RectTransform>();
				rect.anchoredPosition = cursor;
				float w = rect.sizeDelta.x;
				if (colS.data.width > 0) {
					w = colS.data.width;
					rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
				} else {
					colS.data.width = w; // if the width isn't set, use the default width of the column header
				}
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
			//string test = "{a:1,b:[a,[1,2],{a:a,b:[b]}],c:{a:1,b:2}}";
			//CodeConvert.TryParse(test, out object obj);
			//Show.Log(obj.Stringify(pretty:true));
		}

		public void Load(List<object> source) {
			//list = source;
			data.InitData(source, errLog);
			GenerateDataRows();
		}

		RowObject CreateRow(RowData rowData, float yPosition = float.NaN) {
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
			UpdateRowData(rObj, rowData, yPosition);
			return rObj;
		}
		public GameObject UpdateRowData(RowObject rObj, RowData rowData, float yPosition = float.NaN) {
			object[] columns = rowData.columns;
			//StringBuilder sb = new StringBuilder();
			Vector2 rowCursor = Vector2.zero;
			RectTransform rect;
			List<GameObject> unusedColumns = new List<GameObject>();
			for(int i = 0; i < rObj.transform.childCount; ++i) {
				GameObject fieldUi = rObj.transform.GetChild(i).gameObject;
				if (fieldUi != null) {//!fieldUi.name.StartsWith(colS.data.uiBase.name)) {
					fieldUi.transform.SetParent(null, false);
					unusedColumns.Add(fieldUi);
				}
			}
			for (int c = 0; c < columns.Length; ++c) {
				Udash.ColumnSetting colS = data.columnSettings[c];
				GameObject fieldUi = null;
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
				object value = columns[c];
				if (value != null) {
					UiText.SetText(fieldUi, value.ToString());
					//sb.Append(value.ToString() + ", ");
				} else {
					UiText.SetText(fieldUi, "");
				}
				rect = fieldUi.GetComponent<RectTransform>();
				rect.anchoredPosition = rowCursor;
				float w = rect.sizeDelta.x;// colS.data.width > 0 ? colS.data.width : rect.sizeDelta.x;
				if (colS.data.width > 0) {
					w = colS.data.width;
					//Show.Log(colS.data.label+" width should be "+w);
					rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
				}
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
			dataRowsUi.Add(rObj.gameObject);
			return rObj.gameObject;
		}

		void GenerateDataRows() {
			dataRowsUi.ForEach(go => { go.transform.SetParent(null); Destroy(go); });
			dataRowsUi.Clear();
			Vector2 cursor = Vector2.zero;
			for (int r = 0; r < data.rows.Count; ++r) {
				GameObject rowUi = CreateRow(data.rows[r]).gameObject;
				RectTransform rect = rowUi.GetComponent<RectTransform>();
				rect.anchoredPosition = cursor;
				cursor.y -= rect.rect.height;
			}
			contentRectangle.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, -cursor.y);
		}
		public void RefreshColumnText(int column, TokenErrLog errLog) {
			for(int i = 0; i < dataRowsUi.Count; ++i) {
				GameObject fieldUi = dataRowsUi[i].transform.GetChild(column).gameObject;
				object value = data.RefreshValue(i, column, errLog);
				if (errLog.HasError()) return;
				if (value != null) {
					UiText.SetText(fieldUi, value.ToString());
					//sb.Append(value.ToString() + ", ");
				} else {
					UiText.SetText(fieldUi, "");
				}

			}
		}
		public void ResizeColumnWidth(int column, float oldWidth, float newWidth) {
			Show.Log("TODO resize width of column "+column+" from "+oldWidth+" to "+newWidth);
			// go through the header, change the width of the item in question, and push forward every entry after it
			// do the same for each row.
		}
		public void MoveColumn(int oldIndex, int newIndex) {
			// need to re-arrange headers in data
			data.MoveColumn(oldIndex, newIndex);
			// change the index of the column in the header (UI)
			GameObject go = headers[oldIndex];
			headers.RemoveAt(oldIndex);
			headers.Insert(newIndex, go);
			// regenerate headers based on new column settings
			GenerateHeaders();
			// then once all the data and headers are in the right place, refresh the bulk of the UI to match
			RefreshRowsAndColumns(); // TODO figure out why this isn't working
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
					throw new Exception("could not find "+rd.model+", call "+nameof(RefreshAll)+"?");
				}
				uiElement.transform.SetSiblingIndex(i);
				RectTransform rect = uiElement.GetComponent<RectTransform>();
				rect.anchoredPosition = cursor;
				cursor.y -= rect.rect.height;
			}
			contentRectangle.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, -cursor.y);
		}
		public void SetSortState(int column, SortState sortState) {
			RefreshRowsAndColumns();
			data.SetSortState(column, sortState);
			RefreshRows();
		}
		public object GetItem(int index) { return data.rows[index].model; }
		public object SetItem(int index, object dataModel) { return data.rows[index].model = dataModel; }
		public int IndexOf(object dataModel) { return data.IndexOf(dataModel); }
		public int IndexOf(Func<object, bool> predicate) { return data.IndexOf(predicate); }
		public void RefreshAll() {
			List<RowObject> unusedRows = new List<RowObject>();
			// go through the spreadsheet UI. if it there is an element that is missing in the data, delete it.
			for (int i = 0; i < contentRectangle.childCount; ++i) {
				RowObject rObj = contentRectangle.GetChild(i).GetComponent<RowObject>();
				if (rObj == null) { throw new Exception("non row element in the spreadsheet UI?"); }
				int index = IndexOf(rObj.obj);
				if (index < 0) {
					rObj.transform.SetParent(null);
					unusedRows.Add(rObj);
				}
				// don't sort here because it's not clear that all of the required elements are in UI
			}
			// go through the spreadsheet data. if there is no ui element, create it
			float y = 0;
			for (int i = 0; i < data.rows.Count; ++i) {
				RowData rd = data.rows[i];
				RowObject rObj = null;
				if (i < contentRectangle.childCount) {
					rObj = contentRectangle.GetChild(i).GetComponent<RowObject>();
					if (rObj.obj != rd.model) { rObj = null; }
				}
				if (rObj != null) continue;
				bool uiExists = false;
				for (int j = 0; j < contentRectangle.childCount; ++j) {
					rObj = contentRectangle.GetChild(j).GetComponent<RowObject>();
					if (rObj.obj == rd.model) { uiExists = true; break; }
				}
				bool columnsNeedChecking = true;
				if (!uiExists) {
					if (unusedRows.Count > 0) {
						rObj = unusedRows[unusedRows.Count - 1];
						rObj.transform.SetParent(contentRectangle);
						unusedRows.RemoveAt(unusedRows.Count - 1);
					} else {
						rObj = CreateRow(data.rows[i], y);
						columnsNeedChecking = false;
					}
				}
				if (columnsNeedChecking) {
					// go through the columns and make sure the columns are correctly ordered
					UpdateRowData(rObj, rd, y);
				}
				// sort the ui elements to match the data
				rObj.transform.SetSiblingIndex(i);
				y += rObj.GetComponent<RectTransform>().rect.height;
			}
		}
		public void Sort() {
			RefreshRowsAndColumns();
			if (data.Sort()) {
				RefreshRows();
			}
		}
		public void RefreshRowsAndColumns() {
			RefreshAll();
		}
		public Udash.ColumnSetting GetColumn(int index) { return data.GetColumn(index); }

		public Udash.ColumnSetting AddColumn() {
			Udash.ColumnSetting column = new Udash.ColumnSetting(data) {
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
			MakeSureColumnsMarkedLastAreLast();
			GenerateHeaders();
			RefreshAll();
			return column;
		}

		void MakeSureColumnsMarkedLastAreLast() {
			List<Udash.ColumnSetting> moveToEnd = new List<DataSheet<UnityColumnData>.ColumnSetting>();
			for (int i = 0; i < data.columnSettings.Count; ++i) {
				if (data.columnSettings[i].data.alwaysLast) {
					moveToEnd.Add(data.columnSettings[i]);
					data.columnSettings.RemoveAt(i);
					--i;
				}
			}
			if (moveToEnd.Count > 0) {
				for (int i = 0; i < moveToEnd.Count; ++i) {
					data.columnSettings.Add(moveToEnd[i]);
				}
				moveToEnd.Clear();
			}

		}
		// TODO allow columns to be removed
		public void RemoveColumn(int index) {
			Show.Log("TODO allow columns to be removed after a confirmation popup");
		}
	}
}