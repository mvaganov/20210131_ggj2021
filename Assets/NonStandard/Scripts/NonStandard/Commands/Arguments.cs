using System.Collections.Generic;
using NonStandard;
using NonStandard.Commands;
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
	public static Arguments Parse(string text, Command command) {
		Arguments args = new Arguments(command);
		args.ParseText(text);
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

	public void ParseText(string text) {
		Tokenizer tokenizer = new Tokenizer();
		tokenizer.Tokenize(text);
		Show.Log(tokenizer.tokens.Stringify());
	}
}
