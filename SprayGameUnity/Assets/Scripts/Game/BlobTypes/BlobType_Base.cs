using UnityEngine;
using System.Collections;

[System.Serializable]
abstract public class BlobType_Base :RJWard.Core.IDebugDescribable
{
	abstract public string typeName();

	public string name;

	abstract public GameObject Instantiate( );

	abstract public void SetConnectorAppearance( BlobConnector_SimpleCylinder connector );
	abstract public void SetBlobAppearance( Blob_SimpleCylinder blob);
	abstract public void SetCannonAppearance( Cannon c );

	abstract protected void DebugDescribeSub( System.Text.StringBuilder sb);
	public void DebugDescribe( System.Text.StringBuilder sb )
	{
		sb.Append( "[" ).Append( typeName( ) ).Append( ": " );
		DebugDescribeSub( sb );
		sb.Append( "]" );
	}

	abstract protected string GetSubDefnString( );
	public string GetDefnString( )
	{
		return typeName( ) + ":" + name + ":" + GetSubDefnString( );
	}

	abstract public bool ShouldDeleteGroupOfNum( int n );
}
