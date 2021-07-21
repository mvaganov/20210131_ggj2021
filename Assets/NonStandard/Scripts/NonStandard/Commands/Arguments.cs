using System;
using System.Collections.Generic;
using NonStandard;
using NonStandard.Commands;
using NonStandard.Data;
using NonStandard.Data.Parse;
using NonStandard.Extension;

public class Argument {
	/// <summary>
	/// a short name, and cannonical unique identifier
	/// </summary>
	public string id;
	/// <summary>
	/// a full name for the argument
	/// </summary>
	public string name;
	/// <summary>
	/// description of what the argument does or what it is for
	/// </summary>
	public string description;
	/// <summary>
	/// optionally, what value to generate for this argument if no value is given
	/// </summary>
	public object defaultValue;
	/// <summary>
	/// the type of the value expected. optional if <see cref="defaultValue"/> is provided
	/// </summary>
	public System.Type valueType;
	/// <summary>
	/// if unnamed, the argument should be found in this unnamed slot in the argument listing. 0 is the command, 1 is the 1st argument
	/// </summary>
	public int order = -1;
	/// <summary>
	/// this argument is required. cause an error if the argument is missing. default true if order is greater than zero
	/// </summary>
	public bool required = false;
	/// <summary>
	/// this feature is discouraged, and may be removed soon
	/// </summary>
	public bool deprecated = false;
	/// <summary>
	/// this feature is not entirely finished, and future updates will likely change the way it behaves
	/// </summary>
	public bool preview = false;
	/// <summary>
	/// this argument doesn't have a value beyond being present or absent
	/// </summary>
	public bool flag = false;
	/// <summary>
	/// 
	/// </summary>
	/// <param name="id">a short name, and cannonical unique identifier</param>
	/// <param name="name">a full name for the argument</param>
	/// <param name="description">description of what the argument does or what it is for</param>
	/// <param name="defaultValue">optionally, what value to generate for this argument if no value is given</param>
	/// <param name="type">the type of the value expected. optional if <see cref="defaultValue"/> is provided</param>
	/// <param name="order">if unnamed, the argument should be found in this unnamed slot in the argument listing. 0 is the command, 1 is the 1st argument</param>
	/// <param name="required">this argument is required. cause an error if the argument is missing. default true if order is greater than zero</param>
	/// <param name="deprecated">this feature is discouraged, and may be removed soon</param>
	/// <param name="preview">this feature is not entirely finished, and future updates will likely change the way it behaves</param>
	/// <param name="flag">this argument doesn't have a value beyond being present or absent</param>
	public Argument(string id, string name = null, string description = null, object defaultValue = null, System.Type type = null, int order = -1,
		bool required = false, bool deprecated = false, bool preview = false, bool flag = false) {
		this.id = id;
		this.name = name;
		this.description = description;
		this.defaultValue = defaultValue;
		this.valueType = type;
		this.order = order;
		this.required = required;
		this.deprecated = deprecated;
		this.preview = preview;
		this.flag = flag;
		if (this.valueType == null && this.defaultValue != null) {
			this.valueType = defaultValue.GetType();
		}
		if (this.valueType == null && this.flag) {
			this.valueType = typeof(bool);
		}
	}
}

public class Arguments {
	public static Arguments Parse(Command command, Tokenizer tokenizer, object scriptVariables) {
		Arguments args = new Arguments(command);
		args.Parse(tokenizer, scriptVariables);
		return args;
	}

	public Command command;
	/// <summary>
	/// if there is a list of named or id'd arguments in the Command, this dicitonary will be populated
	/// </summary>
	public Dictionary<string, object> namedValues = new Dictionary<string, object>();
	/// <summary>
	/// if there are unnamed values, this list has them in order. argument zero is null
	/// </summary>
	public List<object> orderedValues = new List<object>();

	public override string ToString() {
		return "Arguments<"+command.Name+">{"+namedValues.Stringify()+","+orderedValues.Stringify()+"}";
	}

	public Arguments(Command command) {
		this.command = command;
	}

	public static int GetArgumentIndex(Command command, string text) {
		Argument[] args = command.arguments;
		for (int i = 0; i < args.Length; ++i) {
			if(args[i].id == text) { return i; }
		}
		for (int i = 0; i < args.Length; ++i) {
			if (args[i].name == text) { return i; }
		}
		return -1;
	}

	public void Parse(Tokenizer tokenizer, object scriptVariables = null) {
		Show.Log(tokenizer);
		//tokenizer = command.Tokenize(text);
		List<Token> tokens = tokenizer.tokens;
		orderedValues.Add(tokenizer.GetStr(0));
		for(int i = 1; i < tokens.Count; ++i) {
			Token tArg = tokens[i];
			Show.Log(tArg+" "+tArg.IsSimpleString+" "+tArg.IsDelim);
			int argIndex = tArg.IsSimpleString || tArg.IsDelim ? GetArgumentIndex(command, tArg.ToString()) : -1;
			if (argIndex >= 0) {
				Argument arg = command.arguments[argIndex];
				if (arg.flag) {
					namedValues[arg.id] = true;
				} else {
					++i;
					if (i < tokens.Count) {
						Token tValue = tokens[i];
						Type type = arg.valueType;
						object result = null;
						// TODO find out why this isn't parsing correctly with "help-chelp 0"
						if (CodeConvert.TryParseTokens(type, tokens.GetRange(i, 1), ref result, scriptVariables, tokenizer)) {
						//if (CodeConvert.TryParseTokens(type, tokenizer, i, ref result, scriptVariables)) {
							namedValues[arg.id] = result;
						} else {
							tokenizer.AddError(tValue.index, "Could not cast (" + type.Name + ") from " + tValue.ToString());
						}
					} else {
						tokenizer.AddError(tArg.index, "expected value for argument \"" + tArg.ToString() + "\"");
					}
				}
			} else {
				orderedValues.Add(tokenizer.GetResolvedToken(i, scriptVariables));
			}
		}
	}
}
