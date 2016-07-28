using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class SettingsIds
{
	static public readonly string versionNumber = "versionNumber";
	static public readonly string showFPS = "showFPS";

	public static readonly Dictionary<string, string> defaults = new Dictionary<string, string>( )
	{
        {  showFPS, "0" },
	};

	public static readonly List<string> encrypted = new List<string>( )
	{
	};
}
