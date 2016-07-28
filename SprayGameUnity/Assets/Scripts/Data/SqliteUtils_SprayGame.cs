using UnityEngine;
using System;
using System.IO;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;

public partial class SqliteUtils : RJWard.Core.Singleton.SingletonApplicationLifetimeLazy< SqliteUtils >
{

	private void prepareProgressTable( )
	{
		//		Debug.Log ("prepareProgressTable");
		createSettingsTable( );
	}

	private void checkProgressTableDefaults( )
	{
		//		Debug.Log ("prepareProgressTable");
		SqliteConnection connection = getConnection( "Progress" );

		SqliteCommand insert = connection.CreateCommand( );
		insert.CommandText = "INSERT OR IGNORE INTO settings ( id, value ) VALUES ( ?, ? )";
		SqliteParameter idSql = new SqliteParameter( );
		SqliteParameter valueSql = new SqliteParameter( );
		insert.Parameters.Add( idSql );
		insert.Parameters.Add( valueSql );

		foreach (KeyValuePair<string, string> entry in SettingsIds.defaults)
		{
			idSql.Value = entry.Key;
			if (SettingsIds.encrypted.Contains( entry.Key ))
			{
				Debug.LogError( "Encryption not implemented" ); // See SS Code below
																//				valueSql.Value = EncryptionHelper.EncryptString( entry.Key + entry.Value );
			}
			else
			{
				valueSql.Value = entry.Value;
			}
			insert.ExecuteNonQuery( );
		}
		insert.Dispose( );
	}

	private void createSettingsTable( )
	{
		SqliteConnection connection = getConnection( "Progress" );
		SqliteCommand create = connection.CreateCommand( );
		create.CommandText = "CREATE TABLE IF NOT EXISTS settings (" +
			"id TEXT PRIMARY KEY, " +
				"value TEXT " +
				")";

		create.ExecuteNonQuery( );
		create.Dispose( );

		SqliteCommand insert = connection.CreateCommand( );
		insert.CommandText = "INSERT OR IGNORE INTO settings ( id, value ) VALUES ( ?, ? )";
		SqliteParameter idSql = new SqliteParameter( );
		SqliteParameter valueSql = new SqliteParameter( );
		insert.Parameters.Add( idSql );
		insert.Parameters.Add( valueSql );

		foreach (KeyValuePair<string, string> entry in SettingsIds.defaults)
		{
			idSql.Value = entry.Key;
			if (SettingsIds.encrypted.Contains( entry.Key ))
			{
				Debug.LogError( "Encryption not implemented" ); // See SS Code below
//				valueSql.Value = EncryptionHelper.EncryptString( entry.Key + entry.Value );
			}
			else
			{
				valueSql.Value = entry.Value;
			}
			insert.ExecuteNonQuery( );
		}
		insert.Dispose( );

		SqliteCommand replace = connection.CreateCommand( );
		replace.CommandText = "REPLACE INTO settings ( id, value ) VALUES ( ?, ? )";
		SqliteParameter id2Sql = new SqliteParameter( );
		SqliteParameter value2Sql = new SqliteParameter( );
		replace.Parameters.Add( id2Sql );
		replace.Parameters.Add( value2Sql );

		id2Sql.Value = SettingsIds.versionNumber;
		value2Sql.Value = RJWard.Core.Version.versionNumber.ToString();
		replace.ExecuteNonQuery( );

		//		Debug.Log("Set DB version number to "+AppConfig.version.ToString());

		replace.Dispose( );
	}



}
