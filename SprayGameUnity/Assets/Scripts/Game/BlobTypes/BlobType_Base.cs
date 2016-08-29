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

	static public void SetConnectorAppearance( BlobType_Base bb0, BlobType_Base bb1, BlobConnector_SimpleCylinder connector )
	{
		Color c0 = Color.white;
		Color c1 = Color.white;
		if (bb0 as BlobTypeStandard != null)
		{
			c0 = (bb0 as BlobTypeStandard).colour;
		}
		else
		{
			Debug.LogWarning( "c0 is of type " + c0.GetType( ) );
		}
		if (bb1 as BlobTypeStandard != null)
		{
			c1 = (bb1 as BlobTypeStandard).colour;
		}
		else
		{
			Debug.LogWarning( "c1 is of type " + c0.GetType( ) );
		}
		connector.cachedMaterial.SetColor( "_Color1", c0 );
		connector.cachedMaterial.SetColor( "_Color2", c1 );
		connector.cachedMaterial.SetFloat( "_Alpha", 0.25f );
	}

}
