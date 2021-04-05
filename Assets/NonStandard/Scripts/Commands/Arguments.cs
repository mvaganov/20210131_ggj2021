using System.Collections;
using System.Collections.Generic;
using NonStandard.Commands;
using UnityEngine;

public class Argument {
	public string id;
	public string name;
	public string description;
	public object defaultValue;
	public System.Type type;
	public int order = -1;
	public bool required = false;
}

public class Arguments {
	public static Arguments Parse(string text, Command command) {
		Arguments args = new Arguments(command);
		args.Parse(text);
		return args;
	}

	public Command command;
	/// <summary>
	/// if there is a list of named or id'd arguments in the Command, this dicitonary will be populated
	/// </summary>
	public Dictionary<string, object> namedValues;
	/// <summary>
	/// if there are unnamed values, this list has them in order. argument zero is null
	/// </summary>
	public List<object> orderedValues;

	public Arguments(Command command) {
		this.command = command;
	}

	public void Parse(string text) {
		// TODO
	}
}
