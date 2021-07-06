using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NonStandard.Cli {
	public class CmdLine : CmdLine_base {
		[TextArea(2, 8)]
		public string firstCommands = "";

		public UnityEngine.Object workingObject = null;

		public object Show { get; private set; }

		public Transform[] ChildTransforms(Transform parent) {
			List<Transform> transforms = new List<Transform>();
			if(parent == null) {
				foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType(typeof(GameObject))) {
					if (obj.transform.parent == null) {
						transforms.Add(obj.transform);
					}
				}
			} else {
				for(int i = 0; i < parent.childCount; ++i) {
					transforms.Add(parent.GetChild(i));
				}
			}
			return transforms.ToArray();
		}
		public Transform GetChildTransform(string name) {
			Transform workingTransform = workingObject as Transform;
			if (workingObject == null || workingTransform != null) {
				Transform[] t = ChildTransforms(workingTransform);
				for (int i = 0; i < t.Length; ++i) {
					if (t[i].name == name) {
						return t[i];
					}
				}
			}
			return null;
		}
		public MonoBehaviour GetChildMonobehavior(string name, int index = 0) {
			Transform workingTransform = workingObject as Transform;
			if (workingTransform) {
				MonoBehaviour[] mb = workingTransform.GetComponents<MonoBehaviour>();
				int found = 0;
				for (int i = 0; i < mb.Length; ++i) {
					if (mb[i].GetType().Name == name) {
						if (found == index) { return mb[i]; }
						found++;
					}
				}
			}
			return null;
		}

		// TODO pwd, print MonoBehaviours (in a different color) in ls, erase, copy, rename, cd into MonoBehaviours, ls prints public variables and methods, var creates public variables, ./ runs methods

		/// <summary>example of how to populate the command-line with commands</summary>
		public override void PopulateWithBasicCommands() {
			//When adding commands, you must add a call below to registerCommand() with its name, implementation method, and help text.
			commander.addCommand("help", (args, user) => {
				SetForegroundColor(ColorSet.SpecialText);
				Log(commander.CommandHelpString());
				UnsetForegroundColor();
			}, "prints this help.");
			commander.addCommand("cd", (args, user) => {
				if (args.Length > 1) {
					if(args[1] == ".." && workingObject != null) {
						if(workingObject is Transform) {
							workingObject = (workingObject as Transform).parent;
						} else if(workingObject is MonoBehaviour) {
							workingObject = (workingObject as MonoBehaviour).transform;
						}
					}  else {
						Transform childTransform = GetChildTransform(args[1]);
						if (childTransform != null) { workingObject = childTransform; }
						// BECAUSE multiple MonoBehaviors of the same type can be on a transform
						int index = 0; if(args.Length > 2) { index = int.Parse(args[2]); }
						MonoBehaviour mb = GetChildMonobehavior(args[1], index);
						if(mb != null) { workingObject = mb; }
					}
				}
			}, "changes the current working object");
			commander.addCommand("ls", (args, user) => {
				SetForegroundColor(ColorSet.SpecialText);
				Transform[] t = null;
				Transform workingTransform = workingObject as Transform;
				if (workingTransform != null || workingObject == null) {
					t = ChildTransforms(workingTransform);
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					Array.ForEach(t, (e) => {
						if (sb.Length > 0) sb.Append(", ");
						sb.Append(e.name.Stringify(true));
					});
					if(sb.Length > 0) {
						Log(sb.ToString());
					}
					if (workingObject != null) { // if it is an Transform, not the root dir
						MonoBehaviour[] mb = (workingObject as Transform).GetComponents<MonoBehaviour>();
						sb.Clear();
						Array.ForEach(mb, (e) => {
							if (sb.Length > 0) sb.Append(", ");
							// TODO print index if this type has been appended already
							sb.Append(e.GetType().Name.Stringify(true));
						});
						if (sb.Length > 0) {
							Log(sb.ToString());
						}
					}
				} else if (workingObject is MonoBehaviour) {
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					System.Reflection.MemberInfo[] m = workingObject.GetType().GetMembers();
					Array.ForEach(m, (e) => {
						if (sb.Length > 0) sb.Append(", ");
						sb.Append(e.Name.Stringify(true));
					});
					Log(sb.ToString());
				}	
				UnsetForegroundColor();
			}, "prints traversable objects");
			commander.addCommand("clear", (args, user) => {
				data.Clear();
			}, "clears the command-line terminal.");
			commander.addCommand("echo", (args, user) => {
				Log(string.Join(" ", args, 1, args.Length - 1));
			}, "repeat given arguments as output");
			commander.addCommand("load", (args, user) => {
				if (args.Length > 1) {
					if (args[1] == ".") { args[1] = SceneManager.GetActiveScene().name; }
					SceneManager.LoadScene(args[1]);
				} else {
					Log("to reload current scene, type \"load " + SceneManager.GetActiveScene().name+"\"");
				}
			}, "loads given scene. use: load <scene name>");
			commander.addCommand("pref", (args, user) => {
				bool didSomething = false;
				if (args.Length > 1) {
					didSomething = true;
					switch (args[1]) {
						case "?": case "-?": case "help":
							Log("pref set <key> [value]       : sets pref value\n" +
								"pref get [-v] [<key> ...]    : prints pref value\n" +
								"          -v                 : only return values, no keys\n" +
								"pref reset                   : clears all pref values"); break;
						case "get":
							bool v = false;
							if (Array.IndexOf(args, "-v") >= 0) {
								v = true;
								int i = Array.IndexOf(args, "-v");
								for (; i < args.Length - 1; ++i) {
									args[i] = args[i + 1];
								}
								Array.Resize(ref args, args.Length - 1);
							}
							for (int i = 2; i < args.Length; ++i) {
								string output = null;
								try { output = PlayerPrefs.GetString(args[i]); if (!v) { output = "\"" + output + "\""; } } catch (Exception e) { output = v ? "" : "<#" + ColorSet.ErrorTextHex + ">" + e + "</color>"; }
								if (output == null) { output = v ? "" : "<#" + ColorSet.ErrorTextHex + ">null</color>"; }
								if (v) { Log(output); } else { Log(args[i] + ":" + output); }
							}
							break;
						case "set":
							if (args.Length > 2) {
								PlayerPrefs.SetString(args[2], (args.Length > 3) ? args[3] : null);
								PlayerPrefs.Save();
							} else { Log("missing arguments"); }
							break;
						case "reset": PlayerPrefs.DeleteAll(); PlayerPrefs.Save(); break;
						default: didSomething = false; break;
					}
				}
				if (!didSomething) {
					Log("use \"pref ?\" for more details");
				}
			}, "interfaces with player prefs. use \"pref ?\" for more details");
			commander.addCommand("exit", (args, user) => {
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
				Application.OpenURL(webplayerQuitURL);
#else
				Application.Quit();
#endif
			}, "quits this application");
			commander.addCommand("DoActivateTrigger", (args, user) => {
				if(args.Length == 1) {
					Log("DoActivateTrigger <TargetName> [<TypeName:MonoBehavior>]");
				} else {
					GameObject obj = GameObject.Find(args[1]);
					if (args.Length > 2) {
						MonoBehaviour mb = obj.GetComponent(args[2]) as MonoBehaviour;
						if(mb != null) { mb.Invoke("DoActivateTrigger",0); }
					} else {
						obj.SendMessage("DoActivateTrigger");
					}
				}
			}, "DoActivateTrigger a given GameObject/MonoBehaviour");
		}
		public void Start()
		{
			string timestamp = ""+Resources.Load("app_build_time");
			Log(Application.productName + ", v" + Application.version+", @"+timestamp);
			//data.SetCursorIndex(5, 15);
			//Log("Hello World!");
			//data.SetCursorIndex(5, 2);
			//Write("...");

			//char nbsp = (char)0xA0;
			//string o = "";
			//o += "  0 1 2 3 4 5 6 7 8 9 A B C D E F\n";
			//for(int row = 0; row < 16; ++row) {
			//	o += row.ToString("X1")+" ";
			//	for(int col = 0; col < 16; ++col){
			//		int c = row * 16 + col;
			//		switch(c){ case 0: case 10: case 13: c = 32; break; }
			//		o += ((char)c).ToString() + nbsp.ToString();// " ";
			//	}
			//	o += "\n";
			//}
			//Log(o);

			if (!string.IsNullOrEmpty(firstCommands))
			{
			data.input.WriteInput(firstCommands);
			}
		}
	}
}