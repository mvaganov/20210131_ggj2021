﻿using NonStandard.Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NonStandard.GameUi.DataSheet {
	public class DataSheetWindow : MonoBehaviour {
		#if UNITY_EDITOR
		public Sprite icon;
		[SerializeField] private string title;
		public UnityEvent_List_object dataPopulator;
		[TextArea(1, 10)] public string columnSetup;
		#endif
		public Image iconObj;
		public TMP_Text titleTextObj;
		public UnityDataSheet dataSheetObj;

		#if UNITY_EDITOR
		private void OnValidate() {
			titleTextObj.text = title;
			iconObj.sprite = icon;
			dataSheetObj.dataPopulator = dataPopulator;
			dataSheetObj.columnSetup = columnSetup;
		}
		#endif

		public string Title {
			get => titleTextObj.text;
			set {
				#if UNITY_EDITOR
				title = value;
				#endif
				titleTextObj.text = value;
			}
		}
		public Sprite Icon {
			get => iconObj.sprite;
			set {
				#if UNITY_EDITOR
				icon = value;
				#endif
				iconObj.sprite = value;
			}
		}
		public UnityEvent_List_object DataPopulator {
			get => dataSheetObj.dataPopulator;
			set {
				#if UNITY_EDITOR
				dataPopulator = value;
				#endif
				dataSheetObj.dataPopulator = value;
			}
		}
		public string ColumnSetup {
			get => dataSheetObj.columnSetup;
			set {
				#if UNITY_EDITOR
				columnSetup = value;
				#endif
				dataSheetObj.columnSetup = value;
			}
		}
	}
}