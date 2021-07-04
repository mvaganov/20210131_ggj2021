using System;
using System.Collections.Generic;

public class Incident {
	public readonly long Timestamp;
	public readonly object Source;
	public readonly object Detail;
	public readonly string Identifier;

	public Incident(long timestamp, string identifier, object source, object detail) {
		Timestamp = timestamp;
		Identifier = identifier;
		Source = source;
		Detail = detail;
	}
	public Incident(string identifier, object source = null, object detail = null) {
		Timestamp = Procedure.GetTime();
		Identifier = identifier;
		Source = source;
		Detail = detail;
	}
}
