using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// given a current state, a list of these can be applied to get a previous state at an earlier date
/// </summary>
public class Modification {
	/// <summary>
	/// name of the modification
	/// </summary>
	public string name;
	/// <summary>
	/// the old value. likely a float, but it could be a string, or a Token (script fragment). if missing, the time references creation time & all initial values
	/// </summary>
	public object value;
	/// <summary>
	/// when the old value was changed. YYYYMMDDTHHMMSS. eg: 20211003T122939 is about 12:30 on October 3rd, 2021
	/// </summary>
	public DateTime time;
	/// <summary>
	/// what value we're talking about here. stored as simple string, which is lighter weight than the Token it is parsed into. if missing, the time references creation time & all initial values
	/// </summary>
	public string edit;
}

public class Task {
	/// <summary>
	/// a unique identifier, can be referenced
	/// </summary>
	public string id;
	/// <summary>
	/// a reference to another task. the other task may not be defined yet.
	/// </summary>
	public string ptr;
	///// <summary>
	///// a descriptive name
	///// </summary>
	//public string name;
	//public string description;
	//public Color32 color;
	//public Sprite icon;
	/// <summary>
	/// used for assigning additional variables and flags. name, description, color, icon, cost, progress
	/// </summary>
	public Dictionary<string, object> variables;
	/// <summary>
	/// which tasks are required to finish before this can start
	/// </summary>
	public List<Task> reqStart;
	/// <summary>
	/// which tasks require this one to finish before they can start (inverse of reqStart)
	/// </summary>
	public List<Task> reqFinish;
	/// <summary>
	/// child-tasks/sub-tasks/component-tasks. these need to finish before this one can finish
	/// </summary>
	public List<Task> task;
	/// <summary>
	/// parent tasks. need to start before this task can start.
	/// </summary>
	public List<Task> parents;
	///// <summary>
	///// expected cost by resource
	///// </summary>
	//public Dictionary<string, float> cost;
	///// <summary>
	///// how much has been paid by resource
	///// </summary>
	//public Dictionary<string, float> progress;
	/// <summary>
	/// change log, used for book-keeping, including the burndown chart
	/// </summary>
	public List<Modification> log;
}
