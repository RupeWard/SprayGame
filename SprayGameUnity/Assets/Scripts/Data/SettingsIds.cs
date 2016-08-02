using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class SettingsIds
{
	static public readonly string versionNumber = "versionNumber";
	static public readonly string showFPS = "showFPS";
	static public readonly string cannonSpeed = "cannonSpeed";
	static public readonly string blobSlowDistance = "blobSlowDistance";
	static public readonly string blobSlowFactor = "blobSlowFactor";
	static public readonly string numBlobs = "numBlobs";

	public static readonly Dictionary<string, string> defaults = new Dictionary<string, string>( )
	{
        {  showFPS, "0" },
		{  cannonSpeed, "6" },
		{  blobSlowDistance, "4" },
		{  blobSlowFactor, "0.5" },
		{ numBlobs, "4" },
	};

	public static readonly List<string> encrypted = new List<string>( )
	{
	};
}
