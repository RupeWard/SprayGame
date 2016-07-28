using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;
using System.Collections.Generic;

public class SettingsStore
{
	public static void storeSetting ( string id, int value )
	{
		storeSetting( id, value.ToString() );
	}
	
	public static void storeSetting( string id, uint value )
	{
		storeSetting( id, value.ToString() );
	}
	
	public static void storeSetting( string id, float value )
	{
		storeSetting( id, value.ToString() );
	}
	
	public static void storeSetting( string id, bool value )
	{
		storeSetting( id, value ? "1" : "0" );
	}

	public static void storeSetting( string id, string value )
	{
		if( SettingsIds.encrypted.Contains( id ) )
		{
			Debug.LogError( "Encryption not implemented" );// See SS Code below

			//			value = EncryptionHelper.EncryptString( id + value );
		}
		SqliteConnection connection = SqliteUtils.Instance.getConnection("Progress");
		SqliteCommand insert = connection.CreateCommand();
		insert.CommandText = "REPLACE INTO settings ( id, value ) VALUES ( ?, ? )";
		insert.Parameters.Add( SqliteUtils.newSqlParameter(id) );
		insert.Parameters.Add( SqliteUtils.newSqlParameter(value) );
		insert.ExecuteNonQuery();
		insert.Dispose();
	}

	public static T retrieveSetting<T>( string id )
	{
		SqliteConnection connection = SqliteUtils.Instance.getConnection("Progress");
		SqliteCommand query = connection.CreateCommand();
		query.CommandText = "SELECT value FROM settings WHERE id=?";
		query.Parameters.Add( SqliteUtils.newSqlParameter(id) );
		SqliteDataReader reader = query.ExecuteReader();
		
		object value = null;
		if( reader.Read() )
		{
			string valueString = reader.GetString(0);
			if( SettingsIds.encrypted.Contains( id ) )
			{
				Debug.LogError( "Encryption not implemented" );// See SS Code below
/*
				valueString = EncryptionHelper.DecryptString( valueString );
				if( valueString.Length >= id.Length && valueString.Substring( 0, id.Length ) == id )
				{
					valueString = valueString.Substring( id.Length );
				}
				else
				{
					valueString = "";
				}
*/
			}

			if ( typeof(T) == typeof(string) )
			{
				value = valueString;
			}
			else if( typeof(T) == typeof(uint) )
			{
				value = valueString == "" ? 0 : uint.Parse( valueString );
			}
			else if( typeof(T) == typeof(int) )
			{
				value = valueString == "" ? 0 : int.Parse( valueString );
			}
			else if( typeof(T) == typeof(float) )
			{
				value = valueString == "" ? 0.0f : float.Parse( valueString );
			}
			else if( typeof(T) == typeof(bool) )
			{
				value = ( valueString == "1" );
			}
		}
		else
		{
			if( typeof(T) == typeof(uint) )
			{
				value = 0;
			}
			else if( typeof(T) == typeof(int) )
			{
				value = 0;
			}
			else if( typeof(T) == typeof(float) )
			{
				value = 0.0f;
			}
			else if( typeof(T) == typeof(bool) )
			{
				value = false;
			}
		}
		
		reader.Close();
		query.Dispose();
		return (T)value;
	}

	public static bool retrieveSetting<T>( string id, ref T outValue )
	{
		bool result = true;

		SqliteConnection connection = SqliteUtils.Instance.getConnection("Progress");
		SqliteCommand query = connection.CreateCommand();
		query.CommandText = "SELECT value FROM settings WHERE id=?";
		query.Parameters.Add( SqliteUtils.newSqlParameter(id) );
		SqliteDataReader reader = query.ExecuteReader();
		
		object value = null;
		if( reader.Read() )
		{
			string valueString = reader.GetString(0);
			if( SettingsIds.encrypted.Contains( id ) )
			{
				Debug.LogError( "Encryption not implemented" );// See SS Code below

				/*
				valueString = EncryptionHelper.DecryptString( valueString );
				if( valueString.Length >= id.Length && valueString.Substring( 0, id.Length ) == id )
				{
					valueString = valueString.Substring( id.Length );
				}
				else
				{
					valueString = "";
				}*/
			}

			if (valueString.Length == 0)
			{
				result = false;
			}
			else
			{
				if( typeof(T) == typeof(string) )
				{
					value = valueString;
				}
				else if( typeof(T) == typeof(uint) )
				{
					value = uint.Parse( valueString );
				}
				else if( typeof(T) == typeof(int) )
				{
					value = int.Parse( valueString );
				}
				else if( typeof(T) == typeof(float) )
				{
					value = float.Parse( valueString );
				}
				else if( typeof(T) == typeof(bool) )
				{
					value = ( valueString == "1" );
				}
				outValue = (T)value;
				result = true;
			}
		}
		else
		{
			result = false;
		}
		
		reader.Close();
		query.Dispose();
		return result;
	}

	public static void deleteSetting( string id )
	{
		SqliteConnection connection = SqliteUtils.Instance.getConnection("Progress");
		SqliteCommand query = connection.CreateCommand();
		query.CommandText = "DELETE FROM settings WHERE id = ?";
		query.Parameters.Add( SqliteUtils.newSqlParameter(id) );
		query.ExecuteNonQuery();
		query.Dispose();
	}
	
}
