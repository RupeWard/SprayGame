using UnityEngine;
using System.Collections;

namespace RJWard.Core
{
	public static class CSharpExtensions
	{

#region Numeric

		public static bool EqualsApprox( this double d, double other, double tolerance )
		{
#if UNITY_EDITOR
			if (tolerance < 0)
			{
				Debug.LogWarning( "Negative tolerance!" );
				tolerance *= -1;
			}
#endif
			return System.Math.Abs( other - d ) <= tolerance;
		}

		public static bool EqualsApprox( this float d, float other, float tolerance )
		{
#if UNITY_EDITOR
			if (tolerance < 0f)
			{
				Debug.LogWarning( "Negative tolerance!" );
				tolerance *= -1f;
			}
#endif
			return Mathf.Abs( other - d ) <= tolerance;
		}

		public static bool GreaterThanApprox( this float d, float other, float tolerance )
		{
#if UNITY_EDITOR
			if (tolerance < 0f)
			{
				Debug.LogWarning( "Negative tolerance!" );
				tolerance *= -1f;
			}
#endif
			return d > (other + tolerance);
		}

		public static bool LessThanApprox( this float d, float other, float tolerance )
		{
#if UNITY_EDITOR
			if (tolerance < 0f)
			{
				Debug.LogWarning( "Negative tolerance!" );
				tolerance *= -1f;
			}
#endif
			return d < (other - tolerance);
		}

		public static bool EqualsApprox( this Vector3 v, Vector3 other, float tolerance )
		{ 
#if UNITY_EDITOR
			if (tolerance < 0f)
			{
				Debug.LogWarning( "Negative tolerance!" );
				tolerance *= -1f;
			}
#endif
			return (Vector3.Distance( v, other ) < tolerance);
		}

		public static double DLerp( double from, double to, double fraction )
		{
			if (fraction < 0 || fraction > 1)
			{
				Debug.LogError( "Out of range at " + fraction );
			}
			return from + (to - from) * ((fraction < 0) ? (0) : ((fraction > 1) ? (1) : (fraction)));
		}

		public static double DLerpFree( double from, double to, double fraction )
		{
			return from + (to - from) * fraction;
		}

		public static float LerpFree( float from, float to, float fraction )
		{
			return from + (to - from) * fraction;
		}



		#endregion Numeric

		/*
		#region StringBuilder

		public static void DebugDescribe( this System.Text.StringBuilder sb, IDebugDescribable dd )
		{
			sb.Append( (dd == null) ? ("NULL") : (dd.DebugDescribe( )) );
		}

		public static string DebugDescribe( this IDebugDescribable dd )
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder( );
			dd.DebugDescribe( sb );
			return sb.ToString( );
		}

#endregion StringBuilder
		*/

		#region Vector

		public static void Set( this Vector2 v, Vector2 other )
		{
			v.Set( other.x, other.y );
		}

#endregion Vector

	}
}

