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
	public class DataSheet : DataSheet<ColumnData> {
		public DataSheet() : base() { }
		public DataSheet(Tokenizer unfilteredFormat, int indexOfValueElement = 0) : base(unfilteredFormat, indexOfValueElement) { }
	}
	public enum SortState { None, Ascending, Descening }
	public class DataSheet<MetaData> where MetaData : new() {
		/// <summary>
		/// the actual data
		/// </summary>
		public List<object[]> data = new List<object[]>();
		/// <summary>
		/// data about the columns
		/// </summary>
		public List<ColumnData> columns = new List<ColumnData>();
		/// <summary>
		/// which column is sorted first?
		/// </summary>
		protected List<int> columnSortOrder = new List<int>();

		public class ColumnData {
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
			/// 
			/// </summary>
			public object defaultValue = null;

			public object Resolve(object scope) {
				object result;
				if (needsToLoadEditPath) {
					Show.Log("need to compile " + editPath.JoinToString());
					List<object> compiledPath = new List<object>();
					result = ReflectionParseExtension.GetValueFromRawPath(scope, editPath, null, compiledPath);
					editPath = compiledPath;
					Show.Log("compiled " + editPath.JoinToString(",",o=>o?.GetType()?.ToString() ?? "???")+" : "+result);
					needsToLoadEditPath = false;
				} else if (editPath != null) {
					ReflectionParseExtension.TryGetValueCompiled(scope, editPath, out result);
				} else {
					result = fieldToken.Resolve(new Tokenizer(), scope, true, true);
				}
				return result;
			}

			public void RefreshEditPath() {
				// the field can have an editPath if it doesn't contain any binary operators except for member operators
				List<Token> allTokens = new List<Token>();
				bool isValidEditableField = TokenOnlyContains(_fieldToken, new ParseRuleSet[] { 
					CodeRules.Expression, CodeRules.MembershipOperator, CodeRules.SquareBrace }, allTokens);
				StringBuilder sb = new StringBuilder();
				sb.Append(fieldToken.GetAsSmallText() + " " + isValidEditableField + "\n");
				Show.Log(sb);
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
			public bool SetValue(object scope, object value) {
				if (!canEdit) return false;
				if (needsToLoadEditPath) {
					// TODO load editPath properly

					needsToLoadEditPath = false;
				}
				// TODO use proper editPath to assign the given value.
				// convert the type before assigning it?
				// if the value is not the right type, throw an error?
				return true;
			}
		}

		public DataSheet() { }
		public DataSheet(Tokenizer unfilteredFormat, int indexOfValueElement = 0) {
			List<Token> fieldTokens = GetValueTokens(unfilteredFormat, indexOfValueElement);
			SetColumnCount(fieldTokens.Count);
			for(int i = 0; i < columns.Count; ++i) {
				columns[i].fieldToken = fieldTokens[i];
			}
		}

		public object Get(int row, int col) { return data[row][col]; }
		public void Set(int row, int col, object value) { data[row][col] = value; }

		public object this [int row, int col] { get => Get(row, col); set => Set(row, col, value); }
		public object[] this [int row] { get => data[row]; }

		public void SetSortState(int column, SortState sortState) {
			int columnImportance = columnSortOrder.IndexOf(column);
			if (columnImportance >= 0) { columnSortOrder.RemoveAt(columnImportance); }
			columns[column].sortState = sortState;
			if (sortState == SortState.None) { return; }
			columnSortOrder.Insert(0, column);
			Sort();
		}

		public void SetColumnCount(int count) {
			if (columns.Count < count) { columns.Capacity = count; }
			for (int i = columns.Count; i < count; ++i) { columns.Add(new ColumnData()); }
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
			data = new List<object[]>();
			InsertRange(0, source, errLog);
		}

		public void InsertRange(int index, IList<object> source, TokenErrLog errLog) {
			object[][] items = new object[source.Count][];
			for (int i = 0; i < source.Count; ++i) {
				items[i] = GenerateRow(source[i], errLog);
			}
			data.InsertRange(index, items);
		}
		public void AddRange(IList<object> source, TokenErrLog errLog) { InsertRange(data.Count, source, errLog); }
		public void AddData(object elementForRow, TokenErrLog errLog) { data.Add(GenerateRow(elementForRow, errLog)); }

		public object[] GenerateRow(object source, TokenErrLog errLog) {
			if(errLog == null) { errLog = new Tokenizer(); }
			object[] result = new object[columns.Count];

			for (int i = 0; i < result.Length; ++i) {
				//object value = columns[i].fieldToken.Resolve(errLog, source, true, true);
				object value = columns[i].Resolve(source);
				if(value is CodeRules.DefaultString && columns[i].defaultValue != null) {
					value = columns[i].defaultValue;// (float)0;
				}
				result[i] = value;
				//if (fieldTokens[i].ToString() == "()") { Show.Log("oh hai "+result[i]); }
			}
			return result;
		}

		public int DefaultSort(object a, object b) {
			if (a == b) { return 0; }
			if (a == null && b != null) { return 1; }
			if (a != null && b == null) { return -1; }
			Type ta = a.GetType(), tb = b.GetType();
			if ((ta.IsAssignableFrom(typeof(double)) || ta.IsAssignableFrom(typeof(long)))
			&&  (tb.IsAssignableFrom(typeof(double)) || tb.IsAssignableFrom(typeof(long)))) {
				ta = tb = typeof(double);
				a = Convert.ChangeType(a, ta);
				b = Convert.ChangeType(b, tb);
			}
			if (ta == tb) {
				if (ta == typeof(double)) { return Comparer<double>.Default.Compare((double)a, (double)b); }
				if (ta == typeof(string)) { return StringComparer.Ordinal.Compare(a, b); }
			}
			return 0;
		}

		public int RowSort(object[] rowA, object[] rowB) {
			for (int i = 0; i < columnSortOrder.Count; ++i) {
				int index = columnSortOrder[i];
				if (columns[index].sortState == SortState.None) {
					//Show.Log("SortState not being set...");
					continue;
				}
				Comparison<object> sort = columns[index].sort;
				if (sort == null) { sort = DefaultSort; }
				int comparison = sort.Invoke(rowA[index], rowB[index]);
				//Show.Log(comparison+" compare " + rowA[index]+" vs "+ rowB[index]+"   " + rowA[index].GetType() + " vs " + rowB[index].GetType());
				if (comparison == 0) { continue; }
				if (columns[index].sortState == SortState.Descening) { comparison *= -1; }
				return comparison;
			}
			return 0;
		}

		public void Sort() {
			//Show.Log("SORTING "+columnSortOrder.JoinToString());
			data.Sort(RowSort);
		}

		public void SetColumn(int index, ColumnData column) {
			while (columns.Count <= index) { columns.Add(new ColumnData()); }
			columns[index] = column;
		}
		public ColumnData GetColumn(int index) { return columns[index]; }
	}
}
