public static class StringExtension
{
	public static string RemoveFromFront(this string str, string trimMe) {
		if (str.StartsWith(trimMe)) { return str.Substring(trimMe.Length); }
		return str;
	}

}
