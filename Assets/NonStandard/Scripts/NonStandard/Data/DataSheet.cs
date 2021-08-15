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

			public void RefreshEditPath() {
				// the field can have an editPath if it doesn't contain any binary operators except for member operators
				List<Token> toks = new List<Token>();
				_fieldToken.FlattenInto(toks);
				StringBuilder sb = new StringBuilder();
				sb.Append(fieldToken.GetAsSmallText());
				for(int i = 0; i < toks.Count; ++i) {
					ParseRuleSet.Entry pc = toks[i].meta as ParseRuleSet.Entry;
					if (pc == null) continue;
					sb.Append(" | "+pc.parseRules.name+"@"+toks[i].index);
				}
				Show.Log(sb);
				Show.Log(fieldToken.Stringify());
				//string tokenText = fieldToken.GetAsSmallText();
				//Show.Log(tokenText);
				editPath = null;
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
				object value = columns[i].fieldToken.Resolve(errLog, source, true, true);
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
