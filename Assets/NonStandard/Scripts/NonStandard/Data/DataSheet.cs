using NonStandard.Data.Parse;
using NonStandard.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace NonStandard.Data {
	public class ColumnData {
		/// <summary>
		/// label at the top of the column
		/// </summary>
		public string label;
		/// <summary>
		/// hover-over description for column label
		/// </summary>
		public string description;
	}
	public class RowData {
		public object model;
		public object[] columns;
		public RowData(object model, object[] columns) { this.model = model; this.columns = columns; }
	}
	public class DataSheet : DataSheet<ColumnData> {
		public DataSheet() : base() { }
		public DataSheet(Tokenizer unfilteredFormat, int indexOfValueElement = 0) : base(unfilteredFormat, indexOfValueElement) { }
	}
	public enum SortState { None, Ascending, Descening }
	public class DataSheet<MetaData> where MetaData : new() {
		/// <summary>
		/// the actual data
		/// </summary>
		public List<RowData> rows = new List<RowData>();
		/// <summary>
		/// data about the columns
		/// </summary>
		public List<ColumnSetting> columnSettings = new List<ColumnSetting>();
		/// <summary>
		/// which column is sorted first?
		/// </summary>
		protected List<int> columnSortOrder = new List<int>();

		public class ColumnSetting {
			private Token _fieldToken;
			/// <summary>
			/// what field is being read (or modified) in this column
			/// </summary>
			public Token fieldToken { get => _fieldToken; set {
					_fieldToken = value;
					RefreshEditPath();
				}
			}
			/// <summary>
			/// if this field is editable, this list of members will be used to edit
			/// </summary>
			public List<object> editPath;
			/// <summary>
			/// calculated when <see cref="fieldToken"/> is set to determine if data can be written back to the row object
			/// </summary>
			public bool canEdit;
			/// <summary>
			/// the edit path is loaded if no edit path exists when values change
			/// </summary>
			public bool needsToLoadEditPath;
			/// <summary>
			/// what type the values in this column are expected to be
			/// </summary>
			public Type type;
			/// <summary>
			/// additional meta-data for each colum. eg: UI used to display? word-wrap? text alignment/direction? colorization rules? ...
			/// </summary>
			public MetaData data = new MetaData();
			/// <summary>
			/// expectations for sorting
			/// </summary>
			public SortState sortState = SortState.None;
			/// <summary>
			/// specific algorithms used to sort
			/// </summary>
			public Comparison<object> sort;
			/// <summary>
			/// what to resolve in this column if the path is missing or erroneous
			/// </summary>
			public object defaultValue = null;

			object CompileEditPath(object scope) {
				//Show.Log("need to compile " + editPath.JoinToString());
				List<object> compiledPath = new List<object>();
				object result = ReflectionParseExtension.GetValueFromRawPath(scope, editPath, defaultValue, compiledPath);
				editPath = compiledPath;
				//ReflectionParseExtension.TryGetValueCompiledPath(scope, editPath, out result);
				//Show.Log("compiled " + editPath.JoinToString(",",o=>o?.GetType()?.ToString() ?? "???")+" : "+result);
				needsToLoadEditPath = false;
				return result;
			}

			public object GetValue(TokenErrLog errLog, object scope) {
				object result;
				if (needsToLoadEditPath) {
					result = CompileEditPath(scope);
				} else if (editPath != null) {
					if(!ReflectionParseExtension.TryGetValueCompiledPath(scope, editPath, out result)) {
						result = defaultValue;
					}
				} else {
					result = fieldToken.Resolve(errLog, scope, true, true);
					if (errLog.HasError()) {
						result = defaultValue;
					}
				}
				return FilterType(result);
			}

			public object FilterType(object value) {
				if (type != null) { CodeConvert.Convert(ref value, type); }
				return value;
			}

			public bool SetValue(object scope, object value) {
				//Show.Log("attempting to set " + _fieldToken.GetAsSmallText() + " to " + value);
				if (!canEdit) return false;
				if (needsToLoadEditPath) {
					CompileEditPath(scope);
				}
				value = FilterType(value);
				if (!ReflectionParseExtension.TrySetValueCompiledPath(scope, editPath, value)) {
					Show.Log("unable to set " + _fieldToken.GetAsSmallText() + " to " + value);
				}
				ReflectionParseExtension.TryGetValueCompiledPath(scope, editPath, out object result);
				//Show.Log("set " + scope + "." + _fieldToken.GetAsSmallText() + " to " + result);
				return true;
			}

			public void RefreshEditPath() {
				// the field can have an editPath if it doesn't contain any binary operators except for member operators
				List<Token> allTokens = new List<Token>();
				bool isValidEditableField = TokenOnlyContains(_fieldToken, new ParseRuleSet[] { 
					CodeRules.Expression, CodeRules.MembershipOperator, CodeRules.SquareBrace }, allTokens);
				//StringBuilder sb = new StringBuilder();
				//sb.Append(fieldToken.GetAsSmallText() + " " + isValidEditableField + "\n");
				//Show.Log(sb);
				editPath = null;
				if (!isValidEditableField) {
					canEdit = false;
					needsToLoadEditPath = false;
					return;
				}
				canEdit = true;
				needsToLoadEditPath = true;
				//sb.Clear();
				editPath = new List<object>();
				for(int i = 0; i < allTokens.Count; ++i) {
					ParseRuleSet.Entry pc = allTokens[i].meta as ParseRuleSet.Entry;
					if (pc != null) continue;
					editPath.Add(allTokens[i]);
					//sb.Append(allTokens[i].GetAsSmallText()+"\n");
				}
				//Show.Log(sb);
			}
			/// <summary>
			/// asserts that the given token only contains the given valid parser rules
			/// </summary>
			/// <param name="token"></param>
			/// <param name="validParserRules"></param>
			/// <param name="allTokens">if not null, will put all tokens within the main token argument into this list as per <see cref="Token.FlattenInto(List{Token})"/></param>
			/// <returns></returns>
			public bool TokenOnlyContains(Token token, IList<ParseRuleSet> validParserRules, List<Token> allTokens = null) {
				if (allTokens == null) allTokens = new List<Token>();
				token.FlattenInto(allTokens);
				for (int i = 0; i < allTokens.Count; ++i) {
					ParseRuleSet.Entry pc = allTokens[i].meta as ParseRuleSet.Entry;
					if (pc == null) continue;
					if (validParserRules.IndexOf(pc.parseRules) < 0) { return false; }
				}
				return true;
			}
		}

		public DataSheet() { }
		public DataSheet(Tokenizer unfilteredFormat, int indexOfValueElement = 0) {
			List<Token> fieldTokens = GetValueTokens(unfilteredFormat, indexOfValueElement);
			SetColumnCount(fieldTokens.Count);
			for(int i = 0; i < columnSettings.Count; ++i) {
				columnSettings[i].fieldToken = fieldTokens[i];
			}
		}

		public object Get(int row, int col) { return rows[row].columns[col]; }
		public void Set(int row, int col, object value) { rows[row].columns[col] = value; }

		public object this [int row, int col] { get => Get(row, col); set => Set(row, col, value); }
		public object[] this [int row] { get => rows[row].columns; }

		/// <param name="column">which column is having it's sort statechanged</param>
		/// <param name="sortState">the new <see cref="SortState"/> for this column</param>
		public void SetSortState(int column, SortState sortState) {
			int columnImportance = columnSortOrder.IndexOf(column);
			if (columnImportance >= 0) { columnSortOrder.RemoveAt(columnImportance); }
			columnSettings[column].sortState = sortState;
			if (sortState == SortState.None) {
				columnSortOrder.Remove(column);
				return;
			}
			columnSortOrder.Insert(0, column);
			Sort();
		}

		public void SetColumnCount(int count) {
			if (columnSettings.Count < count) { columnSettings.Capacity = count; }
			for (int i = columnSettings.Count; i < count; ++i) { columnSettings.Add(new ColumnSetting()); }
		}

		public static bool IsValidColumnDescription(List<Token> entry) {
			ParseRuleSet.Entry pre = entry[0].GetAsContextEntry();
			//Show.Log(entry.Count + ": " + pre.IsEnclosure + " " + pre.parseRules.name);
			return pre.IsEnclosure;
		}
		public static List<Token> GetValueTokens(Tokenizer tokenizer, int indexOfValueElement = 0) {
			List<Token> justValues = new List<Token>();
			List<Token> main = tokenizer.tokens[0].GetTokenSublist();
			// main[0] is "[" and main[main.Count-1] is "]"
			for (int i = 1; i < main.Count - 1; ++i) {
				List<Token> entry = main[i].GetTokenSublist();
				if (!IsValidColumnDescription(entry)) { continue; }
				// +1 is needed because element 0 is "["
				justValues.Add(entry[indexOfValueElement + 1]);
			}
			return justValues;
		}

		public void InitData(IList<object> source, TokenErrLog errLog) {
			rows = new List<RowData>();
			InsertRange(0, source, errLog);
		}

		public void InsertRange(int index, IList<object> source, TokenErrLog errLog) {
			RowData[] newRows = new RowData[source.Count];
			for (int i = 0; i < source.Count; ++i) {
				newRows[i] = GenerateRow(source[i], errLog);
			}
			rows.InsertRange(index, newRows);
		}
		public void AddRange(IList<object> source, TokenErrLog errLog) { InsertRange(rows.Count, source, errLog); }
		public RowData AddData(object elementForRow, TokenErrLog errLog) {
			RowData rd = GenerateRow(elementForRow, errLog);
			rows.Add(rd);
			return rd;
		}

		public RowData GenerateRow(object source, TokenErrLog errLog) {
			if(errLog == null) { errLog = new Tokenizer(); }
			object[] result = new object[columnSettings.Count];

			for (int i = 0; i < result.Length; ++i) {
				//object value = columns[i].fieldToken.Resolve(errLog, source, true, true);
				object value = columnSettings[i].GetValue(errLog, source);
				if(value is CodeRules.DefaultString && columnSettings[i].defaultValue != null) {
					value = columnSettings[i].defaultValue;// (float)0;
				}
				result[i] = value;
				//if (fieldTokens[i].ToString() == "()") { Show.Log("oh hai "+result[i]); }
			}
			return new RowData(source, result);
		}

		public RowData GetRowData(object model) {
			for(int i = 0; i < rows.Count; ++i) {
				if (rows[i].model == model) return rows[i];
			}
			return null;
		}

		public int DefaultSort(object a, object b) {
			if (a == b) { return 0; }
			if (a == null && b != null) { return 1; }
			if (a != null && b == null) { return -1; }
			Type ta = a.GetType(), tb = b.GetType();
			if ((ta.IsAssignableFrom(typeof(double)) || ta.IsAssignableFrom(typeof(long)))
			&&  (tb.IsAssignableFrom(typeof(double)) || tb.IsAssignableFrom(typeof(long)))) {
				ta = tb = typeof(double);
				CodeConvert.Convert(ref a, ta);
				CodeConvert.Convert(ref b, tb);
				//Show.Log(a + "vs" + b);
			}
			if (ta == tb) {
				if (ta == typeof(double)) { return Comparer<double>.Default.Compare((double)a, (double)b); }
				if (ta == typeof(string)) { return StringComparer.Ordinal.Compare(a, b); }
			}
			return 0;
		}

		public int RowSort(RowData rowA, RowData rowB) {
			for (int i = 0; i < columnSortOrder.Count; ++i) {
				int index = columnSortOrder[i];
				if (columnSettings[index].sortState == SortState.None) {
					//Show.Log("SortState not being set...");
					continue;
				}
				Comparison<object> sort = columnSettings[index].sort;
				if (sort == null) { sort = DefaultSort; }
				int comparison = sort.Invoke(rowA.columns[index], rowB.columns[index]);
				//Show.Log(comparison+" compare " + rowA.columns[index]+" vs "+ rowB.columns[index]+"   " + rowA.columns[index].GetType() + " vs " + rowB.columns[index].GetType());
				if (comparison == 0) { continue; }
				if (columnSettings[index].sortState == SortState.Descening) { comparison *= -1; }
				return comparison;
			}
			return 0;
		}

		public bool Sort() {
			if (columnSortOrder == null || columnSortOrder.Count == 0) { return false; }
			//Show.Log("SORTING "+columnSortOrder.JoinToString(", ",i=>i.ToString()+":"+columnSettings[i].sortState));
			//StringBuilder sb = new StringBuilder(); sb.Append(rows.JoinToString(",", r => r.model.ToString()) + "\n");
			rows.Sort(RowSort);
			//sb.Append(rows.JoinToString(",", r => r.model.ToString()) + "\n"); Show.Log(sb);
			return true;
		}

		public void SetColumn(int index, ColumnSetting column) {
			while (columnSettings.Count <= index) { columnSettings.Add(new ColumnSetting()); }
			columnSettings[index] = column;
		}
		public ColumnSetting GetColumn(int index) { return columnSettings[index]; }
	}
}
