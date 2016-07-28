using System;

namespace RJWard.Core
{
	public interface IDebugDescribable
	{
		void DebugDescribe( System.Text.StringBuilder sb );
	}

	/*
	public static class DebugDescribeExtensions
	{
		private static System.Text.StringBuilder sb = new System.Text.StringBuilder( );
		public static string DebugDescribe(this IDebugDescribable obj)
		{
			sb.Length = 0;
			obj.DebugDescribe( sb );
			return sb.ToString( );
		}
	}*/
}

public static class DebugDescribeExtensions
{
	private static System.Text.StringBuilder sb = new System.Text.StringBuilder( );
	public static string DebugDescribe( this RJWard.Core.IDebugDescribable obj )
	{
		sb.Length = 0;
		obj.DebugDescribe( sb );
		return sb.ToString( );
	}

	public static void DebugDescribe( this System.Text.StringBuilder sb, RJWard.Core.IDebugDescribable dd )
	{
		sb.Append( (dd == null) ? ("NULL") : (dd.DebugDescribe( )) );
	}

}

