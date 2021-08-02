using NonStandard.Data.Parse;
using System;
using System.Collections.Generic;

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
	public class DataSheet : DataSheet_<ColumnData> {
		public DataSheet() : base() { }
		public DataSheet(Tokenizer unfilteredFormat, int indexOfValueElement = 0) : base(unfilteredFormat, indexOfValueElement) { }
	}
	public class DataSheet_<MetaData> where MetaData : new() {
		public Tokenizer fieldFormat;
		/// <summary>
		/// the actual data
		/// </summary>
		public List<object[]> data = new List<object[]>();
		/// <summary>
		/// data about the columns
		/// </summary>
		public List<Column> columns = new List<Column>();
		/// <summary>
		/// which column is sorted first?
		/// </summary>
		protected List<int> columnSortOrder = new List<int>();

		public enum SortState { None, Ascending, Descening }
		public class Column {
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
		}

		public DataSheet_() { }
		public DataSheet_(Tokenizer unfilteredFormat, int indexOfValueElement = 0) {
			InitFormat(unfilteredFormat, indexOfValueElement);
			SetColumnCount(fieldFormat.tokens.Count);
		}

		public object Get(int row, int col) { return data[row][col]; }
		public void Set(int row, int col, object value) { data[row][col] = value; }

		public object this [int row, int col] { get => Get(row, col); set => Set(row, col, value); }
		public object[] this [int row] { get => data[row]; }

		public void SetColumnCount(int count) {
			if (columns.Count < count) { columns.Capacity = count; }
			for (int i = columns.Count; i < count; ++i) { columns.Add(new Column()); }
		}

		public void InitFormat(Tokenizer unfilteredFieldFormat, int indexOfValueElement = 0) {
			fieldFormat = GetFilteredTokenizerOfValues(unfilteredFieldFormat, indexOfValueElement);
		}

		public static Tokenizer GetFilteredTokenizerOfValues(Tokenizer tokenizer, int indexOfValueElement = 0) {
			Tokenizer justValues = new Tokenizer();
			justValues.str = tokenizer.str;
			List<Token> main = tokenizer.tokens[0].GetTokenSublist();
			// main[0] is "[" and main[main.Count-1] is "]"
			for (int i = 1; i < main.Count - 1; ++i) {
				List<Token> entry = main[i].GetTokenSublist();
				// +1 is needed because element 0 is "["
				justValues.tokens.Add(entry[indexOfValueElement + 1]);
			}
			return justValues;
		}

		public void InitData(IList<object> source) {
			data = new List<object[]>();
			InsertRange(0, source);
		}

		public void InsertRange(int index, IList<object> source) {
			object[][] items = new object[source.Count][];
			for (int i = 0; i < source.Count; ++i) {
				items[i] = GenerateRow(source[i]);
			}
			data.InsertRange(index, items);
		}
		public void AddRange(IList<object> source) { InsertRange(data.Count, source); }
		public void AddData(object elementForRow) { data.Add(GenerateRow(elementForRow)); }

		public object[] GenerateRow(object source) {
			object[] result = new object[fieldFormat.tokens.Count];
			for (int i = 0; i < result.Length; ++i) {
				result[i] = fieldFormat.GetResolvedToken(i, source);
			}
			return result;
		}

		public int RowSort(object[] rowA, object[] rowB) {
			for (int i = 0; i < columnSortOrder.Count; ++i) {
				int index = columnSortOrder[i];
				if (columns[index].sortState == SortState.None) { continue; }
				Comparison<object> sort = columns[index].sort;
				int comparison = sort(rowA[index], rowB[index]);
				if (comparison == 0) continue;
				if (columns[index].sortState == SortState.Descening) { comparison *= -1; }
				return comparison;
			}
			return 0;
		}

		public void Sort() {
			data.Sort(RowSort);
		}
	}
}