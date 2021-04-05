//#define UNKNOWN_CMDLINE_APPEARS
using UnityEngine;
using System;
using System.Collections.Generic;

namespace NonStandard.Cli {
	/// <summary>A Command Line emulation for Unity3D
	/// <description>Unliscence - This code is Public Domain, don't bother me about it!</description>
	/// <author email="mvaganov@hotmail.com">Michael Vaganov</author>
	public class CmdLine_base : MonoBehaviour
	{
		/// <summary>the user object that should be used for normal input into this CmdLine</summary>
		public object UserRawInput { get { return _tmpInputField; } }

		/// the thing that runs commands
		public Commander commander;

		/// <param name="command">name of the command to add (case insensitive)</param>
		/// <param name="handler">code to execute with this command, think standard main</param>
		/// <param name="help">reference information, think concise man-pages. Make help <c>"\b"</c> for hidden commands</param>
		public static void AddCommand(string command, Commander.Command.Handler handler, string help) {
			Instance.commander.addCommand(command, handler, help);
		}

		/// <summary>Enqueues a command to run, which will be run during the next Update</summary>
		public static void DoCommand(string commandWithArguments, object fromWho = null) {
			bool isNewInstance = _instance == null;
			Instance.commander.EnqueueRun(new Commander.Instruction() { text = commandWithArguments, source = fromWho });
			if (isNewInstance) { Instance.Interactivity = ManageUI.InteractivityEnum.Disabled; }
		}

		public static void DoSystemCommand(string command, object whosAsking = null) {
			bool isNewInstance = _instance == null;
			Instance.commander.DoSystemCommand(command, whosAsking);
			if (isNewInstance) { Instance.Interactivity = ManageUI.InteractivityEnum.Disabled; }
		}

		/// <summary>populates the command-line with commands</summary>
		public virtual void PopulateWithBasicCommands() {}

		#region user interface
		public string PromptArtifact = "$ ";
		public string alternateCommandExecutable = "";
		[Tooltip("the main viewable UI component")]
		public Canvas _mainView;
		[SerializeField]
		private ManageUI.InteractivityEnum interactivity = ManageUI.InteractivityEnum.ActiveScreenAndInactiveWorld;
		public ManageUI.InteractivityEnum Interactivity {
			get { return interactivity; }
			set { interactivity = value; ManageUI.SetInteractive(this, ManageUI.IsInteractive(this)); }
		}

		public ManageUI.KeyUsedTo keyUsedTo = new ManageUI.KeyUsedTo();

		[Tooltip("used to size the console Rect Transform on creation as a UI overlay")]
		public ManageUI.RectTransformSettings ScreenOverlaySettings;
		[Tooltip("fill this out to set the console in the world someplace")]
		public ManageUI.PutItInWorldSpace WorldSpaceSettings = new ManageUI.PutItInWorldSpace(0.005f, Vector2.zero);
		[Tooltip("used to color the console on creation")]
		public ManageUI.InitialColorSettings ColorSet = new ManageUI.InitialColorSettings();

		/// <summary>renders and receives input for commandline system</summary>
		public TMPro.TMP_InputField _tmpInputField; // TODO add additional text output for debug messages or dialog
		/// <summary>used to prevent multiple simultaneous toggles of visibility</summary>
		[HideInInspector] public bool _togglingVisiblityWithMultitouch = false;
		[Tooltip("If true, will show up and take user input immediately")]
		public bool ActiveOnStart = true;
		[Tooltip("If true, will add every line entered into a queue as a command to execute")]
		public bool AcceptingCommands = true;
		private bool _initailized = false;
		#region Debug.Log intercept
		[SerializeField, Tooltip("If true, all Debug.Log messages will be intercepted and duplicated here.")]
		private bool interceptDebugLog = false;
		public bool InterceptDebugLog { get { return interceptDebugLog; } set { interceptDebugLog = value; SetDebugLogIntercept(interceptDebugLog); } }
		/// <summary>if this object was intercepting Debug.Logs, this will ensure that it un-intercepts as needed</summary>
		private bool dbgIntercepted = false;

		public void EnableDebugLogIntercept() { SetDebugLogIntercept(InterceptDebugLog); }
		public void DisableDebugLogIntercept() { SetDebugLogIntercept(false); }
		public void SetDebugLogIntercept(bool intercept) {
#if UNITY_EDITOR
			if (!Application.isPlaying) return;
#endif
			if (intercept && !dbgIntercepted) {
				Application.logMessageReceived += HandleLog;
				dbgIntercepted = true;
			} else if (!intercept && dbgIntercepted) {
				Application.logMessageReceived -= HandleLog;
				dbgIntercepted = false;
			}
		}
		public void HandleLog(string logString, string stackTrace = "", LogType type = LogType.Log) {
			switch (type) {
				case LogType.Error:
					data.SetForeground(ColorSet.ErrorText);
					Log(logString);
					data.UnsetForeground();
					break;
				case LogType.Exception:
					data.SetForeground(ColorSet.ExceptionText);
					Log(logString);
					Log(stackTrace);
					data.UnsetForeground();
					break;
				case LogType.Warning:
					data.SetForeground(ColorSet.SpecialText);
					Log(logString);
					data.UnsetForeground();
					break;
				default:
					Log(logString);
					break;
			}
		}
		#endregion // Debug.Log intercept

		public bool NeedToRefreshUserPrompt { get; set; }

		[HideInInspector]
		/// used to prevent prompt artifact from being written more than once in a row
		public Vector2Int indexWherePromptWasPrintedRecently = new Vector2Int(-1,-1);

		[Tooltip("The TextMeshPro font used. If null, built-in-font should be used.")]
		public TMPro.TMP_FontAsset textMeshProFont;
		public TMPro.TMP_FontAsset TextMeshProFont {
			get { return textMeshProFont; }
			set {
				textMeshProFont = value;
				if (textMeshProFont != null && _mainView != null) {
					TMPro.TMP_Text[] texts = _mainView.GetComponentsInChildren<TMPro.TMP_Text>();
					for (int i = 0; i < texts.Length; ++i) {
						if (texts[i].gameObject.name == ManageUI.mainTextObjectName) {
							texts[i].font = textMeshProFont; break;
						}
					}
				}
			}
		}
		/// <summary>which command line is currently active, and disabling user controls</summary>
		/// TODO make some global manager instead of having static functions....
		public static CmdLine_base currentlyActiveCmdLine, disabledUserControls;
		/// <summary>used to check which command line is the best one for the user controlling the main camera</summary>
		[HideInInspector] public float viewscore;

		public Canvas CreateUI() {
			ManageUI.GenerateUIElements(transform, Interactivity, commandLineWidth, WorldSpaceSettings,
				ref _mainView, ref _tmpInputField, textMeshProFont, ColorSet, ScreenOverlaySettings);
			Validator v = GetInputValidator();
			ManageUI.ConnectInput(ref _tmpInputField, v, v.listener_OnValueChanged, listener_OnTextSelectionChange);
			return _mainView;
		}

		#endregion // user interface
		#region input validation
		/// handles input from the user, including delete
		[HideInInspector] public Validator inputvalidator;
		/// <summary>keeps track of user selection so that the text field can be fixed if selected text is removed</summary> TODO move to MetaText
		[HideInInspector] private int selectBegin = -1, selectEnd = -1;
		/// <summary>flag to move text view to the bottom when content is added</summary>
		[HideInInspector] public bool showBottomWhenTextIsAdded = false;
		/// <summary>if text is being modified to refresh it after the user did something naughty</summary>
		[HideInInspector] public bool addingOnChanged = false;
		[Tooltip("Maximum number of lines to retain.")]
		public int maxLines = 99;
		[SerializeField, Tooltip("lines with more characters than this will count as more than one line.")]
		public int commandLineWidth = 80;
		public int CommandLineWidth { get { return commandLineWidth; } set { SetCommandLineWidth(value); } }

		public Validator GetInputValidator() {
			if (inputvalidator == null) {
				inputvalidator = ScriptableObject.CreateInstance<Validator>();
				inputvalidator.Init(this);
			}
			return inputvalidator;
		}

		private void listener_OnTextSelectionChange(string str, int start, int end) {
			selectBegin = start;// Math.Min(start, end);
			selectEnd = end;// Math.Max(start, end);
		}
		public int SelectBegin { get { return Math.Min(selectBegin, selectEnd); } }
		public int SelectEnd { get { return Math.Max(selectBegin, selectEnd); } }

		//public MetaText data = new MetaText();
		public TTYData data = new TTYData();
		private long freshnessOfOutput;

		/// <summary>be sure to UnsetForgroundColor afterward.</summary><param name="c">color to set</param>
		public void SetForegroundColor(Color c) { data.SetForeground(c); }
		/// <summary>be sure to UnsetBackgroundColor afterward.</summary><param name="c">color to set</param>
		public void SetBackgroundColor(Color c) { data.SetBackground(c); }
		public void UnsetForegroundColor() { data.UnsetForeground(); }
		public void UnsetBackgroundColor() { data.UnsetForeground(); }
		public Color CurrentForegroundColor() { return data.CurrentForeground(); }
		public Color CurrentBackgroundColor() { return data.CurrentBackground(); }

		/// flushes the TTY data into the input field UI
		public void RefreshText() {
			int cursorIndex = 0;
			_tmpInputField.text = data.GetTMProString(out cursorIndex, commander.CommandPromptArtifact());
			_tmpInputField.stringPosition = cursorIndex;
		}

		private void SetCommandLineWidth(int newWidth) {
			commandLineWidth = newWidth;
			ManageUI.RecalculateFontSize(this);
			throw new System.Exception("TODO implement SetCommandLineWidth()");
		}

		#endregion // input validation
		#region singleton
		/// <summary>the singleton instance. One will be created if none exist.</summary>
		public static CmdLine_base _instance;
		public static CmdLine_base Instance {
			get {
				if (_instance == null && (_instance = FindObjectOfType(typeof(CmdLine)) as CmdLine) == null)
				{
					GameObject g = new GameObject();
					_instance = g.AddComponent<CmdLine>();
					g.name = "<" + _instance.GetType().Name + ">";
#if UNITY_EDITOR && UNKNOWN_CMDLINE_APPEARS
				_instance.whereItWasStarted = Environment.StackTrace;
#endif
				}
				return _instance;
			}
		}
#if UNITY_EDITOR && UNKNOWN_CMDLINE_APPEARS
	public string whereItWasStarted;
#endif
		#endregion // singleton
		#region public API

		/// <summary>what to do after a string is read.</summary>
		public delegate void DoAfterStringIsRead(string readFromUser);

		/// <param name="line">line to add as output, also turning current user input into text output</param>
		public void println(string line) {
			data.WriteOutput(line + "\n");
		}
		public void Write(string text) {
			data.WriteOutput(text);
		}
		public void ReadLineAsync(DoAfterStringIsRead stringCallback) {
			if (!ManageUI.IsInteractive(this) && _tmpInputField != null) { ManageUI.SetInteractive(this, true); }
			commander.waitingToReadLine += stringCallback;
		}
		public void GetInputAsync(DoAfterStringIsRead stringCallback) { ReadLineAsync(stringCallback); }

		public void Log(string line) { println(line); }
		public void LogError(string line) { SetForegroundColor(ColorSet.ErrorText); println(line); UnsetForegroundColor(); }
		public void LogWarning(string line) { SetForegroundColor(ColorSet.SpecialText); println(line); UnsetForegroundColor(); }
		public void ReadLine(DoAfterStringIsRead stringCallback) { ReadLineAsync(stringCallback); }

		#endregion // pubilc API
		#region Unity Editor interaction
#if UNITY_EDITOR
		private static Mesh _editorMesh = null; // one variable to enable better UI in the editor

		public List<Action> thingsToDoWhileEditorIsRunning = new List<Action>();

		void UnityEditorUIUpdates() {
			if (thingsToDoWhileEditorIsRunning.Count > 0) {
				thingsToDoWhileEditorIsRunning.ForEach(a => a());
				thingsToDoWhileEditorIsRunning.Clear();
			}
		}

		void OnValidate() {
			thingsToDoWhileEditorIsRunning.Add(() => {
				Interactivity = interactivity;
				InterceptDebugLog = interceptDebugLog;
				TextMeshProFont = textMeshProFont;
			});
		}

		void OnDrawGizmos() {
			if (_editorMesh == null) {
				_editorMesh = new Mesh();
				_editorMesh.vertices = new Vector3[] { new Vector3(-.5f, .5f), new Vector3(.5f, .5f), new Vector3(-.5f, -.5f), new Vector3(.5f, -.5f) };
				_editorMesh.triangles = new int[] { 0, 1, 2, 3, 2, 1 };
				_editorMesh.RecalculateNormals();
				_editorMesh.RecalculateBounds();
			}
			Vector3 s = this.WorldSpaceSettings.screenSize;
			if (s == Vector3.zero) { s = new Vector3(Screen.width, Screen.height, 1); }
			s.Scale(transform.lossyScale);
			s *= WorldSpaceSettings.textScale;
			Color c = ColorSet.Background;
			Gizmos.color = c;
			if (!UnityEditor.EditorApplication.isPlaying) {
				Gizmos.DrawMesh(_editorMesh, transform.position, transform.rotation, s);
			}
			Transform t = transform;
			// calculate extents
			Vector3[] points = {(t.up*s.y/2 + t.right*s.x/-2),(t.up*s.y/2 + t.right*s.x/2),
			(t.up*s.y/-2 + t.right*s.x/2),(t.up*s.y/-2 + t.right*s.x/-2)};
			for (int i = 0; i < points.Length; ++i) { points[i] += t.position; }
			c.a = 1;
			Gizmos.color = c;
			for (int i = 0; i < points.Length; ++i) {
				Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
			}
		}
#endif
		#endregion // Unity Editor interaction
		[System.Serializable]
		public struct Callbacks
		{
			[Tooltip("When the command line goes into active editing. This may be useful to refresh info for the command line, or disable a 3D FPS controller.")]
			public UnityEngine.Events.UnityEvent whenThisActivates;
			[Tooltip("When the command line leaves active editing. This may be useful to re-enable a 3D FPS controller.")]
			public UnityEngine.Events.UnityEvent whenThisDeactivates;
			[Tooltip("When a command is executed. Check <code>RecentInstruction</code>")]
			public UnityEngine.Events.UnityEvent whenCommandRuns;
			public bool ignoreCallbacks;
		}
		[Tooltip("Recommended callbacks:\n  Global.Pause()\n  Global.Unpause()")]
		public Callbacks callbacks = new Callbacks();

		public void EnsureInit() {
			if(_initailized == false) {
				_initailized = true;
				//Debug.Log("INITING " + this.GetHashCode());
				commander = new Commander { cmdLine = this };
				data = new TTYData();
				data.SetSize(maxLines, commandLineWidth);
				data.SetColors(this.ColorSet);
				data.SetCursorIndex(0, 0);
				if (_instance == null) { _instance = this; }
				showBottomWhenTextIsAdded = true;
				NeedToRefreshUserPrompt = true;
				commander.PopulateBasicCommands();
				PopulateWithBasicCommands();
				ManageUI.SetInteractive(this, ActiveOnStart);
			}
		}

		void Awake() { EnsureInit(); }

		private void Start()
		{
			RefreshText();
		}

		public void MaintainFreshOutput() {
			if (freshnessOfOutput != data.timestamp) {
				freshnessOfOutput = data.timestamp;
				if (NeedToRefreshUserPrompt) {
					if(indexWherePromptWasPrintedRecently != data.cursorIndex) {
						data.WriteOutput(commander.CommandPromptArtifact());
						indexWherePromptWasPrintedRecently = data.cursorIndex;
					}
					NeedToRefreshUserPrompt = false;
				}
				RefreshText();
			}
		}

		void Update()
		{
#if UNITY_EDITOR
			UnityEditorUIUpdates();
#endif
			ManageUI.UpdateUIInteractivity(this);
			commander.ExecuteInstructions();
			commander.UpdateActivationCallbacks();
			MaintainFreshOutput();
		}
	}
}
