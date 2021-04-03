using UnityEngine;

namespace NonStandard.Cli {
	public static class ManageUI
	{
		[System.Serializable]
		public class RectTransformSettings
		{
			public Vector2 AnchorMin = Vector2.zero;
			public Vector2 AnchorMax = Vector2.one;
			public Vector2 OffsetMin = Vector2.zero;
			public Vector2 OffsetMax = Vector2.zero;
		}

		[System.Serializable]
		public class InitialColorSettings
		{
			public Color Background = new Color(0, 0, 0, 0.5f);
			public Color Text = new Color(1, 1, 1);
			public Color ErrorText = new Color(1, .5f, .5f);
			public Color SpecialText = new Color(1, .75f, 0);
			public Color ExceptionText = new Color(1, .5f, 1);
			public Color Scrollbar = new Color(1, 1, 1, 0.5f);
			public Color UserInput = new Color(.75f, .875f, .75f);
			public Color UserSelection = new Color(.5f, .5f, 1, .75f);
			private string _cachedUInputHex; private Color _cachedUInputColor;
			public string UserInputHex {
				get {
					if (_cachedUInputColor != UserInput)
					{
						_cachedUInputHex = Util.ColorToHexCode(UserInput);
						_cachedUInputColor = UserInput;
					}
					return _cachedUInputHex;
				}
			}
			public string ErrorTextHex { get { return Util.ColorToHexCode(ErrorText); } }
			public string SpecialTextHex { get { return Util.ColorToHexCode(SpecialText); } }
			public string ExceptionTextHex { get { return Util.ColorToHexCode(ExceptionText); } }
		}

		public const string mainTextObjectName = "MainText";

		public static Canvas GenerateUIElements(Transform self,
			InteractivityEnum Interactivity,
			int commandLineWidth,
			PutItInWorldSpace WorldSpaceSettings,
			ref Canvas _mainView,
			ref TMPro.TMP_InputField _tmpInputField,
			TMPro.TMP_FontAsset textMeshProFont,
			InitialColorSettings ColorSet,
			RectTransformSettings ScreenOverlaySettings)
		{
			_mainView = self.GetComponentInParent<Canvas>();
			if (!_mainView)
			{
				_mainView = (new GameObject("canvas")).AddComponent<Canvas>(); // so that the UI can be drawn at all
				_mainView.renderMode = RenderMode.ScreenSpaceOverlay;
				if (!_mainView.GetComponent<UnityEngine.UI.CanvasScaler>())
				{
					_mainView.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>(); // so that text is pretty when zoomed
				}
				if (!_mainView.GetComponent<UnityEngine.UI.GraphicRaycaster>())
				{
					_mainView.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>(); // so that mouse can select input area
				}
				_mainView.transform.SetParent(self);
			}
			GameObject tmpGo = new GameObject("user input");
			tmpGo.transform.SetParent(_mainView.transform);
			UnityEngine.UI.Image img = tmpGo.AddComponent<UnityEngine.UI.Image>();
			img.color = ColorSet.Background;
			if (ScreenOverlaySettings == null)
			{
				MaximizeRectTransform(tmpGo.transform);
			} else
			{
				RectTransform r = tmpGo.GetComponent<RectTransform>();
				r.anchorMin = ScreenOverlaySettings.AnchorMin;
				r.anchorMax = ScreenOverlaySettings.AnchorMax;
				r.offsetMin = ScreenOverlaySettings.OffsetMin;
				r.offsetMax = ScreenOverlaySettings.OffsetMax;
			}
			_tmpInputField = tmpGo.AddComponent<TMPro.TMP_InputField>();
			_tmpInputField.lineType = TMPro.TMP_InputField.LineType.MultiLineNewline;
			_tmpInputField.textViewport = _tmpInputField.GetComponent<RectTransform>();
			TMPro.TextMeshProUGUI tmpText;
#if UNITY_EDITOR
			try
			{
#endif
				tmpText = (new GameObject(mainTextObjectName)).AddComponent<TMPro.TextMeshProUGUI>();
#if UNITY_EDITOR
			} catch (System.Exception)
			{
				throw new System.Exception("Could not create a TextMeshProUGUI object. Did you get default fonts into TextMeshPro? Window -> TextMeshPro -> Import TMP Essential Resources");
			}
#endif
			if (textMeshProFont != null)
			{
				tmpText.font = textMeshProFont;
			}
			tmpText.fontSize = 20;
			tmpText.transform.SetParent(tmpGo.transform);
			_tmpInputField.textComponent = tmpText;
			_tmpInputField.fontAsset = tmpText.font;
			_tmpInputField.pointSize = tmpText.fontSize;
			MaximizeRectTransform(tmpText.transform);

			tmpGo.AddComponent<UnityEngine.UI.RectMask2D>();
			_tmpInputField.onFocusSelectAll = false;
			tmpText.color = ColorSet.Text;
			_tmpInputField.selectionColor = ColorSet.UserSelection;
			_tmpInputField.customCaretColor = true;
			_tmpInputField.caretColor = ColorSet.UserInput;
			_tmpInputField.caretWidth = 5;

			if (_tmpInputField.verticalScrollbar == null)
			{
				GameObject scrollbar = new GameObject("scrollbar vertical");
				scrollbar.transform.SetParent(_tmpInputField.transform);
				scrollbar.AddComponent<RectTransform>();
				_tmpInputField.verticalScrollbar = scrollbar.AddComponent<UnityEngine.UI.Scrollbar>();
				_tmpInputField.verticalScrollbar.direction = UnityEngine.UI.Scrollbar.Direction.TopToBottom;
				RectTransform r = scrollbar.GetComponent<RectTransform>();
				r.anchorMin = new Vector2(1, 0);
				r.anchorMax = Vector2.one;
				r.offsetMax = Vector3.zero;
				r.offsetMin = new Vector2(-16, 0);
			}
			if (_tmpInputField.verticalScrollbar.handleRect == null)
			{
				GameObject slideArea = new GameObject("sliding area");
				slideArea.transform.SetParent(_tmpInputField.verticalScrollbar.transform);
				RectTransform r = slideArea.AddComponent<RectTransform>();
				MaximizeRectTransform(slideArea.transform);
				r.offsetMin = new Vector2(10, 10);
				r.offsetMax = new Vector2(-10, -10);
				GameObject handle = new GameObject("handle");
				UnityEngine.UI.Image bimg = handle.AddComponent<UnityEngine.UI.Image>();
				bimg.color = ColorSet.Scrollbar;
				handle.transform.SetParent(slideArea.transform);
				r = handle.GetComponent<RectTransform>();
				r.anchorMin = r.anchorMax = Vector2.zero;
				r.offsetMax = new Vector2(5, 5);
				r.offsetMin = new Vector2(-5, -5);
				r.pivot = Vector2.one;
				_tmpInputField.verticalScrollbar.handleRect = r;
				_tmpInputField.verticalScrollbar.targetGraphic = img;
			}
			// an event system is required... if there isn't one, make one
			UnityEngine.EventSystems.StandaloneInputModule input =
				GameObject.FindObjectOfType(typeof(UnityEngine.EventSystems.StandaloneInputModule))
				as UnityEngine.EventSystems.StandaloneInputModule;
			if (input == null)
			{
				input = (new GameObject("<EventSystem>")).AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
			}
			// put all UI in the UI layer
			Util.SetLayerRecursive(_mainView.gameObject, LayerMask.NameToLayer("UI"));
			// turn it off and then back on again... that fixes some things.
			tmpGo.SetActive(false); tmpGo.SetActive(true);
			// put it in the world (if needed)
			if (Interactivity == InteractivityEnum.WorldSpaceOnly
			|| Interactivity == InteractivityEnum.ActiveScreenAndInactiveWorld)
			{
				WorldSpaceSettings.ApplySettingsTo(_mainView);
				RecalculateFontSize(_tmpInputField, commandLineWidth, WorldSpaceSettings);
			}

			return _mainView;
		}

		public static void ConnectInput(ref TMPro.TMP_InputField _tmpInputField,
			TMPro.TMP_InputValidator InputValidator,
			UnityEngine.Events.UnityAction<string> listener_OnValueChanged,
			UnityEngine.Events.UnityAction<string, int, int> listener_OnTextSelectionChange)
		{
			_tmpInputField.ActivateInputField();
			_tmpInputField.onValueChanged.AddListener(listener_OnValueChanged);
			_tmpInputField.onTextSelection.AddListener(listener_OnTextSelectionChange);

			_tmpInputField.characterValidation = TMPro.TMP_InputField.CharacterValidation.CustomValidator;
			_tmpInputField.inputValidator = InputValidator;// GetInputValidator();
		}

		public static float CalculateIdealFontSize(TMPro.TMP_Text tmpText, float idealCharsPerLine,
				PutItInWorldSpace WorldSpaceSettings)
		{
			float normalCharacterWidth = tmpText.font.characterLookupTable[(int)'e'].glyph.metrics.horizontalAdvance;
			//characterDictionary[(int)'e'].xAdvance;
			float idealFontSize = (WorldSpaceSettings.screenSize.x * tmpText.font.faceInfo.pointSize//fontInfo.PointSize
				) / (idealCharsPerLine * normalCharacterWidth);
			return idealFontSize;
		}
		public static void RecalculateFontSize(CmdLine_base self)
		{
			RecalculateFontSize(self._tmpInputField, self.commandLineWidth, self.WorldSpaceSettings);
		}
		public static void RecalculateFontSize(TMPro.TMP_InputField _tmpInputField, int commandLineWidth,
			PutItInWorldSpace WorldSpaceSettings)
		{
			TMPro.TMP_Text tmpText = _tmpInputField.textComponent;
			tmpText.fontSize = CalculateIdealFontSize(tmpText, commandLineWidth + .125f, WorldSpaceSettings);
		}
		private static RectTransform MaximizeRectTransform(Transform t)
		{
			return MaximizeRectTransform(t.GetComponent<RectTransform>());
		}
		private static RectTransform MaximizeRectTransform(RectTransform r)
		{
			r.anchorMax = Vector2.one;
			r.anchorMin = r.offsetMin = r.offsetMax = Vector2.zero;
			return r;
		}

		public enum InteractivityEnum { Disabled, ScreenOverlayOnly, WorldSpaceOnly, ActiveScreenAndInactiveWorld };
		[System.Serializable]
		public class KeyUsedTo
		{
			[Tooltip("Which key shows the terminal")]
			public KeyCode activate = KeyCode.BackQuote;
			[Tooltip("Which key hides the terminal")]
			public KeyCode deactivate = KeyCode.Escape;
		}

		[System.Serializable]
		public class PutItInWorldSpace
		{
			[Tooltip("If zero, will automatically set to current Screen's pixel size")]
			public Vector2 screenSize = new Vector2(0, 0);
			[Tooltip("how many meters each pixel should be")]
			public float textScale = 0.005f;
			public PutItInWorldSpace(float scale, Vector2 size)
			{
				this.textScale = scale;
				this.screenSize = size;
			}
			public void ApplySettingsTo(Canvas c)
			{
				if (screenSize == Vector2.zero) { screenSize = new Vector2(Screen.width, Screen.height); }
				RectTransform r = c.GetComponent<RectTransform>();
				r.sizeDelta = screenSize;
				c.transform.localPosition = Vector3.zero;
				c.transform.localRotation = Quaternion.identity;
				r.anchoredPosition = Vector2.zero;
				r.localScale = Vector3.one * textScale;
			}
		}

		public static void PrintPrompt(CmdLine_base self)
		{
			//int indexBeforePrompt = self.data.GetRawText().Length;
			//if (self.indexWherePromptWasPrintedRecently != -1)
			//{
			//	indexBeforePrompt = self.indexWherePromptWasPrintedRecently;
			//}
			//// commander has the prompt artifact because commander knows the possible machine name
			//string promptText = self.commander.CommandPromptArtifact();
			//self.data.AddText(promptText);
			//self.indexWherePromptWasPrintedRecently = indexBeforePrompt;
			//self.data.SetCursorIndex(self.data.WriteCursor);
			string promptText = self.commander.CommandPromptArtifact();
			self.data.WriteOutput(promptText);
		}
		public static bool IsInOverlayMode(CmdLine_base self)
		{
			return self._mainView.renderMode == RenderMode.ScreenSpaceOverlay;
		}
		public static void PositionInWorld(CmdLine_base self, Vector3 center, Vector2 size = default(Vector2), float scale = 0.005f)
		{
			if (size == Vector2.zero) size = new Vector2(Screen.width, Screen.height);
			PutItInWorldSpace ws = new PutItInWorldSpace(scale, size);
			self.transform.position = center;
			if (self._mainView == null)
			{
				self.WorldSpaceSettings = ws;
			} else
			{
				ws.ApplySettingsTo(self._mainView);
			}
			RecalculateFontSize(self);
		}
		public static void SetOverlayModeInsteadOfWorld(CmdLine_base self, bool useOverlay)
		{
			if (useOverlay && self._mainView.renderMode != RenderMode.ScreenSpaceOverlay)
			{
				self._mainView.renderMode = RenderMode.ScreenSpaceOverlay;
			} else if (!useOverlay)
			{
				self._mainView.renderMode = RenderMode.WorldSpace;
				self.WorldSpaceSettings.ApplySettingsTo(self._mainView);
				RecalculateFontSize(self);
			}
		}

		public static bool IsVisible(CmdLine_base self)
		{
			return self._mainView != null && self._mainView.gameObject.activeInHierarchy;
		}
		/// <summary>shows (true) or hides (false).</summary>
		public static void SetVisibility(CmdLine_base self, bool visible)
		{
			if (self._mainView == null)
			{
				self.ActiveOnStart = visible;
			} else
			{
				self._mainView.gameObject.SetActive(visible);
			}
		}
		/// <param name="enabled">If <c>true</c>, reads from keybaord. if <c>false</c>, stops reading from keyboard</param>
		public static void SetInputActive(CmdLine_base self)
		{
			if (self._tmpInputField.interactable) { self._tmpInputField.ActivateInputField(); } 
			else { self._tmpInputField.DeactivateInputField(); }
		}
		/// <param name="enableInteractive"><c>true</c> to turn this on (and turn the previous CmdLine off)</param>
		public static void SetInteractive(CmdLine_base self, bool enableInteractive)
		{
			if (self._mainView == null && self.Interactivity != ManageUI.InteractivityEnum.Disabled)
			{
				self.CreateUI();
			}
			if (self._tmpInputField == null) { return; }
			bool activityWhenStarted = self._tmpInputField.interactable;
			if (enableInteractive && CmdLine_base.currentlyActiveCmdLine != null)
			{
				SetInteractive(CmdLine_base.currentlyActiveCmdLine, false);
			}
			self._tmpInputField.interactable = enableInteractive; // makes focus possible
			switch (self.Interactivity)
			{
				case InteractivityEnum.Disabled:
					SetVisibility(self, false);
					break;
				case InteractivityEnum.ScreenOverlayOnly:
					if (!IsInOverlayMode(self))
					{
						SetOverlayModeInsteadOfWorld(self, true);
					}
					SetVisibility(self, enableInteractive);
					break;
				case InteractivityEnum.WorldSpaceOnly:
					if (!IsVisible(self))
					{
						SetVisibility(self, true);
					}
					if (enableInteractive)
						SetOverlayModeInsteadOfWorld(self, false);
					break;
				case InteractivityEnum.ActiveScreenAndInactiveWorld:
					//Debug.Log("switching "+ enableInteractive);
					if (!IsVisible(self))
					{
						SetVisibility(self, true);
					}
					SetOverlayModeInsteadOfWorld(self, enableInteractive);
					break;
			}
			self._tmpInputField.verticalScrollbar.value = 1; // scroll to the bottom
			//MoveCaretToEnd(self); // move caret focus to end
			SetInputActive(self); // request/deny focus
			if (enableInteractive)
			{
				CmdLine_base.currentlyActiveCmdLine = self;
			} else if (CmdLine_base.currentlyActiveCmdLine == self)
			{
				// if this command line has disabled the user
				if (CmdLine_base.disabledUserControls == CmdLine_base.currentlyActiveCmdLine)
				{
					// tell it to re-enable controls
					if (!self.callbacks.ignoreCallbacks && self.callbacks.whenThisDeactivates != null)
						self.callbacks.whenThisDeactivates.Invoke();
					CmdLine_base.disabledUserControls = null;
				}
				CmdLine_base.currentlyActiveCmdLine = null;
			}
		}
		public static bool IsInteractive(CmdLine_base self) {
			return self._tmpInputField != null && self._tmpInputField.interactable;
		}
		///// <summary>Moves the caret to the end, clearing all selections in the process</summary>
		//public static void MoveCaretToEnd(CmdLine_base self)
		//{
		//	int lastPoint = self.data.GetRawText().Length;
		//	self.data.SetCursorIndex(lastPoint);
		//}

		public static bool IsInteractivityBeingToggled(CmdLine_base self)
		{
			// toggle visibility based on key presses
			bool toggle = Input.GetKeyDown(ManageUI.IsInteractive(self) ? self.keyUsedTo.deactivate : self.keyUsedTo.activate);
			// or toggle visibility when 5 fingers touch
			if (Input.touches.Length == 5)
			{
				if (!self._togglingVisiblityWithMultitouch)
				{
					toggle = true;
					self._togglingVisiblityWithMultitouch = true;
				}
			} else
			{
				self._togglingVisiblityWithMultitouch = false;
			}
			return toggle;
		}

		public static void ToggleInteractivity(CmdLine_base self)
		{
			if (!ManageUI.IsInteractive(self))
			{
				// check to see how clearly the user is looking at this CmdLine
				if (self._mainView.renderMode == RenderMode.ScreenSpaceOverlay)
				{
					self.viewscore = 1;
				} else
				{
					Transform cameraTransform = Camera.main.transform;
					Vector3 lookPosition = cameraTransform.position;
					Vector3 gaze = cameraTransform.forward;
					Vector3 delta = self.transform.position - lookPosition;
					float distFromCam = delta.magnitude;
					float viewAlignment = Vector3.Dot(gaze, delta / distFromCam);
					if (viewAlignment < 0)
					{
						self.viewscore = -1;
					} else
					{
						self.viewscore = (1 - viewAlignment) * distFromCam;
					}
				}
				if (CmdLine_base.currentlyActiveCmdLine == null
					|| (CmdLine_base.currentlyActiveCmdLine != null && (CmdLine_base.currentlyActiveCmdLine.viewscore < 0
						|| (self.viewscore >= 0 && self.viewscore <= CmdLine_base.currentlyActiveCmdLine.viewscore))))
				{
					SetInteractive(self, true);
				}
			} else
			{
				SetInteractive(self, false);
				self.viewscore = -1;
			}
		}

		public static void UpdateUIInteractivity(CmdLine_base self)
		{
			if (self.Interactivity != ManageUI.InteractivityEnum.Disabled)
			{
				if (IsInteractivityBeingToggled(self))
				{
					ToggleInteractivity(self);
				}
				UpdateScrolling(self, Input.GetAxis("Mouse ScrollWheel"));
			}
		}

		public static void UpdateScrolling(CmdLine_base self, float mouseScrollWheel)
		{
			// stop trying to show the bottom if the user wants to scroll, unless the user scrolled all the way down
			if (mouseScrollWheel != 0)
			{
				self.showBottomWhenTextIsAdded = self._tmpInputField.verticalScrollbar.value == 1;
			}
			if (self.showBottomWhenTextIsAdded)
			{
				// if the vertical scrollbar handle is not the entire handle (meaning there is something to scroll)
				if (self._tmpInputField.verticalScrollbar.size < 1)
				{
					// force scrolling to the bottom
					self._tmpInputField.verticalScrollbar.value = 1;
				}
			}
		}
	}
}