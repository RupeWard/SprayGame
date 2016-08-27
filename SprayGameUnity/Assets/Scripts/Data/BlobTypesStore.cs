using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;

public class BlobTypesStore : RJWard.Core.Singleton.SingletonApplicationLifetimeLazy<BlobTypesStore>, RJWard.Core.IDebugDescribable
{
	private static readonly bool DEBUG_BLOBTYPES = true;

	private SqliteUtils.TextColumn nameCol_ = new SqliteUtils.TextColumn( "name" );
	private SqliteUtils.ColourColumn colourCol_ = new SqliteUtils.ColourColumn( "colour" );
	private SqliteUtils.TextColumn prefabCol_ = new SqliteUtils.TextColumn( "prefabName" );

	private SqliteUtils.Table standardBlobsTable_ = null;
	private Dictionary<string, BlobTypeStandard> standardBlobDefnsCore_ = new Dictionary<string, BlobTypeStandard>( );

	public BlobType_Base GetBlobType(string n)
	{
		BlobType_Base result = null;
		if (n.StartsWith(BlobTypeStandard.s_typeName_))
		{
			n = n.Replace( BlobTypeStandard.s_typeName_ + "_", "" );
			result = GetBlobTypeStandard( n );
		}
		else
		{
			Debug.LogError( "Unrecognised blobType identifier in '" + n + "'" );
		}
		return result;
	}

	public BlobTypeStandard GetBlobTypeStandard(string n)
	{
		BlobTypeStandard result = null;
		if (standardBlobDefnsCore_.ContainsKey( n ))
		{
			result = standardBlobDefnsCore_[n];
		}
		return result;
	}

	protected override void PostAwake( )
	{
		standardBlobsTable_ = new SqliteUtils.Table( "BlobTypeStandard",
			new List<SqliteUtils.Column>( )
			{
				nameCol_,
				colourCol_,
				prefabCol_
			}
			);
		standardBlobDefnsCore_ = GetAllStandardBlobDefns("CoreData" );

		if (DEBUG_BLOBTYPES)
		{
			Debug.Log( this.DebugDescribe( ) );
		}
	}

	private Dictionary< string, BlobTypeStandard> GetAllStandardBlobDefns(string db)
	{
		if (!standardBlobsTable_.ExistsInDB( db ))
		{
			Debug.LogWarning( "Table '" + standardBlobsTable_.name + "' doesn't exist in DB '" + db + "'" );
			return null;
		}
		Dictionary<string, BlobTypeStandard> result = new Dictionary<string, BlobTypeStandard>( );
		SqliteConnection connection = SqliteUtils.Instance.getConnection( db );

		SqliteCommand selectCommand = connection.CreateCommand( );
		selectCommand.CommandText = standardBlobsTable_.GetSelectCommand( );

		SqliteDataReader reader = selectCommand.ExecuteReader( );
		while (reader.Read( ))
		{
			BlobTypeStandard defn = new BlobTypeStandard( );
			try
			{
				defn.name = nameCol_.Read( reader, 0 );
				defn.colour = colourCol_.Read( reader, 1 );
				defn.prefabName = prefabCol_.Read( reader, 2 );

				result.Add( defn.name, defn );
			}
			catch (System.InvalidCastException /* e */)
			{
				if (DEBUG_BLOBTYPES)
				{
					Debug.LogWarning( "BlobTypeStandard read error: '" + defn.name + "' '" +defn.colour+"' '"+defn.prefabName+"'" );
				}
			}
		}
		return result;
    }

	public void DebugDescribe(System.Text.StringBuilder sb)
	{
		sb.Append( "BlobTypesStore:" );
		sb.Append( "\n " + standardBlobDefnsCore_.Count + " standard..." );
		foreach (KeyValuePair < string, BlobTypeStandard  > kvp in standardBlobDefnsCore_)
		{
			sb.Append( "\n  " ).Append( kvp.Key ).Append( " = " );
			kvp.Value.DebugDescribe( sb );
		}
	}

}
