using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class DataHelpers
{
	private static bool DEBUG_LOCAL = false;

	public static readonly string FLOAT_REGEX = @"[-+]?[0-9]*\.?[0-9]+";

	static public List< Vector3 > extractVector3s(ref string str)
	{
		List< Vector3> result = new List<Vector3> ();
		Vector3 v = Vector3.zero;

		while (extractOptionalVector3( ref str, ref v))
		{
			result.Add(v);
		}
		return result;
	}

#region Vector4
	
	static public bool extractRequiredVector4(ref string str, ref Vector4 v)
	{
		return extractVector4(ref str, ref v, true);
	}
	
	static public bool extractOptionalVector4(ref string str, ref Vector4 v)
	{
		return extractVector4(ref str, ref v, false);
	}
	
	static public bool extractVector4(ref string str, ref Vector4 v, bool required)
	{
		bool success = false;
		System.Text.RegularExpressions.Regex regex = 
			new System.Text.RegularExpressions.Regex ( @"^(\(("+FLOAT_REGEX+@"),\s*("+FLOAT_REGEX+@"),\s*("+FLOAT_REGEX+@"),\s*("+FLOAT_REGEX+@")\))");
		System.Text.RegularExpressions.Match match = regex.Match (str);
		if (match.Success && match.Groups.Count == 6)
		{
			string matched = match.Groups[1].Value;
			string sx = match.Groups[2].Value;
			string sy = match.Groups[3].Value;
			string sz = match.Groups[4].Value;
			string ss = match.Groups[5].Value;
			//			string remainder = match.Groups[4].Value;
			float x, y, z, s;
			if (float.TryParse(sx, out x) && float.TryParse(sy, out y) && float.TryParse(sz, out z) && float.TryParse(ss, out s))
			{
				v = new Vector4(x,y,z,s);
				string prevStr = str;
				str = str.Replace(matched,"");
				if (DEBUG_LOCAL)
				{
					Debug.Log ("Extracted vector 4 from '"+prevStr+"' = "+v+"\nRemainder = '"+str+"'");
				}
				success = true;
			}
			else 
			{
				Debug.LogError("Couldn't parse floats from '"+sx+"' '"+sy+"' '"+sz+"' '"+ss+"'");
			}
		} 
		else
		{
			if (required)
			{
				Debug.LogError ("Furniture: Couldn't parse Vector4 from '"+str+"'");
			}
		}
		return success;
	}
	
#endregion Vector4
	


#region Vector3

	static public bool extractRequiredVector3(ref string str, ref Vector3 v)
	{
		return extractVector3(ref str, ref v, true);
	}

	static public bool extractOptionalVector3(ref string str, ref Vector3 v)
	{
		return extractVector3(ref str, ref v, false);
	}

	static public bool extractVector3(ref string str, ref Vector3 v, bool required)
	{
		bool success = false;
		System.Text.RegularExpressions.Regex regex = 
			new System.Text.RegularExpressions.Regex ( @"^(\(("+FLOAT_REGEX+@"),\s*("+FLOAT_REGEX+@"),\s*("+FLOAT_REGEX+@")\))");
		System.Text.RegularExpressions.Match match = regex.Match (str);
		if (match.Success && match.Groups.Count == 5)
		{
			string matched = match.Groups[1].Value;
			string sx = match.Groups[2].Value;
			string sy = match.Groups[3].Value;
			string sz = match.Groups[4].Value;
			//			string remainder = match.Groups[4].Value;
			float x, y, z;
			if (float.TryParse(sx, out x) && float.TryParse(sy, out y) && float.TryParse(sz, out z))
			{
				v = new Vector3(x,y,z);
				string prevStr = str;
				str = str.Replace(matched,"");
				if (DEBUG_LOCAL)
				{
					Debug.Log ("Extracted vector 3 from '"+prevStr+"' = "+v+"\nRemainder = '"+str+"'");
				}
				success = true;
			}
			else 
			{
				Debug.LogError("Couldn't parse floats from '"+sx+"' '"+sy+"' '"+sz+"'");
			}
		} 
		else
		{
			if (required)
			{
				Debug.LogError ("Furniture: Couldn't parse Vector3 from '"+str+"'");
			}
		}
		return success;
	}

#endregion Vector3

#region Vector2

	static public bool extractRequiredVector2(ref string str, ref Vector2 v)
	{
		return extractVector2(ref str, ref v, true);
	}
	
	static public bool extractOptionalVector2(ref string str, ref Vector2 v)
	{
		return extractVector2(ref str, ref v, false);
	}

	static public bool extractVector2(ref string str, ref Vector2 v, bool required)
	{
		bool success = false;
		System.Text.RegularExpressions.Regex regex = 
			new System.Text.RegularExpressions.Regex ( @"^(\(("+FLOAT_REGEX+@"),\s*("+FLOAT_REGEX+@")\))");
		System.Text.RegularExpressions.Match match = regex.Match (str);
		if (match.Success && match.Groups.Count == 4)
		{
			string matched = match.Groups[1].Value;
			string sx = match.Groups[2].Value;
			string sy = match.Groups[3].Value;
			//			string remainder = match.Groups[3].Value;
			float x, y;
			if (float.TryParse(sx, out x) && float.TryParse(sy, out y))
			{
				string prevStr = str;
				str = str.Replace(matched, "");
				v = new Vector2(x,y);
				if (DEBUG_LOCAL)
				{
					Debug.Log ("Extracted Vector2 from '"+prevStr+" = "+v+"\nRemainder = '"+str+"'");
				}
				success = true;
			}
			else 
			{
				Debug.LogError("Couldn't parse floats from '"+sx+"' '"+sy+"'");
			}
		} 
		else
		{
			if (required)
			{
				Debug.LogError ("Furniture: Couldn't parse Vector2 from '"+str+"'");
			}
		}
		return success;
	}
	
#endregion Vector2

}
