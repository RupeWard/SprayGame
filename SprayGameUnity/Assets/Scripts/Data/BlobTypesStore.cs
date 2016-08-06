using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;

public class BlobTypesStore : RJWard.Core.Singleton.SingletonApplicationLifetimeLazy<BlobTypesStore>, RJWard.Core.IDebugDescribable
{
	private static readonly bool DEBUG_BLOBTYPES = true;

	private SqliteUtils.TextColumn idCol_ = new SqliteUtils.TextColumn( "id" );
	private SqliteUtils.ColourColumn colourCol_ = new SqliteUtils.ColourColumn( "colour" );
	private SqliteUtils.TextColumn prefabCol_ = new SqliteUtils.TextColumn( "prefabName" );

	private SqliteUtils.Table standardBlobsTable_ = null;
	private Dictionary<string, BlobTypeStandard> standardBlobDefns_ = new Dictionary<string, BlobTypeStandard>( );

	public BlobType_Base GetBlobType(string n)
	{
		BlobType_Base result = null;
		if (n.StartsWith(BlobTypeStandard.s_typeName_))
		{
			n = n.Replace( BlobTypeStandard.s_typeName_ + "_", "" );
			result = GetBlobTypeStandard( n );
		}
		return result;
	}

	public BlobTypeStandard GetBlobTypeStandard(string n)
	{
		BlobTypeStandard result = null;
		if (standardBlobDefns_.ContainsKey( n ))
		{
			result = standardBlobDefns_[n];
		}
		return result;
	}

	protected override void PostAwake( )
	{
		standardBlobsTable_ = new SqliteUtils.Table( "BlobTypeStandard",
			new List<SqliteUtils.Column>( )
			{
				idCol_,
				colourCol_,
				prefabCol_
			}
			);
		standardBlobDefns_ = GetAllStandardBlobDefns( );

		if (DEBUG_BLOBTYPES)
		{
			Debug.Log( this.DebugDescribe( ) );
		}
	}

	private Dictionary< string, BlobTypeStandard> GetAllStandardBlobDefns()
	{
		Dictionary<string, BlobTypeStandard> result = new Dictionary<string, BlobTypeStandard>( );
		SqliteConnection connection = SqliteUtils.Instance.getConnection( "CoreData" );

		SqliteCommand selectCommand = connection.CreateCommand( );
		selectCommand.CommandText = standardBlobsTable_.GetSelectCommand( );

		SqliteDataReader reader = selectCommand.ExecuteReader( );
		while (reader.Read( ))
		{
			BlobTypeStandard defn = new BlobTypeStandard( );
			defn.name = idCol_.Read( reader, 0 );
			defn.colour = colourCol_.Read( reader, 1 );
			defn.prefabName = prefabCol_.Read( reader, 2 );

			result.Add( defn.name, defn );
		}
		return result;
    }

	public void DebugDescribe(System.Text.StringBuilder sb)
	{
		sb.Append( "BlobTypesStore:" );
		sb.Append( "\n " + standardBlobDefns_.Count + " standard..." );
		foreach (KeyValuePair < string, BlobTypeStandard  > kvp in standardBlobDefns_)
		{
			sb.Append( "\n  " ).Append( kvp.Key ).Append( " = " );
			kvp.Value.DebugDescribe( sb );
		}
	}

}
